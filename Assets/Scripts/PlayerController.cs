using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private CinemachineCamera fpsCamera;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float mouseSensitivity = .1f;
    [SerializeField] private float friction = 10f;
    [SerializeField] private float animationDampTime = 0.1f; // time to smooth animation parameter changes
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    private Animator animator;
    private float afkTime = 0f;
    private float gravity = -9.81f;

    private Vector3 currentVelocity;
    private Vector2 smoothInput;
    private Vector2 inputVelocity;
    private float verticalVelocity;

    //cache animator parameter hashes for performance
    private static readonly int ForwardHash = Animator.StringToHash("forward");
    private static readonly int StrafeHash = Animator.StringToHash("strafe");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int AfkTimeHash = Animator.StringToHash("afkTime");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");




    private void OnEnable()
    {
        moveAction.action.Enable();
        runAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable(); 
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        runAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();
        smoothInput = Vector2.SmoothDamp(smoothInput, rawInput, ref inputVelocity, 0.05f);

        bool isFPS = fpsCamera.IsLive;
        bool isMoving = rawInput.magnitude > .1f;
        bool isRunning = runAction.action.IsPressed();

        float targetSpeed = isRunning ? runSpeed : walkSpeed;

        ApplyGravity();
        HandleJump();

        if (isFPS)
        {
            Vector3 fpsDir = transform.right * smoothInput.x + transform.forward * smoothInput.y;
            HandleFPSMovement(fpsDir, targetSpeed);
        }
        else
        {
            HandleTPSMovement(rawInput, targetSpeed);
        }

        HandleAnimations(isMoving, isRunning, smoothInput, isFPS);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // small downward force to keep grounded

        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (animator != null)
        {
            animator.SetBool(IsGroundedHash, controller.isGrounded);
        }
    }

    private void HandleFPSMovement(Vector3 direction, float speed)
    {
        //rotation based on mouse input
        float mouseX = lookAction.action.ReadValue<Vector2>().x;
        transform.Rotate(Vector3.up * mouseX * mouseSensitivity);

        
        //apply friction
        Vector3 targetVelocity = direction * speed;
        currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetVelocity.x, friction * Time.deltaTime);
        currentVelocity.z = Mathf.Lerp(currentVelocity.z, targetVelocity.z, friction * Time.deltaTime);

        Vector3 finalMove = currentVelocity;
        finalMove.y = verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleTPSMovement(Vector2 input, float speed)
    {
        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredMoveDir = (camForward * input.y + camRight * input.x).normalized;

        if (desiredMoveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            Vector3 movement = desiredMoveDir * speed;
            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);
        }
        else
        {
            controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (controller.isGrounded && jumpAction.action.WasPressedThisFrame()) {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null) 
            {
                animator.SetTrigger(JumpTriggerHash);
            }
        }
    }

    private void HandleAnimations(bool isMoving, bool isRunning, Vector2 input, bool isFPS)
    {
        if (!isMoving)
            afkTime += Time.deltaTime;
        else
            afkTime = 0f;

        float targetForward = isMoving ? (isRunning ? 2f : 1f) : 0f;
        float targetStrafe = 0f;

        if (isFPS)
        {
            float speedMultiplier = isRunning ? 2f : 1f;
            targetForward = input.y * speedMultiplier;
            targetStrafe = input.x * speedMultiplier;
        }

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(IsRunningHash, isRunning);
            animator.SetFloat(AfkTimeHash, afkTime);

            animator.SetFloat(ForwardHash, targetForward, animationDampTime, Time.deltaTime);
            animator.SetFloat(StrafeHash, targetStrafe, animationDampTime, Time.deltaTime);
        }
    }
}
