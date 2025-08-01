using UnityEngine;

public class StartLevel : MonoBehaviour
{
    public LevelManager levelManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            levelManager.SetPlayerInStartZone(true);
            Debug.Log("Player entered start area.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            levelManager.SetPlayerInStartZone(false);
            levelManager.PlayerExitedStart();
            Debug.Log("Player exited start area - loop timer started.");
        }
    }
}
