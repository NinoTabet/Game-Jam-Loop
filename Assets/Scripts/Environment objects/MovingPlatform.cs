using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform endingTransform;
    [SerializeField] private Transform platform;
    private Vector3 startingPosition;
    private Vector3 endingPosition => endingTransform.position;
    private bool movingForward = true;

    [Header("Activatable Items")]
    [SerializeField] private ActivatableItem[] activatableItems;

    [Header("Current State")]
    [SerializeField] private bool hasAccess = false;

    void Start()
    {
        startingPosition = platform.position;

        foreach (var item in activatableItems)
        {
            if (item != null)
            {
                // Subscribe to the activation changed event
                item.onActivationChanged += (OnActivatableItemChanged);
            }
        }

        // Check initial access state
        CheckAccess();
    }

    void FixedUpdate(){
        if(hasAccess){
            if (movingForward){
                if (Vector3.Distance(platform.position, endingPosition) > 0.1f){
                    platform.position = Vector3.Lerp(platform.position, endingPosition, Time.deltaTime * 1f);
                }else{
                    movingForward = false;
                }
            }else{
                if (Vector3.Distance(platform.position, startingPosition) > 0.1f){
                    platform.position = Vector3.Lerp(platform.position, startingPosition, Time.deltaTime * 1f);
                }else{
                    movingForward = true;
                }
            }
        }
    }

    void OnDestroy()
    {
        foreach (var item in activatableItems)
        {
            item.onActivationChanged -= (OnActivatableItemChanged);
        }
    }

    // Check if access should be granted based on activatable items
    public void CheckAccess()
    {
        if (activatableItems == null || activatableItems.Length == 0)
        {
            hasAccess = true;
            return;
        }

        // All items must be active
        bool newAccessState = true;
        foreach (var item in activatableItems)
        {
            if (item != null && !item.IsActive)
            {
                newAccessState = false;
                break;
            }
        }

        hasAccess = newAccessState;

    }

    protected void OnActivatableItemChanged(bool isActive)
    {
        CheckAccess();
    }
}
