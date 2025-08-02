using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Loop Settings")]
    private float loopDuration;
    private int maxClones;
    public GameObject playerPrefab;
    private Transform spawnPoint;

    private int clonesSpawned = 0;
    private float timer = 0f;
    private bool timerRunning = false;
    private bool recordingComplete = false;
    private List<Recorder.InputFrameData> pendingRecording = null;

    private Recorder playerRecorder;
    private PlayerController3D playerController;
    private GameObject playerInstance;

    private List<List<Recorder.InputFrameData>> allRecordings = new List<List<Recorder.InputFrameData>>();
    private List<GameObject> clones = new List<GameObject>();

    private bool playerInStartZone = true;
    private bool canStartNextRecording = true;

    // Method to receive level settings from StartLevel
    public void SetLevelSettings(float duration, int maxClonesCount)
    {
        loopDuration = duration;
        maxClones = maxClonesCount;
        Debug.Log($"Level settings updated: loopDuration={loopDuration}, maxClones={maxClones}");
    }

    // Method to set spawn point from StartLevel
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
        Debug.Log($"Spawn point updated to: {spawnPoint.position}");
    }

    // Method to clear all data when entering a start zone for the first time
    public void ClearAllData()
    {
        Debug.Log("Clearing all recording data and clones for new start zone");
        
        // Clear all clones
        ClearAllClones();
        
        // Clear all recordings
        allRecordings.Clear();
        
        // Reset recording state
        canStartNextRecording = true;
        timerRunning = false;
        recordingComplete = false;
        pendingRecording = null;
        clonesSpawned = 0;
        
        // Clear player recorder data
        if (playerRecorder != null)
        {
            playerRecorder.recordedInputs.Clear();
            playerRecorder.ResetReplayIndex();
            playerRecorder.mode = Recorder.Mode.Recording;
        }
        
        // Re-enable player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    void Start()
    {
        playerInstance = GameObject.FindWithTag("Player");
        if (playerInstance != null)
        {
            playerRecorder = playerInstance.GetComponent<Recorder>();
            playerController = playerInstance.GetComponent<PlayerController3D>();
        }
    }

    void Update()
    {
        HandleInput();
        HandleRecordingTimer();
    }

    void HandleInput()
    {
        // L key - Return to spawn or reset clones
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            if (recordingComplete && !playerInStartZone)
            {
                // Return to spawn when recording is complete and player is outside start zone
                TeleportPlayerToSpawn();

            }
            else if (playerInStartZone && clones.Count > 0)
            {
             StartPlayback();
            }

        }
        // R key - Reset level completely
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetLevel();
        }
    }

    void HandleRecordingTimer()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f && clones.Count < maxClones && !playerInStartZone)
            {
                CompleteRecording();
            }
        }
    }

    void CompleteRecording()
    {
        timerRunning = false;
        recordingComplete = true;
        playerRecorder.StopRecording();

        // Store the recording but don't spawn clone yet
        pendingRecording = new List<Recorder.InputFrameData>(playerRecorder.recordedInputs);
        
        Debug.Log($"Recording completed. Player recorder has {playerRecorder.recordedInputs.Count} frames");
        Debug.Log($"Pending recording now has {pendingRecording.Count} frames");

        // Disable player movement
        if (playerController != null)
        {
            playerController.enabled = false;
        }

    }

    void StartPlayback()
    {

        Debug.Log($"Starting playback with {clones.Count} clones");

        // Reset all clones to start position first
        ResetClonesToStart();

        // Start all clones replaying
        foreach (var clone in clones)
        {
            if (clone != null)
            {
                Recorder cloneRecorder = clone.GetComponent<Recorder>();
                if (cloneRecorder != null)
                {
                    Debug.Log($"Starting replay for clone with {cloneRecorder.recordedInputs.Count} frames");
                    cloneRecorder.ResetReplayIndex();
                    cloneRecorder.StartReplaying();
                }
                else
                {
                    Debug.LogWarning("Clone has no Recorder component!");
                }
            }
        }
    }

    void TeleportPlayerToSpawn()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is null! Cannot teleport player.");
            return;
        }

        CharacterController controller = playerInstance.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            playerInstance.transform.position = spawnPoint.position;
            playerInstance.transform.rotation = spawnPoint.rotation;
            controller.enabled = true;
        }
        else
        {
            playerInstance.transform.position = spawnPoint.position;
            playerInstance.transform.rotation = spawnPoint.rotation;
        }

        // Re-enable player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Spawn the clone now that player is back at spawn
        if (pendingRecording != null)
        {
            StartPlayback();
            
            SpawnClone(pendingRecording);
            clonesSpawned++;
            pendingRecording = null;
            
            // Clear player recorder data AFTER clone is spawned and has its own copy
            if (playerRecorder != null)
            {
                playerRecorder.recordedInputs.Clear();
                playerRecorder.ResetReplayIndex();
                playerRecorder.mode = Recorder.Mode.Recording;
            }
        }
        else
        {
            Debug.Log("No pending recording to spawn clone from");
        }

        // Reset recording state for next recording
        recordingComplete = false;
        canStartNextRecording = true;
    }

    public void PlayerExitedStart()
    {
        // Only start recording if we can start next recording and haven't reached max clones
        if (canStartNextRecording && !timerRunning && clones.Count < maxClones)
        {
            Debug.Log("Starting new recording session");
            
            // Reset clone count for new recording cycle
            clonesSpawned = 0;
            
            canStartNextRecording = false;
            playerRecorder.StartRecording();
            timerRunning = true;
            timer = loopDuration;
            recordingComplete = false;

        }
        else
        {
            Debug.Log($"Cannot start recording: canStartNextRecording={canStartNextRecording}, timerRunning={timerRunning}, clones.Count={clones.Count}, maxClones={maxClones}");
        }
    }

    private void ClearAllClones()
    {
        Debug.Log($"Clearing {clones.Count} old clones");
        foreach (var clone in clones)
        {
            if (clone != null)
                Destroy(clone);
        }
        clones.Clear();
        clonesSpawned = 0;
    }

    private void SpawnClone(List<Recorder.InputFrameData> inputData)
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is null! Cannot spawn clone.");
            return;
        }

        Debug.Log($"SpawnClone called with {inputData.Count} frames");
        if (inputData.Count > 0)
        {
            Debug.Log($"Input data first frame: {inputData[0].position}, last frame: {inputData[inputData.Count - 1].position}");
        }
        
        GameObject clone = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Recorder cloneRecorder = clone.GetComponent<Recorder>();

        // Ensure the clone recorder is properly set up
        cloneRecorder.enabled = true;
        
        // Create a deep copy of the recorded data
        List<Recorder.InputFrameData> clonedData = new List<Recorder.InputFrameData>();
        foreach (var frame in inputData)
        {
            clonedData.Add(new Recorder.InputFrameData
            {
                horizontal = frame.horizontal,
                vertical = frame.vertical,
                jump = frame.jump,
                position = frame.position,
                rotation = frame.rotation
            });
        }
        
        Debug.Log($"Deep copy created with {clonedData.Count} frames");
        
        cloneRecorder.recordedInputs = clonedData;
        cloneRecorder.mode = Recorder.Mode.Replaying;
        cloneRecorder.ResetReplayIndex(); // Ensure replay index is reset

        clones.Add(clone);
        
        Debug.Log($"Clone #{clonesSpawned + 1} spawned with {clonedData.Count} recorded frames at position {spawnPoint.position}");
        Debug.Log($"Clone recorder mode: {cloneRecorder.mode}, enabled: {cloneRecorder.enabled}");
        
        // Verify the clone has the correct data
        if (clonedData.Count > 0)
        {
            Debug.Log($"First frame position: {clonedData[0].position}, Last frame position: {clonedData[clonedData.Count - 1].position}");
        }
        else
        {
            Debug.LogWarning("Clone spawned with no recorded data!");
        }
    }

    private void ResetClonesToStart()
    {
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is null! Cannot reset clones.");
            return;
        }

        Debug.Log($"Resetting {clones.Count} clones to start position");

        foreach (var clone in clones)
        {
            if (clone == null) continue;

            Recorder cloneRecorder = clone.GetComponent<Recorder>();
            if (cloneRecorder != null)
            {
                // Stop any current replay first
                cloneRecorder.StopReplaying();
                
                // Move clone to spawn position
                clone.transform.position = spawnPoint.position;
                clone.transform.rotation = spawnPoint.rotation;
                
                Debug.Log($"Reset clone {clone.name} to position {spawnPoint.position}");
            }
        }
    }

    private void ResetLevel()
    {
        // Clear all clones
        ClearAllClones();
        allRecordings.Clear();
        canStartNextRecording = true;
        timerRunning = false;
        recordingComplete = false;
        pendingRecording = null;

        // Reset player
        TeleportPlayerToSpawn();
        if (playerRecorder != null)
        {
            playerRecorder.recordedInputs.Clear();
            playerRecorder.ResetReplayIndex();
            playerRecorder.mode = Recorder.Mode.Recording;
        }

    }

    // Called from your StartZone trigger script to keep track if player is inside
    public void SetPlayerInStartZone(bool isInside)
    {
        playerInStartZone = isInside;
        
        Debug.Log($"Player in start zone: {isInside}, canStartNextRecording: {canStartNextRecording}, timerRunning: {timerRunning}, clones.Count: {clones.Count}");
        
        // Handle recording state based on player location
        if (playerRecorder != null)
        {
            if (isInside)
            {
                // Disable recording when entering start zone
                playerRecorder.DisableRecording();
                Debug.Log("Recording disabled - player entered start zone");
            }
            else
            {
                // Enable recording when leaving start zone (only if we should be recording)
                if (canStartNextRecording && !timerRunning && clones.Count < maxClones)
                {
                    playerRecorder.EnableRecording();
                    Debug.Log("Recording enabled - player left start zone");
                }
                else
                {
                    Debug.Log($"Recording NOT enabled - conditions not met: canStartNextRecording={canStartNextRecording}, timerRunning={timerRunning}, clones.Count={clones.Count}");
                }
            }
        }
    }

    private void StopAllClones()
    {
        Debug.Log("Stopping all clones from current replay");
        foreach (var clone in clones)
        {
            if (clone == null) continue;
            // find all objects with the tag "Clone"
            GameObject[] globalclones = GameObject.FindGameObjectsWithTag("Clone");
            foreach (var globalclone in globalclones)
            {
                Recorder cloneRecorder = globalclone.GetComponent<Recorder>();
                // set the frameindex to 0
                cloneRecorder.StartReplaying();
            }
        }
    }
}
