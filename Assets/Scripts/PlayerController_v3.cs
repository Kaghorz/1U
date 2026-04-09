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
    [SerializeField] private Transform mainCamera; // Reference for direction-based movement calculations

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference walkAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference selectSpell1Action;
    [SerializeField] private InputActionReference attackAction;

    // References to specialized modules
    private PlayerMovement movement;
    private PlayerStats stats;
    private PlayerCombat combat;
    private PlayerAnimations animations;

    private bool isCtrlPressed = false; // Toggle state for slow walking

    private void Awake()
    {
        // Initialize references to neighboring components on the same object
        movement = GetComponent<PlayerMovement>();
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        animations = GetComponent<PlayerAnimations>();
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
    }

    private void Update()
    {
        // 1. Gather raw input values
        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();
        bool isMoving = rawInput.magnitude > 0.1f;
        bool isFPS = fpsCamera.IsLive;
        bool isShiftPressed = sprintAction.action.IsPressed();

        // 2. Handle walking toggle logic
        if (walkAction.action.WasPressedThisFrame())
        {
            isCtrlPressed = !isCtrlPressed;
        }

        // 3. Determine movement state and speed multipliers
        float speedMultiplier = 1f; // Default is running
        bool isSprinting = false;

        if (isShiftPressed && isMoving && stats.HasStamina() && movement.IsGrounded)
        {
            speedMultiplier = 2f;
            isSprinting = true;
        }
        else if (isCtrlPressed && isMoving)
        {
            speedMultiplier = 0.5f;
        }

        // 4. Update physical movement and rotation
        movement.ProcessGravity();
        movement.CheckGroundStatus();
        movement.SmoothlyResizeCollider();

        if (isFPS)
        {
            float mouseX = lookAction.action.ReadValue<Vector2>().x;
            movement.ApplyFPSRotation(mouseX);
        }
        else if (isMoving)
        {
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
            combat.SelectSpellSlot1();
        }

        if (attackAction.action.WasPressedThisFrame())
        {
            combat.TryCastSelectedSpell(stats, mainCamera);
        }

        // 8. Update Animations
        animations.UpdateMovementParameters(rawInput, speedMultiplier, isMoving, movement.IsGrounded, isSprinting, isFPS);
    }

    private void OnAnimatorMove()
    {
        // Transfer root motion data from the animator to the movement module
        if (movement != null)
        {
            movement.ExecuteRootMotion(animations.GetDeltaPosition());
        }
    }
}