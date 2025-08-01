using UnityEngine;
using System.Collections.Generic;

public class Recorder: MonoBehaviour
{
    
    public class InputFrameData
    {
        public float horizontal;
        public float vertical;
        public bool jump;
        public Vector3 position;
    }

    public enum Mode { Recording, Replaying }
    public Mode mode = Mode.Recording;

    public List<InputFrameData> recordedInputs = new List<InputFrameData>();
    private int frameIndex = 0;

    private CharacterController controller;
    private Vector3 velocity;
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    private float yVelocity;
    private bool isGrounded;

    public GameObject clonePrefab;

    void Start()
    {
        controller = GetComponent<CharacterController>();
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
        isGrounded = controller.isGrounded;
        if (isGrounded && yVelocity < 0)
            yVelocity = 0f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        yVelocity += gravity * Time.deltaTime;
        velocity.y = yVelocity;
        controller.Move(velocity * Time.deltaTime);

        InputFrameData frame = new InputFrameData
        {
            horizontal = h,
            vertical = v,
            jump = Input.GetButtonDown("Jump"),
            position = transform.position
        };

        recordedInputs.Add(frame);
    }

    void ReplayUpdate()
    {
        if (frameIndex >= recordedInputs.Count) return;

        InputFrameData frame = recordedInputs[frameIndex];

        float h = frame.horizontal;
        float v = frame.vertical;

        Vector3 move = transform.right * h + transform.forward * v;
        controller.Move(move.normalized * moveSpeed * Time.deltaTime);

        if (frame.jump && controller.isGrounded)
            yVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        yVelocity += gravity * Time.deltaTime;
        velocity.y = yVelocity;
        controller.Move(velocity * Time.deltaTime);

        frameIndex++;
    }

    public void StartRecording()
    {
        recordedInputs.Clear();
        frameIndex = 0;
        mode = Mode.Recording;
    }

    public void StartReplaying()
    {
        frameIndex = 0;
        mode = Mode.Replaying;
    }

    public void SpawnClone()
    {
        GameObject clone = Instantiate(clonePrefab, transform.position, transform.rotation);
        Recorder cloneRecorder = clone.GetComponent<Recorder>();
        cloneRecorder.recordedInputs = new List<InputFrameData>(recordedInputs);
        cloneRecorder.mode = Mode.Replaying;
    }
}

