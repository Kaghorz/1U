using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class PlayerController_v2 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private CinemachineCamera fpsCamera;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float mouseSensitivity = .1f;
    [SerializeField] private float animationDampTime = 0.1f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float jumpColliderHeight = 1.2f;

    [Header("Stamina UI")]
    [SerializeField] private GameObject staminaBarParent; //object to hide/show
    [SerializeField] private Image staminaFillImage; //image

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDepleteRate = 15f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float jumpStaminaPenalty = 20f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference walkAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.25f;
    [SerializeField] private float groundCheckOffset = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Jump Assist")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float manaRegenRate = 5f;
    private float currentMana;

    [Header("Spell Settings")]
    [SerializeField] private SpellData slot1Spell;
    [SerializeField] private Image spell1CooldownImage;
    [SerializeField] private InputActionReference selectSpell1Action;
    [SerializeField] private InputActionReference attackAction;

    private SpellData selectedSpell;
    private float lastCastTime = float.NegativeInfinity;
    
    

    private Animator animator;
    private float afkTime = 0f;
    private float gravity = -9.81f;
    private bool useRootMotion = true;

    //Movement variables
    private Vector2 smoothInput;
    private Vector2 inputVelocity;
    private float verticalVelocity;

    private float currentStamina;
    private bool isGrounded = true;
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;

    bool isCtrlPressed = false;

    //Air movement variables
    private float airControl = 2f;
    private float airDrag = 0.98f;
    private Vector3 horizontalMomentum;
    private bool isInAir = false;
    private float maxAirSpeed = 5f;

    //Jump assist timers
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpGraceTimer = 0f;

    private static readonly int ForwardHash = Animator.StringToHash("forward");
    private static readonly int StrafeHash = Animator.StringToHash("strafe");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsSprintingHash = Animator.StringToHash("isSprinting");
    private static readonly int AfkTimeHash = Animator.StringToHash("afkTime");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    private void OnEnable()
    {
        moveAction.action.Enable();
        runAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        walkAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        runAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        sprintAction.action.Disable();
        walkAction.action.Disable();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        currentStamina = maxStamina;
        currentMana = maxMana;

        spell1CooldownImage.fillAmount = 0;

        originalColliderHeight = controller.height;
        originalColliderCenter = controller.center;

        if (staminaBarParent != null) staminaBarParent.SetActive(false);
    }

    void Update()
    {
        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();
        smoothInput = Vector2.SmoothDamp(smoothInput, rawInput, ref inputVelocity, 0.05f);

        bool isFPS = fpsCamera.IsLive;
        bool isMoving = rawInput.magnitude > .1f;

        bool isShiftPressed = sprintAction.action.IsPressed();

        //Check if Ctrl is held for walking
        if (walkAction.action.WasPressedThisFrame() && !isCtrlPressed)
        {
            isCtrlPressed = true;
        }
        else if (walkAction.action.WasPressedThisFrame() && isCtrlPressed)
        {
            isCtrlPressed = false;
        }
            

        //Determine movement state: Sprint has priority, then Walk, default is Run
        float speedMultiplier = 1f; //Default running speed
        bool isSprinting = false;

        if (isShiftPressed && isMoving && currentStamina > 0 && isGrounded)
        {
            speedMultiplier = 2f; //Increase speed for sprinting
            isSprinting = true;
        }
        else if (isCtrlPressed && isMoving)
        {
            speedMultiplier = 0.5f; //Decrease speed for walking
        }

        //Check if player is allowed to regenerate stamina
        bool isRegenerating = !isShiftPressed && isGrounded;

        HandleStamina(isSprinting, isRegenerating);

        ApplyGravity();

        //Detect jump input early
        if (jumpAction.action.WasPressedThisFrame())
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        HandleJump();
        HandleAirMovement();
        CheckGround();

        RegenerateMana();
        HandleSpellSelection();
        HandleSpellCasting();

        if (isFPS)
        {
            float mouseX = lookAction.action.ReadValue<Vector2>().x;
            transform.Rotate(Vector3.up * mouseX * mouseSensitivity);
        }
        else if (isMoving)
        {
            HandleTPSRotation(rawInput);
        }

        HandleAnimations(isMoving, isSprinting, smoothInput, isFPS, speedMultiplier);
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void HandleTPSRotation(Vector2 input)
    {
        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;
        camForward.y = 0;
        camRight.y = 0;

        Vector3 desiredMoveDir = (camForward.normalized * input.y + camRight.normalized * input.x).normalized;

        if (desiredMoveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleJump()
    {
        if (coyoteTimer > 0f && jumpBufferTimer > 0f && currentStamina >= jumpStaminaPenalty)
        {
            isGrounded = false;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;

            currentStamina -= jumpStaminaPenalty;

            horizontalMomentum = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
            isInAir = true;

            jumpGraceTimer = .25f; //Allow a short window after jump to still be considered "jumping" for animation purposes

            animator.SetTrigger(JumpTriggerHash);
        }
    }

    private void HandleAirMovement()
    {
        if (!isInAir) return;

        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();

        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 desiredAirVelocity = (camForward * rawInput.y + camRight * rawInput.x) * maxAirSpeed;

        horizontalMomentum = Vector3.Lerp(horizontalMomentum, desiredAirVelocity, airControl * Time.deltaTime);

        if (rawInput.sqrMagnitude < 0.01f)
        {
            horizontalMomentum *= airDrag;
        }
    }

    private void HandleAnimations(bool isMoving, bool isSprinting, Vector2 input, bool isFPS, float speedMultiplier)
    {
        if (!isMoving && !isInAir)
            afkTime += Time.deltaTime;
        else
            afkTime = 0f;

        float targetForward;
        float targetStrafe;

        if (isFPS)
        {
            //in FPS, forward/backward and strafing are separate, so we use the input directly
            targetForward = input.y * speedMultiplier;
            targetStrafe = input.x * speedMultiplier;
        }
        else
        {
            //in TPS, movement is always forward in the animation, so we use the magnitude of the input to determine speed
            targetForward = input.magnitude * speedMultiplier;
            targetStrafe = 0f;
        }

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(IsSprintingHash, isSprinting);
            animator.SetBool(IsRunningHash, isMoving && !isSprinting && speedMultiplier >= 1f);
            animator.SetFloat(AfkTimeHash, afkTime);
            animator.SetFloat(ForwardHash, targetForward, animationDampTime, Time.deltaTime);
            animator.SetFloat(StrafeHash, targetStrafe, animationDampTime, Time.deltaTime);
        }
    }

    public void LaunchJumpPhysics()
    {
        isGrounded = false;

        //Reduce collider when jump actually launches
        controller.height = jumpColliderHeight;
        controller.center = new Vector3(
            originalColliderCenter.x,
            jumpColliderHeight / 2f,
            originalColliderCenter.z
        );

        useRootMotion = false;
        verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        //Debug
        Debug.Log("LaunchJumpPhysics: isGrounded" + isGrounded + ", jump collider height: " + controller.height);
    }

    public void LandJumpPhysics()
    {
        useRootMotion = true; 
    }

    private void OnAnimatorMove()
    {
        if (animator == null) return;

        //Get the movement proposed by the animation
        Vector3 velocity = animator.deltaPosition;

        //Apply gravity and air control if in the air
        if (isInAir)
        {
            velocity = horizontalMomentum * Time.deltaTime;
            velocity.y = verticalVelocity * Time.deltaTime;
        }
        else
        {
            velocity = animator.deltaPosition;
            velocity.y = verticalVelocity * Time.deltaTime;
        }

        //Move the controller
        controller.Move(velocity);
    }

    private void HandleStamina(bool isSprinting, bool isRegenerating)
    {
        if (isSprinting)
        {
            //Decrease stamina when running
            currentStamina -= staminaDepleteRate * Time.deltaTime;
        }
        else if (isRegenerating)
        {
            //Increase stamina only when on the ground and shift is not pressed
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        //Keep value within 0 and maxStamina
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        //Deactivate the bar when it is completely filled
        if (staminaBarParent != null)
        {
            staminaBarParent.SetActive(currentStamina < maxStamina);
        }

        //Update the visual fill amount of the bar
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = currentStamina / maxStamina;
        }
    }

    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
        }
    }

    private void HandleSpellSelection()
    {
        if (selectSpell1Action.action.WasPressedThisFrame())
        {
            selectedSpell = slot1Spell;

            //Debug
            Debug.Log("Spell selected: " + selectedSpell);
        }
    }

    private void HandleSpellCasting()
    {
        if (selectedSpell == null) return;

        float timeSinceCast = Time.time - lastCastTime;

        if (spell1CooldownImage != null)
        {
            float cooldownProgress = Mathf.Clamp01(1 - (timeSinceCast / selectedSpell.cooldown));
            spell1CooldownImage.fillAmount = cooldownProgress;
        }

        if (attackAction.action.WasPressedThisFrame())
        {
            if (timeSinceCast >= selectedSpell.cooldown && currentMana >= selectedSpell.cooldown)
            {
                CastSelectedSpell();
            }
        }
    }

    private void CastSelectedSpell()
    {
                currentMana -= selectedSpell.manaCost;
        lastCastTime = Time.time;

        animator.SetTrigger("CastSpell");

        Ray ray = mainCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(.5f, .5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 castDirection = (targetPoint - transform.position + transform.up).normalized;

        Instantiate(selectedSpell.spellPrefab, transform.position + transform.up, Quaternion.LookRotation(castDirection));
    }

    private void CheckGround()
    {
        //Prevent grounding shortly after jumping
        if (verticalVelocity > 0.1f || jumpGraceTimer > 0f)
        {
            jumpGraceTimer -= Time.deltaTime;
            isGrounded = false;
            animator.SetBool(IsGroundedHash, false);
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        Vector3 left = origin - transform.right * groundCheckOffset;
        Vector3 right = origin + transform.right * groundCheckOffset;

        bool centerHit = Physics.Raycast(origin, Vector3.down, groundCheckDistance, groundLayer);
        bool leftHit = Physics.Raycast(left, Vector3.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics.Raycast(right, Vector3.down, groundCheckDistance, groundLayer);

        isGrounded = centerHit || leftHit || rightHit;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            isInAir = false;
            horizontalMomentum = Vector3.zero;
        }
        else
            coyoteTimer -= Time.deltaTime;

        if (isGrounded && verticalVelocity < 0)
        {
            controller.height = originalColliderHeight;
            controller.center = originalColliderCenter;
        }

        //Debug
        Debug.Log("CheckGround isGrounded: " + isGrounded + " (center: " + centerHit + ", left: " + leftHit + ", right: " + rightHit + ")");

        if (animator != null)
            animator.SetBool(IsGroundedHash, isGrounded);
    }




    //Debug function
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Vector3 left = origin - transform.right * groundCheckOffset;
        Vector3 right = origin + transform.right * groundCheckOffset;

        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(left, left + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(right, right + Vector3.down * groundCheckDistance);
    }
}