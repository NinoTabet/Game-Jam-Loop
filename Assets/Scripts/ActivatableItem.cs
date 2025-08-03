using UnityEngine;
using UnityEngine.Events;
using System;

public class ActivatableItem : MonoBehaviour
{
    public event Action<bool> onActivationChanged;
    
    [Header("Activation State")]
    [SerializeField] private bool isActive = false;
    
    // Public property to get/set activation state
    public bool IsActive 
    { 
        get { return isActive; }
        set 
        { 
            isActive = value;
            onActivationChanged?.Invoke(isActive);
        }
    }
}
