using UnityEngine;

public class CameraGuard : ActivatableItem
{    
    [Header("Guard Settings")]
    [SerializeField] private Material playerSeenMaterial;
    [SerializeField] private Material playerNotSeenMaterial;
    
    private MeshRenderer meshRenderer;
    private Transform playerTransform;
    private GameObject followTarget;
    protected bool playerNotSeen = true;
    public Quaternion originalTransformRotation;
    
    void Awake(){
        originalTransformRotation = transform.parent.rotation;
        IsActive = playerNotSeen;
        meshRenderer = GetComponent<MeshRenderer>();
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
        meshRenderer.material = playerNotSeen ? playerNotSeenMaterial : playerSeenMaterial;
    }
}
