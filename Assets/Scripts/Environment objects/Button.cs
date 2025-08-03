using UnityEngine;
using UnityEngine.InputSystem;

public class Button : ActivatableItem
{
    [Header("Button Settings")]
    public bool isActivated = false;
    public float activationDuration = 5f;
    
    [Header("Detection")]
    public LayerMask playerLayer = -1; // Default to all layers
    
    private float activationTimer = 0f;
    private bool playerInRange = false;
    private bool cloneInRange = false;
    private bool playerPressed = false;

    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame){
            playerPressed = false;
        }
        // Check for E key press when player or clone is in range
        if (cloneInRange && playerPressed && !isActivated)
        {
            Debug.Log("Clone pressed button");
            ActivateButton();
        }
        else if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame){
            Debug.Log("Player pressed button");
            playerPressed = true;
            ActivateButton();
        }

        // Handle activation timer
        if (isActivated)
        {
            activationTimer -= Time.deltaTime;
            Debug.Log("activationTimer: " + activationTimer);
            if (activationTimer <= 0f)
            {
                DeactivateButton();
            }
        }
    }
    
    void ActivateButton()
    {
        isActivated = true;
        activationTimer = activationDuration;
        Debug.Log($"Button activated for {activationDuration} seconds!");
        IsActive = true;
    }
    
    void DeactivateButton()
    {
        isActivated = false;
        cloneInRange = false;
        activationTimer = 0f;
        Debug.Log("Button deactivated!");
        IsActive = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered button range");
        }
        else if (other.CompareTag("Clone"))
        {
            cloneInRange = true;
            Debug.Log("Clone entered button range");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited button range");
        }
        else if (other.CompareTag("Clone"))
        {
            cloneInRange = false;
            Debug.Log("Clone exited button range");
        }else if (other == null){
            cloneInRange = false;
            playerPressed = false;
        }
    }
}
