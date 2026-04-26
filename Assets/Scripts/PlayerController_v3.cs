using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerAnimations))]
public class PlayerController_v3 : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private CinemachineCamera fpsCamera; // Reference to determine if player is in first-person mode
    [SerializeField] private CinemachineCamera tpsCamera; // Third-person camera for regular movement
    [SerializeField] private Transform mainCamera; // Reference for direction-based movement calculations
    [SerializeField] private CinemachineCamera aimCamera; // Camera to switch to when aiming

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference walkAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference selectSpell1Action;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference hollowPurpleAction;

    // References to specialized modules
    private PlayerMovement movement;
    private PlayerStats stats;
    private PlayerCombat combat;
    private PlayerAnimations animations;

    private bool isCtrlPressed = false; // Toggle state for slow walking
    private Vector3 curMoveDir;
    float speedMultiplier;

    private void Awake()
    {
        // Initialize references to neighboring components on the same object
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        animations = GetComponent<PlayerAnimations>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        // Enable all input listeners when the object is active
        moveAction.action.Enable();
        sprintAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        walkAction.action.Enable();
        selectSpell1Action.action.Enable();
        attackAction.action.Enable();
        hollowPurpleAction.action.Enable();
    }

    private void OnDisable()
    {
        // Disable all input listeners to prevent errors when the object is inactive
        moveAction.action.Disable();
        sprintAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        walkAction.action.Disable();
        selectSpell1Action.action.Disable();
        attackAction.action.Disable();
        hollowPurpleAction.action.Disable();
    }

    private void Update()
    {
        // 1. Gather raw input values
        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();

        //
        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;
        camForward.y = 0;
        camRight.y = 0;

        curMoveDir = (camForward.normalized * rawInput.y + camRight.normalized * rawInput.x).normalized;
        //


        bool isMoving = rawInput.magnitude > 0.1f;
        bool isFPS = fpsCamera.IsLive;
        bool isTPS = tpsCamera.IsLive;
        bool isShiftPressed = sprintAction.action.IsPressed();
        bool isAiming = combat.IsSpellSelected;

        // 2. Handle walking toggle logic
        if (walkAction.action.WasPressedThisFrame())
        {
            isCtrlPressed = !isCtrlPressed;
        }

        // 3. Determine movement state and speed multipliers
        speedMultiplier = 1f; // Default is running
        bool isSprinting = false;

        if (combat.IsSpellSelected)
        {
            speedMultiplier = .5f;
        }
        else if (isShiftPressed && isMoving && stats.HasStamina() && movement.IsGrounded)
        {
            speedMultiplier = 2f;
            isSprinting = true;
        }
        else if (isCtrlPressed && isMoving)
        {
            speedMultiplier = 0.5f;
        }

        aimCamera.Priority = isAiming && !isFPS ? 20 : 5;

        // 4. Update physical movement and rotation
        movement.ProcessGravity();
        movement.CheckGroundStatus();
        movement.SmoothlyResizeCollider();

        

        if (isFPS)
        {
            RecenterTPSOrbitalCamera();
            float mouseX = lookAction.action.ReadValue<Vector2>().x;
            movement.ApplyFPSRotation(mouseX);
        }
        else if (isAiming)
        {
            RecenterTPSOrbitalCamera();
            float mouseX = lookAction.action.ReadValue<Vector2>().x;
            movement.ApplyAimRotation(mainCamera, mouseX);
        }
        else if (isTPS)
        {
            CancelTPSOrbitalCameraRecentering();
            movement.ApplyTPSRotation(rawInput, mainCamera);
        }

        // 5. Process Jump request
        if (jumpAction.action.WasPressedThisFrame())
        {
            movement.TryJump(stats);

        }

        movement.HandleAirMovement(moveAction, mainCamera);

        // 6. Update Resources (Stamina and Mana)
        stats.TickResources(isSprinting, movement.IsGrounded);

        // 7. Handle Combat and Spellcasting
        if (selectSpell1Action.action.WasPressedThisFrame())
        {
            if (combat.selectedSpell == combat.GetSlot1Spell())
            {
                combat.DeselectSpell();
            }
            else
            {
                combat.SelectSpellSlot1();
            }
                
        }
        else if (hollowPurpleAction.action.WasPressedThisFrame())
        {
            if (combat.selectedSpell == combat.GetSlot2Spell())
            {
                combat.DeselectSpell();
            }
            else
            {
                combat.SelectSpellHollowPurple();
            }
        }

        if (attackAction.action.WasPressedThisFrame())
        {
            combat.TryCastSelectedSpell(stats, mainCamera);
        }

        // 8. Update Animations
        animations.UpdateMovementParameters(rawInput, speedMultiplier, isMoving, movement.IsGrounded, isSprinting, isFPS, combat.IsSpellSelected, isAiming);
    }

    private void OnAnimatorMove()
    {
        // Transfer root motion data from the animator to the movement module
        if (movement != null)
        {
            movement.ExecuteRootMotion(animations.GetDeltaPosition(), curMoveDir, speedMultiplier);
        }
    }

    private void RecenterTPSOrbitalCamera()
    {
        var orbitalFollow = tpsCamera.GetComponent<CinemachineOrbitalFollow>();

        if (orbitalFollow != null)
        {
            orbitalFollow.HorizontalAxis.Recentering.Enabled = true;
            orbitalFollow.HorizontalAxis.Recentering.Wait = 0f;
        }
        else
        {
            Debug.LogWarning("CinemachineOrbitalFollow component not found on TPS camera. Cannot recenter.");
        }
    }

    private void CancelTPSOrbitalCameraRecentering()
    {
        var orbitalFollow = tpsCamera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow != null)
        {
            orbitalFollow.HorizontalAxis.Recentering.Enabled = false;
        }
        else
        {
            Debug.LogWarning("CinemachineOrbitalFollow component not found on TPS camera. Cannot cancel recentering.");
        }
    }
}