using UnityEngine;

public class AccessChecker : MonoBehaviour
{
    [Header("Activatable Items")]
    [SerializeField] private ActivatableItem[] activatableItems;
    
    [Header("Access Settings")]
    [SerializeField] private bool requireAllItems = true; // If true, all items must be active. If false, any item being active is sufficient
    
    [Header("Current State")]
    [SerializeField] private bool hasAccess = false;
    
    // Unity event that can be assigned in inspector
    public UnityEngine.Events.UnityEvent<bool> onAccessChanged;
    
    protected bool newAccessState = false;
    
    void Start()
    {

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
    
    // Called when any activatable item changes state
    protected virtual void OnActivatableItemChanged(bool isActive)
    {
        Debug.Log("OnActivatableItemChanged MEOW: " + isActive);
        CheckAccess();
    }
    
    // Check if access should be granted based on activatable items
    public void CheckAccess()
    {
        if (activatableItems == null || activatableItems.Length == 0)
        {
            SetAccess(true); // No items to check, grant access
            return;
        }
        
        bool newAccessState;
        
        if (requireAllItems)
        {
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
        }
        else
        {
            // Any item being active is sufficient
            newAccessState = false;
            foreach (var item in activatableItems)
            {
                if (item != null && item.IsActive)
                {
                    newAccessState = true;
                    break;
                }
            }
        }
        
        SetAccess(newAccessState);
    }
    
    // Set access state and trigger event
    private void SetAccess(bool access)
    {
        if (hasAccess != access)
        {
            hasAccess = access;
            onAccessChanged?.Invoke(hasAccess);
        }
    }
    
    // Public property to check current access state
    public bool HasAccess => hasAccess;
    
    // Public method to manually check access (useful for external calls)
    public void RefreshAccess()
    {
        CheckAccess();
    }
}
