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
    [SerializeField] private InputActionReference selectSpell2Action;
    [SerializeField] private InputActionReference selectSpell3Action;
    [SerializeField] private InputActionReference selectUltimativeSpellAction; // FUTURE: Placeholder for an ultimate spell input
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference enableFightModeAction;

    // References to specialized modules
    private PlayerMovement movement;
    private PlayerStats stats;
    private PlayerCombat combat;
    private PlayerAnimations animations;

    private bool isCtrlPressed = false; // Toggle state for slow walking
    private Vector3 curMoveDir;
    float speedMultiplier;
    private bool isFightModeEnabled = false;
    private bool isCastingSpell = false;
    private float lastActionTime = 0f; // To track time since last action to return to basic locomotion
    private float fightModeTimeout = 5f; // Time threshold to blend out from fight mode back to basic locomotion
    private bool isIdleInFightMode = false; // To track if the player has been idle in fight mode for too long


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
        enableFightModeAction.action.Enable();
        selectSpell1Action.action.Enable();
        attackAction.action.Enable();
        selectSpell3Action.action.Enable();
    }

    private void OnDisable()
    {
        // Disable all input listeners to prevent errors when the object is inactive
        moveAction.action.Disable();
        sprintAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        walkAction.action.Disable();
        enableFightModeAction.action.Disable();
        selectSpell1Action.action.Disable();
        attackAction.action.Disable();
        selectSpell3Action.action.Disable();
    }

    private void Update()
    {
        // Gather raw input values
        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();

        if (combat.IsAttacking)
        {
            // Lock movement directional input while dealing a melee combo
            rawInput = Vector2.zero;
        }

        Vector3 camForward = mainCamera.forward;
        Vector3 camRight = mainCamera.right;
        camForward.y = 0;
        camRight.y = 0;

        curMoveDir = (camForward.normalized * rawInput.y + camRight.normalized * rawInput.x).normalized;
       
        bool isMoving = rawInput.magnitude > 0.1f;
        bool isFPS = fpsCamera.IsLive;
        bool isTPS = tpsCamera.IsLive;
        bool isShiftPressed = sprintAction.action.IsPressed();


            

        // Handle walking toggle logic
        if (walkAction.action.WasPressedThisFrame() && !isCastingSpell && !isFightModeEnabled)
        {
            isCtrlPressed = !isCtrlPressed;
        }

  

        // Determine movement state and speed multipliers
        speedMultiplier = 1f; // Default is running
        bool isSprinting = false;

        if (isCastingSpell || isFightModeEnabled)
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

        aimCamera.Priority = (isCastingSpell || (isFightModeEnabled && !isIdleInFightMode)) && !isFPS ? 20 : 5;

        // Update physical movement and rotation
        movement.ProcessGravity();
        movement.CheckGroundStatus();
        movement.SmoothlyResizeCollider();

        
        if (isFPS || isCastingSpell || isFightModeEnabled)
        {
            RecenterTPSOrbitalCamera();
            float mouseX = lookAction.action.ReadValue<Vector2>().x;
            movement.ApplyMouseBasedRotation(mouseX);
        }
        else if (isTPS || isIdleInFightMode)
        {
            CancelTPSOrbitalCameraRecentering();
            movement.ApplyTPSRotation(rawInput, curMoveDir);
        }

        // Process Jump request
        if (jumpAction.action.WasPressedThisFrame() && !isCastingSpell && !isFightModeEnabled)
        {
            movement.TryJump(stats);

        }

        movement.HandleAirMovement(moveAction, mainCamera);

        // Update Resources (Stamina and Mana)
        stats.TickResources(isSprinting, movement.IsGrounded);



        // Handle Combat and Spellcasting
        if (selectSpell1Action.action.WasPressedThisFrame())
        {
            if (combat.selectedSpell == combat.GetSlot1Spell())
            {
                combat.DeselectSpell();
                isCastingSpell = false;
            }
            else if (combat.SelectSpellSlot1(stats))
            {
                isCastingSpell = true;
                isFightModeEnabled = false;
                Debug.Log("Exiting Fight Mode to cast spell in slot 1. Fight Mode: " + isFightModeEnabled + " | Casting Spell: " + isCastingSpell);
            }

            lastActionTime = Time.time;
        }
        else if (selectSpell2Action.action.WasPressedThisFrame())
        {
            if (combat.selectedSpell == combat.GetSlot2Spell())
            {
                combat.DeselectSpell();
                isCastingSpell = false;
            }
            else if (combat.SelectSpellSlot2(stats))
            {
                isCastingSpell = true;
                isFightModeEnabled = false;
                Debug.Log("Exiting Fight Mode to cast spell in slot 2. Fight Mode: " + isFightModeEnabled + " | Casting Spell: " + isCastingSpell);
            }

            lastActionTime = Time.time;
        }
        else if (selectSpell3Action.action.WasPressedThisFrame())
        {
            if (combat.selectedSpell == combat.GetSlot3Spell())
            {
                combat.DeselectSpell();
                isCastingSpell = false;
            }
            else if (combat.SelectSpellSlot3(stats))
            {
                isCastingSpell = true;
                isFightModeEnabled = false;
                Debug.Log("Exiting Fight Mode to cast spell in slot 3. Fight Mode: " + isFightModeEnabled + " | Casting Spell: " + isCastingSpell);
            }

            lastActionTime = Time.time;
        }
        else if (enableFightModeAction.action.WasPressedThisFrame())
        {
            isCastingSpell = false;
            isFightModeEnabled = !isFightModeEnabled;
            Debug.Log("Fight Mode " + (isFightModeEnabled ? "Enabled" : "Disabled") + " | Casting Spell: " + isCastingSpell);

            lastActionTime = Time.time;
        }

        if (isCastingSpell && attackAction.action.WasPressedThisFrame())
        {
            combat.TryPerformAttack(stats, mainCamera);
            isCastingSpell = false; // Exit casting state after performing the attack

            lastActionTime = Time.time;
        }
        else if (isFightModeEnabled && attackAction.action.WasPressedThisFrame())
        {
            combat.TryPerformAttack(stats, mainCamera);

            lastActionTime = Time.time;
        }

        if (isFightModeEnabled && (Time.time - lastActionTime) > fightModeTimeout)
        {
            isIdleInFightMode = true;
            isFightModeEnabled= false;
        }
        else
        {
            isIdleInFightMode = false;
        }

        // Update Animations
        animations.UpdateMovementParameters(rawInput, speedMultiplier, isMoving, movement.IsGrounded, isSprinting, isFPS, combat.IsSpellSelected, isCastingSpell, isFightModeEnabled, isIdleInFightMode);
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