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

    private int clonesSpawned = 0;
    private float timer = 0f;
    private bool timerRunning = false;

    private Recorder playerRecorder;
    private GameObject playerInstance;

    private int recordingIndex = 0;
    private List<List<Recorder.InputFrameData>> allRecordings = new List<List<Recorder.InputFrameData>>();
    private List<GameObject> clones = new List<GameObject>();

    private bool playerInStartZone = true;

    void Start()
    {
        playerInstance = GameObject.FindWithTag("Player");
        if (playerInstance != null)
            playerRecorder = playerInstance.GetComponent<Recorder>();
    }

    void Update()
    {
        if (timerRunning)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f && clonesSpawned < maxClones && !playerInStartZone)
            {
                SpawnClone(playerRecorder.recordedInputs);
                clonesSpawned++;

                // Teleport player back to spawn point
                CharacterController controller = playerInstance.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;  // Disable to safely teleport
                    playerInstance.transform.position = spawnPoint.position;
                    controller.enabled = true;
                }
                else
                {
                    playerInstance.transform.position = spawnPoint.position;
                }

                timer = loopDuration;  // Reset timer for next clone
            }
        }

        // Press L to reset clones and optionally teleport player
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            if (!playerInStartZone)
            {
                TeleportPlayerToSpawn();
            }
            ResetPlayerAndClones();
        }
    }

    void TeleportPlayerToSpawn()
    {
        CharacterController controller = playerInstance.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            playerInstance.transform.position = spawnPoint.position;
            controller.enabled = true;
        }
        else
        {
            playerInstance.transform.position = spawnPoint.position;
        }
    }

    public void PlayerExitedStart()
    {
        // Only start recording if timer is NOT running AND player is outside start zone AND haven't reached max clones
        if (!timerRunning && playerInStartZone == false && clonesSpawned < maxClones)
        {
            playerRecorder.StartRecording();
            timerRunning = true;
            timer = loopDuration;
        }
    }

    public void PlayerEnteredEnd()
    {
        timerRunning = false;
        playerRecorder.StopRecording();

        foreach (var clone in clones)
        {
            if (clone != null)
                Destroy(clone);
        }

        clones.Clear();
        allRecordings.Clear();
        recordingIndex = 0;

        // (Optional) Start next level logic here
    }

    private void SpawnClone(List<Recorder.InputFrameData> inputData)
    {
        GameObject clone = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Recorder cloneRecorder = clone.GetComponent<Recorder>();

        cloneRecorder.enabled = true;
        cloneRecorder.recordedInputs = inputData;
        cloneRecorder.mode = Recorder.Mode.Replaying;

        // Disable clone camera if present
        Camera cam = clone.GetComponentInChildren<Camera>();
        if (cam != null) cam.enabled = false;

        clones.Add(clone);
        StartCoroutine(StartCloneReplayAfterDelay(cloneRecorder));
    }

    private IEnumerator StartCloneReplayAfterDelay(Recorder cloneRecorder)
    {
        yield return new WaitForSeconds(cloneStartDelay);
        cloneRecorder.StartReplaying();
    }

    // Called from your StartZone trigger script to keep track if player is inside
    public void SetPlayerInStartZone(bool isInside)
    {
        playerInStartZone = isInside;
    }

    private void ResetPlayerAndClones()
    {
        if (playerInstance != null)
        {
            // Teleport player only if NOT inside start zone
            if (!playerInStartZone)
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
            }

            // Reset player recorder (clear recordings and start fresh)
            playerRecorder.recordedInputs.Clear();
            playerRecorder.ResetReplayIndex();
            playerRecorder.mode = Recorder.Mode.Recording;
            playerRecorder.StartRecording();

            // Reset all clones to spawn and restart their replay
            foreach (var clone in clones)
            {
                if (clone == null) continue;

                Recorder cloneRecorder = clone.GetComponent<Recorder>();
                if (cloneRecorder != null)
                {
                    clone.transform.position = spawnPoint.position;
                    clone.transform.rotation = spawnPoint.rotation;
                    cloneRecorder.ResetReplayIndex();  // Fixed this line to reset clone recorder, not playerRecorder
                    cloneRecorder.StartReplaying();
                }
            }
        }
    }
}
