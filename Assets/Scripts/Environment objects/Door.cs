using UnityEngine;

public class Door : MonoBehaviour
{

    private Vector3 startingPosition;
    private bool isDoorOpen = false; // Track door state

    [Header("Activatable Items")]
    [SerializeField] private ActivatableItem[] activatableItems;

    [Header("Current State")]
    [SerializeField] private bool hasAccess = false;

    protected bool newAccessState = false;

    void Start()
    {
        startingPosition = transform.position;

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

        bool newAccessState;

        // All items must be active
        newAccessState = true;
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

    // Public property to check current access state
    public bool HasAccess => hasAccess;

    // Public method to manually check access (useful for external calls)
    public void RefreshAccess()
    {
        CheckAccess();
    }

    protected void OnActivatableItemChanged(bool isActive)
    {
        CheckAccess();
        Debug.Log("Button pressed MEOW");
        Debug.Log("Is active state MEOW: ");
        if (hasAccess){
            DoorOpen();
        }else if (transform.position != startingPosition && isDoorOpen){
            DoorClose();
        }
    }
    void DoorOpen(){
        //transform door up by 100
        Debug.Log("Door Open MEOW");
        transform.position = startingPosition + new Vector3(0, 5, 0);
        isDoorOpen = true;
    }

    void DoorClose(){
        //transform door down by 100
        transform.position = startingPosition;
        isDoorOpen = false;
    }
}
