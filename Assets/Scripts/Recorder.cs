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
    }

    void ReplayUpdate()
    {
        if (frameIndex >= recordedInputs.Count)
            return;

        InputFrameData frame = recordedInputs[frameIndex];

        transform.position = frame.position;
        transform.rotation = frame.rotation;

        frameIndex++;
    }

    public void StartRecording()
    {
        recordedInputs.Clear();
        frameIndex = 0;
        mode = Mode.Recording;
    }

    public void StopRecording()
    {
        if (mode == Mode.Recording)
        {
            Debug.Log("Recording stopped");

            if (CompareTag("Player"))
            {
                this.enabled = false; // stop recorder on player
            }
            else
            {
                mode = Mode.Replaying;
            }
        }
    }

    public void StartReplaying()
    {
        frameIndex = 0;
        mode = Mode.Replaying;
    }

    public void ResetReplayIndex()
    {
        frameIndex = 0;
    }


    public void SpawnClone()
    {
        GameObject clone = Instantiate(clonePrefab, transform.position, transform.rotation);

        // Set clone tag so its input doesn't initialize
        clone.tag = "Clone";

        // Disable camera on clone, just in case
        Camera cloneCamera = clone.GetComponentInChildren<Camera>();
        if (cloneCamera != null)
            cloneCamera.enabled = false;

        Recorder cloneRecorder = clone.GetComponent<Recorder>();
        cloneRecorder.recordedInputs = new List<InputFrameData>(recordedInputs);
        cloneRecorder.mode = Mode.Replaying;
    }
}
