using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Recorder : MonoBehaviour
{
    public class InputFrameData
    {
        public float horizontal;
        public float vertical;
        public bool jump;
        public Vector3 position;
        public Quaternion rotation;
    }

    public enum Mode { Recording, Replaying }
    public Mode mode = Mode.Recording;

    public List<InputFrameData> recordedInputs = new List<InputFrameData>();
    private int frameIndex = 0;

    public GameObject clonePrefab;

    // Input System (for Player only)
    private InputSystemActions controls;
    private Vector2 moveInput = Vector2.zero;
    private bool jumpRequested = false;

    // Recording control
    private bool shouldRecord = false;

    void Awake()
    {
        // Only setup input if this is the player (not a clone)
        if (CompareTag("Player"))
        {
            controls = new InputSystemActions();

            controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            controls.Player.Jump.performed += ctx => jumpRequested = true;
        }
    }

    void OnEnable()
    {
        if (controls != null)
            controls.Enable();
    }

    void OnDisable()
    {
        if (controls != null)
            controls.Disable();
    }

    void Update()
    {
        if (mode == Mode.Recording)
        {
            RecordUpdate();
        }
        else if (mode == Mode.Replaying)
        {
            ReplayUpdate();
        }
    }

    void RecordUpdate()
    {
        // Only record if we should be recording
        if (!shouldRecord) return;

        InputFrameData frame = new InputFrameData
        {
            horizontal = moveInput.x,
            vertical = moveInput.y,
            jump = jumpRequested,
            position = transform.position,
            rotation = transform.rotation
        };

        recordedInputs.Add(frame);
        jumpRequested = false;
        
        // Debug recording progress
        if (recordedInputs.Count % 60 == 0) // Log every 60 frames (about once per second)
        {
            Debug.Log($"Recording frame {recordedInputs.Count} for {gameObject.name} at position {transform.position}");
        }
    }

    void ReplayUpdate()
    {
        if (frameIndex >= recordedInputs.Count)
        {
            Debug.Log($"Replay complete for {gameObject.name}, frameIndex: {frameIndex}, total frames: {recordedInputs.Count}");
            return;
        }

        InputFrameData frame = recordedInputs[frameIndex];

        transform.position = frame.position;
        transform.rotation = frame.rotation;

        frameIndex++;
    }

    public void StartRecording()
    {
        // Don't clear recordedInputs here - let LevelManager handle it after spawning clone
        frameIndex = 0;
        mode = Mode.Recording;
        // shouldRecord will be set by LevelManager based on player location
    }

    public void EnableRecording()
    {
        shouldRecord = true;
        Debug.Log($"Recording enabled for {gameObject.name}");
    }

    public void DisableRecording()
    {
        Debug.Log($"RecordedInputs count: {recordedInputs.Count}");
        shouldRecord = false;
        Debug.Log($"Recording disabled for {gameObject.name}");
    }

    public void StopRecording()
    {
        if (mode == Mode.Recording)
        {
            shouldRecord = false;
            Debug.Log("Recording stopped");
        }
    }

    public void StopReplaying()
    {
        mode = Mode.Replaying;
        frameIndex = 0;
        Debug.Log($"Stopped replaying for {gameObject.name}");
    }

    public void StartReplaying()
    {
        frameIndex = 0;
        mode = Mode.Replaying;
        Debug.Log($"Started replaying for {gameObject.name} with {recordedInputs.Count} frames");
        
        // Debug the actual data the clone has
        if (recordedInputs.Count > 0)
        {
            Debug.Log($"Clone {gameObject.name} has data from {recordedInputs[0].position} to {recordedInputs[recordedInputs.Count - 1].position}");
        }
        else
        {
            Debug.LogWarning($"Clone {gameObject.name} has NO recorded data!");
        }
    }

    public void ResetReplayIndex()
    {
        frameIndex = 0;
    }
}
