using UnityEngine;
using UnityEngine.InputSystem;

public class Guard : MonoBehaviour
{
    [Header("Guard Settings")]
    
    private Transform playerTransform;
    private Transform parentTransform; // The parent object to rotate
    public Transform cameraTransform;
    private Quaternion originalCameraRotation;
    private bool playerSeen = false;
    
    void Start()
    {

        // Get the parent transform (the whole camera assembly)
        parentTransform = transform.parent;
        originalCameraRotation = cameraTransform.rotation;

        if (parentTransform == null)
        {
            Debug.LogError("Guard script must be on a child object with a parent!");
            return;
        }
        
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("Player found, camera will follow player");
        }
        else
        {
            Debug.LogError("No player found with 'Player' tag!");
        }
    }
    
    void Update(){
        if (Keyboard.current.rKey.wasPressedThisFrame){
            cameraTransform.rotation = originalCameraRotation;
            playerTransform = null;
            playerSeen = false;
        }

    }

    void OnTriggerEnter(Collider other){
        if (other.gameObject.tag == "Player"){
            playerSeen = true;
        }
        playerTransform = other.transform;

    }

    void OnTriggerStay()
    {
        if (playerTransform != null && cameraTransform != null)
        {
            FollowPlayer();
        }
    }

    void OnTriggerExit(Collider other){
        cameraTransform.rotation = originalCameraRotation;
        Debug.Log("Guard returning to original position");
        if (other.gameObject.tag == "Player"){
            playerSeen = false;
        }
        playerTransform = null;
    }

    void FollowPlayer()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = (playerTransform.position - cameraTransform.position).normalized;
        
        // Create rotation to look at player
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        
        // Apply smooth rotation
        cameraTransform.rotation = targetRotation;
    }
    
    // Optional: Visualize the line to player in the editor
    void OnDrawGizmosSelected()
    {
        if (parentTransform == null || playerTransform == null) return;
        
        // Draw line to player
        Gizmos.color = Color.red;
        Gizmos.DrawLine(parentTransform.position, playerTransform.position);
    }
}
