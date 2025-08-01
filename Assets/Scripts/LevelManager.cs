using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Loop Settings")]
    public float loopDuration = 10f;
    public int maxClones = 3;
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public float cloneStartDelay = 3f;

    [Header("UI")]
    public GameObject promptUI; // Assign a UI element to show prompts

    private int clonesSpawned = 0;
    private float timer = 0f;
    private bool timerRunning = false;
    private bool recordingComplete = false;
    private bool isPlayingBack = false;
    private List<Recorder.InputFrameData> pendingRecording = null;

    private Recorder playerRecorder;
    private PlayerController3D playerController;
    private GameObject playerInstance;

    private List<List<Recorder.InputFrameData>> allRecordings = new List<List<Recorder.InputFrameData>>();
    private List<GameObject> clones = new List<GameObject>();

    private bool playerInStartZone = true;
    private bool canStartNextRecording = true;

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
                ShowPrompt("Press L to start clone replay, then leave start zone for next recording");
            }
            else if (playerInStartZone)
            {
                // Reset all clones to spawn and start replay when in start zone
                if (clonesSpawned > 0)
                {
                    ResetClonesToStart();
                    StartPlayback();
                }
                else
                {
                    ShowPrompt("No clones to replay. Leave start zone to begin recording.");
                }
            }
        }

        // P key - Play back the sequence (kept for compatibility)
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (playerInStartZone && clonesSpawned > 0)
            {
                ResetClonesToStart();
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

            if (timer <= 0f && clonesSpawned < maxClones && !playerInStartZone)
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

        ShowPrompt("Press L to return to spawn");
    }

    void StartPlayback()
    {
        if (isPlayingBack) return;

        isPlayingBack = true;
        ShowPrompt("Playing back sequence...");

        Debug.Log($"Starting playback with {clones.Count} clones");

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

        // Start a coroutine to reset playback state after completion
        StartCoroutine(ResetPlaybackAfterDelay());
    }

    IEnumerator ResetPlaybackAfterDelay()
    {
        // Wait for the longest recording to complete
        float maxDuration = 0f;
        foreach (var clone in clones)
        {
            if (clone != null)
            {
                Recorder cloneRecorder = clone.GetComponent<Recorder>();
                if (cloneRecorder != null)
                {
                    float cloneDuration = cloneRecorder.recordedInputs.Count * Time.fixedDeltaTime;
                    maxDuration = Mathf.Max(maxDuration, cloneDuration);
                }
            }
        }

        yield return new WaitForSeconds(maxDuration + 1f); // Add 1 second buffer
        isPlayingBack = false;
        ShowPrompt("Press L to replay or leave start zone for next recording");
    }

    void TeleportPlayerToSpawn()
    {
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
            Debug.Log($"Spawning clone with {pendingRecording.Count} recorded frames");
            Debug.Log($"Pending recording data check - First frame: {pendingRecording[0].position}, Last frame: {pendingRecording[pendingRecording.Count - 1].position}");
            
            // Clear old clones when spawning a new one
            ClearAllClones();
            
            SpawnClone(pendingRecording);
            clonesSpawned++;
            pendingRecording = null;
        }
        else
        {
            Debug.Log("No pending recording to spawn clone from");
        }

        // Reset recording state for next recording
        recordingComplete = false;
        canStartNextRecording = true;
        
        // Reset player recorder for next recording - but preserve the recorded data first
        if (playerRecorder != null)
        {
            // Clear the recorded inputs after spawning the clone
            playerRecorder.recordedInputs.Clear();
            playerRecorder.ResetReplayIndex();
            playerRecorder.mode = Recorder.Mode.Recording;
        }
    }

    public void PlayerExitedStart()
    {
        // Only start recording if we can start next recording and haven't reached max clones
        if (canStartNextRecording && !timerRunning && clonesSpawned < maxClones)
        {
            Debug.Log("Starting new recording session");
            
            // Reset clone count for new recording cycle
            clonesSpawned = 0;
            
            canStartNextRecording = false;
            playerRecorder.StartRecording();
            timerRunning = true;
            timer = loopDuration;
            recordingComplete = false;
            ShowPrompt("Recording started...");
        }
        else
        {
            Debug.Log($"Cannot start recording: canStartNextRecording={canStartNextRecording}, timerRunning={timerRunning}, clonesSpawned={clonesSpawned}, maxClones={maxClones}");
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

    public void PlayerEnteredEnd()
    {
        timerRunning = false;
        recordingComplete = false;
        playerRecorder.StopRecording();

        // Re-enable player movement
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        foreach (var clone in clones)
        {
            if (clone != null)
                Destroy(clone);
        }

        clones.Clear();
        allRecordings.Clear();
        clonesSpawned = 0;
        canStartNextRecording = true;
        pendingRecording = null;

        ShowPrompt("Level Complete!");
    }

    private void SpawnClone(List<Recorder.InputFrameData> inputData)
    {
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

        // Disable clone camera if present
        Camera cam = clone.GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        // Disable clone movement controller
        PlayerController3D cloneController = clone.GetComponent<PlayerController3D>();
        if (cloneController != null) cloneController.enabled = false;

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
        Debug.Log($"Resetting {clones.Count} clones to start position");
        
        foreach (var clone in clones)
        {
            if (clone == null) continue;

            Recorder cloneRecorder = clone.GetComponent<Recorder>();
            if (cloneRecorder != null)
            {
                clone.transform.position = spawnPoint.position;
                clone.transform.rotation = spawnPoint.rotation;
                cloneRecorder.ResetReplayIndex();
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
        isPlayingBack = false;
        pendingRecording = null;

        // Reset player
        TeleportPlayerToSpawn();
        if (playerRecorder != null)
        {
            playerRecorder.recordedInputs.Clear();
            playerRecorder.ResetReplayIndex();
            playerRecorder.mode = Recorder.Mode.Recording;
        }

        ShowPrompt("Level reset. Leave start zone to begin recording.");
    }

    // Called from your StartZone trigger script to keep track if player is inside
    public void SetPlayerInStartZone(bool isInside)
    {
        playerInStartZone = isInside;
        
        Debug.Log($"Player in start zone: {isInside}, canStartNextRecording: {canStartNextRecording}, timerRunning: {timerRunning}, clonesSpawned: {clonesSpawned}");
        
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
                if (canStartNextRecording && !timerRunning && clonesSpawned < maxClones)
                {
                    playerRecorder.EnableRecording();
                    Debug.Log("Recording enabled - player left start zone");
                }
                else
                {
                    Debug.Log($"Recording NOT enabled - conditions not met: canStartNextRecording={canStartNextRecording}, timerRunning={timerRunning}, clonesSpawned={clonesSpawned}");
                }
            }
        }
    }

    void ShowPrompt(string message)
    {
        UIPrompt.ShowPrompt(message);
    }
}
