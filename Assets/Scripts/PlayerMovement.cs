using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float mouseSensitivity = .1f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float jumpColliderHeight = 1.1f;
    [SerializeField] private float jumpStaminaPenalty = 20f;

    [Header("Jump Assist")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    private float jumpBufferTimer;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private float groundCheckOffset = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Air Control")]
    [SerializeField] private float airControl = 2f;
    [SerializeField] private float airDrag = 0.98f;
    [SerializeField] private float maxAirSpeed = 5f;

    [Header("Physics Limits")]
    [SerializeField] private float terminalVelocity = -20f; //Prevent falling at infinite speed

    [Header("Collider Transition Settings")]
    [SerializeField] private float colliderResizeSpeed = 2.5f;
    private float targetColliderHeight;

    // State Variables
    public bool IsGrounded { get; private set; } = true;
    private float verticalVelocity;
    private Vector3 horizontalMomentum;
    private bool isInAir = false;
    private float coyoteTimer;
    private bool hasJumped = false;

    // Stored original values for collider resetting
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;

    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    private static readonly int VerticalVelocityHash = Animator.StringToHash("verticalVelocity");

    private void Start()
    {
        // Store initial collider dimensions for landing reset
        originalColliderHeight = controller.height;
        originalColliderCenter = controller.center;

        targetColliderHeight = originalColliderHeight;
    }

    public void ProcessGravity()
    {
        // Apply constant downward force when grounded to snap to floor
        if (IsGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, terminalVelocity);
        }

        animator.SetFloat(VerticalVelocityHash, verticalVelocity);
    }

    public void CheckGroundStatus()
    {
        // Ignore ground checks briefly during the upward phase of a jump
        if (verticalVelocity > 0.1f || jumpBufferTimer > 0f)
        {
            //Debug
            Debug.Log("In jump buffer or ascending: verticalVelocity=" + verticalVelocity + ", jumpBufferTimer=" + jumpBufferTimer);

            jumpBufferTimer -= Time.deltaTime;
            IsGrounded = false;
            animator.SetBool(IsGroundedHash, IsGrounded);
            return;
        }

        //Vector3 jumpColliderOffset = Vector3.zero;
        //jumpColliderOffset.y = hasJumped ? controller.height / 2f : 0f;
        
        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Vector3 left = origin - transform.right * groundCheckOffset;
        Vector3 right = origin + transform.right * groundCheckOffset;
        Vector3 forward = origin + transform.forward * groundCheckOffset;
        Vector3 back = origin - transform.forward * groundCheckOffset;

        // Multi-raycast for stability on ledges
        bool centerHit = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
        bool leftHit = Physics.Raycast(left, Vector3.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics.Raycast(right, Vector3.down, groundCheckDistance, groundLayer);
        bool forwardHit = Physics.Raycast(forward, Vector3.down, groundCheckDistance, groundLayer);
        bool backHit = Physics.Raycast(back, Vector3.down, groundCheckDistance, groundLayer);

        IsGrounded = centerHit || leftHit || rightHit || forwardHit || backHit;
        //Debug
        Debug.Log("Grounded: centerHit=" + centerHit + ", leftHit=" + leftHit + ", rightHit=" + rightHit + ", forwardHit=" + forwardHit + ", backHit=" + backHit);

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
            isInAir = false;
            horizontalMomentum = Vector3.zero;
            hasJumped = false;

            //start growing the collider back to normal size
            targetColliderHeight = originalColliderHeight;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            isInAir = true;

            //keep the collider small while in the air
            targetColliderHeight = jumpColliderHeight;
        }

        animator.SetBool(IsGroundedHash, IsGrounded);
    }

    public void ApplyTPSRotation(Vector2 input, Transform cam)
    {
        // Rotate character toward camera-relative movement direction
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector3 desiredMoveDir = (camForward.normalized * input.y + camRight.normalized * input.x).normalized;

        if (desiredMoveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void ApplyAimRotation(Transform cam, float mouseX)
    {
        Vector3 cameraForward = cam.forward;
        cameraForward.y = 0f;

        //if (cameraForward.sqrMagnitude > 0.001f)
        //{
        //    Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        //}
        transform.Rotate(mouseX * Vector3.up * mouseSensitivity);
    }

    public void ApplyFPSRotation(float mouseX)
    {
        // Rotate character based on mouse horizontal axis
        transform.Rotate(Vector3.up * mouseX * mouseSensitivity);
    }

    public void TryJump(PlayerStats stats)
    {
        // Check for sufficient resources and valid timing (coyote time)
        if (coyoteTimer > 0f && stats.CanConsumeStamina(jumpStaminaPenalty) && IsGrounded)
        {
            IsGrounded = false;
            jumpBufferTimer = jumpBufferTime;
            coyoteTimer = 0f;

            // Capture momentum for air movement calculations
            horizontalMomentum = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            isInAir = true;

            stats.ConsumeStamina(jumpStaminaPenalty);

            animator.SetTrigger(JumpTriggerHash);
        }
    }

    public void HandleAirMovement(InputActionReference moveAction, Transform cam)
    {
        if (!isInAir) return;

        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 desiredAirVelocity = (camForward * rawInput.y + camRight * rawInput.x) * maxAirSpeed;

        horizontalMomentum = Vector3.Lerp(horizontalMomentum, desiredAirVelocity, airControl * Time.deltaTime);

        if (rawInput.sqrMagnitude < 0.01f)
        {
            horizontalMomentum *= airDrag;
        }
    }

    public void ExecuteRootMotion(Vector3 animationDelta, Vector3 moveDirection, float speedMultiplier)
    {
        Vector3 velocity;

        // Use custom physics logic while in the air, use root motion delta while grounded
        if (isInAir)
        {
            velocity = horizontalMomentum * Time.deltaTime;
        }
        else
        {
            // 1. Calculate how fast the animation wants to move
            float animSpeed = animationDelta.magnitude / Time.deltaTime;

            // 2. Define a "Reference Speed" (Your forward run speed)
            // Usually, a Mixamo run is around 4-5.5 units per second.
            float referenceSpeed = 3.5f * speedMultiplier;

            // 3. If the user is pushing the stick, but the animation speed is too low
            if (moveDirection.magnitude > 0.1f && animSpeed < referenceSpeed)
            {
                // Inject the missing speed manually
                velocity = moveDirection.normalized * referenceSpeed * Time.deltaTime;
            }
            else
            {
                velocity = animationDelta;
            }
        }

        velocity.y = verticalVelocity * Time.deltaTime;
        controller.Move(velocity);
    }

    public void SmoothlyResizeCollider()
    {
        // Check if the collider needs to grow
        if (Mathf.Abs(controller.height - targetColliderHeight) > 0.01f)
        {
            // Gradually increase the height back to the original size
            controller.height = Mathf.MoveTowards(controller.height, targetColliderHeight, colliderResizeSpeed * Time.deltaTime);

            // Force the center to always be half of the current height.
            // This anchors the bottom of the capsule to the character's feet, preventing sinking.
            controller.center = new Vector3(
                originalColliderCenter.x,
                controller.height / 2f,
                originalColliderCenter.z
            );
        }
    }

    public void LaunchJumpPhysics3()
    {
        IsGrounded = false;

        // Shrink collider to prevent getting stuck on ceilings/ledges during ascent
        controller.height = jumpColliderHeight;
        controller.center = new Vector3(
            originalColliderCenter.x, 
            controller.height / 2f, 
            originalColliderCenter.z
        );

        hasJumped = true;

        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        //Debug
        Debug.Log("LaunchJumpPhysics333: isGrounded" + IsGrounded + ", jump collider height: " + controller.height);
        
    }

    //Debug function
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        //Vector3 jumpColliderOffset = Vector3.zero;
        //jumpColliderOffset.y = hasJumped ? controller.height / 2f : 0f;

        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Vector3 left = origin - transform.right * groundCheckOffset;
        Vector3 right = origin + transform.right * groundCheckOffset;
        Vector3 forward = origin + transform.forward * groundCheckOffset;
        Vector3 back = origin - transform.forward * groundCheckOffset;

        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(left, left + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(right, right + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(forward, forward + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(back, back + Vector3.down * groundCheckDistance);
    }
}