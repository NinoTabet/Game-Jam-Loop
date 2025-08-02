using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Object Counts")]
    public int numberOfButtons = 0;
    public int numberOfPressurePlates = 0;
    public int numberOfCameras = 0;
    
    private int buttonTrueCount = 0;

    [Header("Button Objects")]
    public Button[] buttons;
    
    [Header("Pressure Plate Objects")]
    public PressurePlate[] pressurePlates;
    
    [Header("Camera Objects")]
    public Transform[] cameras;

    private Vector3 startingPosition;
    private Button lastKnownButton = null; // Track the last known button reference
    private bool isDoorOpen = false; // Track door state
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Only initialize arrays if they haven't been set up yet
        if (buttons == null) {
            buttons = new Button[numberOfButtons];
        }
        if (pressurePlates == null) {
            pressurePlates = new PressurePlate[numberOfPressurePlates];
        }
        if (cameras == null) {
            cameras = new Transform[numberOfCameras];
        }
        startingPosition = transform.position;
    }

    void Update(){
        Debug.Log("Update called - numberOfButtons: " + numberOfButtons + ", buttons array length: " + (buttons != null ? buttons.Length : "null"));
        
        // Check if button reference is lost
        if (buttons != null && buttons.Length > 0) {
            if (buttons[0] != null && lastKnownButton == null) {
                lastKnownButton = buttons[0];
                Debug.Log("Button reference found: " + buttons[0].name);
            }
            else if (buttons[0] == null && lastKnownButton != null) {
                Debug.Log("WARNING: Button reference was lost! Button was: " + lastKnownButton.name);
                lastKnownButton = null;
            }
        }
        
        bool shouldBeOpen = AllButtonsActive() && AllPressurePlatesActive();
        
        if (shouldBeOpen && !isDoorOpen){
            Debug.Log("All Buttons Active: " + AllButtonsActive());
            Debug.Log("AllPressurePlatesActive: " + AllPressurePlatesActive());
            DoorOpen();
        }
        else if (!shouldBeOpen && isDoorOpen){
            Debug.Log("Conditions no longer met, closing door");
            DoorClose();
        }
    }

    //checks if all the buttons are currently active
    bool AllButtonsActive(){
        Debug.Log("AllButtonsActive called");
        if (buttons == null) {
            Debug.Log("buttons array is null");
            return false;
        }
        
        Debug.Log("Checking " + buttons.Length + " buttons");
        buttonTrueCount = 0; // Reset counter each time
        
        for (int i = 0; i < buttons.Length; i++) {
            Debug.Log("Checking button[" + i + "]: " + (buttons[i] != null ? buttons[i].name : "NULL"));
            
            if (buttons[i] == null) {
                Debug.Log("Found null button at index " + i + ", skipping");
                continue; // Skip null buttons
            }

            Debug.Log("button activation: "+buttons[i].isActivated);
            if (buttons[i].isActivated){
                buttonTrueCount++;
                Debug.Log("Button activated, count: " + buttonTrueCount);
            }else{
                Debug.Log("Button not activated, returning false");
                return false;
            }
        }
        
        Debug.Log("Final buttonTrueCount: " + buttonTrueCount + ", numberOfButtons: " + numberOfButtons);
        if (buttonTrueCount == numberOfButtons) {
            Debug.Log("All buttons active, returning true");
            return true;
        } else {
            Debug.Log("Not all buttons active, returning false");
            return false;
        }
    }

    // checks if all the pressure plates are active
    bool AllPressurePlatesActive(){
        if (pressurePlates == null) return false;
        foreach (PressurePlate pressurePlate in pressurePlates){
            if (pressurePlate == null) continue; // Skip null pressure plates
            if (!pressurePlate.isActivated){
                continue;
            }else{
                return false;
            }
        }
        return true;
    }

    void DoorOpen(){
        Debug.Log("Door Open");
        //transform door up by 100
        transform.position = startingPosition + new Vector3(0, 5, 0);
        isDoorOpen = true;
    }

    void DoorClose(){
        //transform door down by 100
        transform.position = startingPosition;
        isDoorOpen = false;
    }
}
