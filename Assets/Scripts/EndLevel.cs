using UnityEngine;

public class EndLevel : MonoBehaviour
{
    public LevelManager levelManager;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the end trigger
        if (other.CompareTag("Player"))
        {
            levelManager.PlayerEnteredEnd();
            Debug.Log("Player entered end area - loops stopped, level complete.");
        }
    }
}
