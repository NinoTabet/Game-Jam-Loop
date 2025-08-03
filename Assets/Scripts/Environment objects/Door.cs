using UnityEngine;

public class Door : MonoBehaviour
{

    private Vector3 startingPosition;
    private bool isDoorOpen = false; // Track door state

    [Header("Activatable Items")]
    [SerializeField] private ActivatableItem[] activatableItems;

    [Header("Current State")]
    [SerializeField] private bool hasAccess = false;

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
