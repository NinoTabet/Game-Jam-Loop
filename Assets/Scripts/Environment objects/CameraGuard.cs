using UnityEngine;

public class CameraGuard : ActivatableItem
{
    private GameObject followTarget;
    protected bool playerNotSeen = true;
    public Quaternion originalTransformRotation;
    
    void Awake(){
        originalTransformRotation = transform.parent.rotation;
    }

    void OnTriggerStay(Collider other){
        if(other.gameObject.tag == "Player" || other.gameObject.tag == "Clone"){
             transform.parent.LookAt(other.gameObject.transform);
        }
    }

    void OnTriggerEnter(Collider other){
        if(other.gameObject.tag == "Player"){
            PlayerSeenCheck();
        }
    }

    void OnTriggerExit(Collider other){
        if(other.gameObject.tag == "Player"){
            PlayerSeenCheck();
        }
        if(other.gameObject.tag == "Clone" || other.gameObject.tag == "Player"){
            transform.parent.rotation = originalTransformRotation;
        }
    }

    public void PlayerSeenCheck(){
        playerNotSeen = !playerNotSeen;
        IsActive = playerNotSeen;
    }
}
