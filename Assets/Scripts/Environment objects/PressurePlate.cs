using UnityEngine;
using UnityEngine.InputSystem;

public class PressurePlate : ActivatableItem
{
    [Header("Pressure Plate Settings")]
    public bool isActivated = false;
    
    [Header("Detection")]
    public LayerMask playerLayer = -1; // Default to all layers
    
    private int objectsOnPlate = 0;

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame){
            isActivated = false;
            IsActive = false;
        }
        // Check if any object (player or clone) is on the plate
        if (objectsOnPlate > 0 && !isActivated)
        {
            ActivatePlate();
        }
        else if (objectsOnPlate == 0 && isActivated)
        {
            DeactivatePlate();
        }
        
    }
    
    void ActivatePlate()
    {
        isActivated = true;
        IsActive = true;
    }
    
    void DeactivatePlate()
    {
        isActivated = false;
        IsActive = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectsOnPlate++;
            Debug.Log("Player stepped on pressure plate");
        }
        else if (other.CompareTag("Clone"))
        {
            objectsOnPlate++;
            Debug.Log("Clone stepped on pressure plate");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            objectsOnPlate--;
            Debug.Log("Player stepped off pressure plate");
        }
        else if (other.CompareTag("Clone"))
        {
            objectsOnPlate--;
            Debug.Log("Clone stepped off pressure plate");
        }
        else if (other == null)
        {
            // Handle case where object is destroyed while on plate
            objectsOnPlate = 0;
        }
    }
}
