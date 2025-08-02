using UnityEngine;

public class StartLevel : MonoBehaviour
{
    [Header("Level Settings")]
    public float loopDuration;
    public int maxClones;
    public bool firstTime = true;

    [Header("References")]
    public LevelManager levelManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (firstTime){
                // Clear all recording data and clones for new start zone
                levelManager.ClearAllData();
                firstTime = false;
            }
            // Set this start zone as the spawn point
            levelManager.SetSpawnPoint(transform);
            
            // Pass the level settings to the level manager
            levelManager.SetLevelSettings(loopDuration, maxClones);
            
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
