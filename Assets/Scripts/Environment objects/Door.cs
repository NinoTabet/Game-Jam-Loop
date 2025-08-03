using UnityEngine;

public class Door : AccessChecker
{

    private Vector3 startingPosition;
    private bool isDoorOpen = false; // Track door state
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPosition = transform.position;
    }

    protected override void OnActivatableItemChanged(bool isActive){
        Debug.Log("Button pressed MEOW");
        Debug.Log("Is active state MEOW: " + isActive);
        base.OnActivatableItemChanged(isActive);
        if (newAccessState){
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
