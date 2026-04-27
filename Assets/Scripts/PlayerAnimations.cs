using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private float animationDampTime = 0.1f; // Time used to smooth out transitions between movement states

    private float afkTime = 0f; // Tracks how long the player has been standing still

    // Cached Animator hashes for better performance
    private static readonly int ForwardHash = Animator.StringToHash("forward");
    private static readonly int StrafeHash = Animator.StringToHash("strafe");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsSprintingHash = Animator.StringToHash("isSprinting");
    private static readonly int AfkTimeHash = Animator.StringToHash("afkTime");
    private static readonly int IsFightModeEnabledHash = Animator.StringToHash("isFightModeEnabled");


    public void UpdateMovementParameters(Vector2 input, float speedMultiplier, bool isMoving, bool isGrounded, bool isSprinting, bool isFPS, bool isSpellSelected, bool isCastingSpell, bool isFightModeEnabled)
    {
        // Handle AFK timer logic
        if (!isMoving && isGrounded && !isSpellSelected && !isFightModeEnabled)
            afkTime += Time.deltaTime;
        else
            afkTime = 0f;

        float targetForward;
        float targetStrafe;

        // Determine the target weight for aiming animations
        float targetWeight = isCastingSpell || isFightModeEnabled ? 1f : 0f; // Full weight for spell casting or fight mode, no weight for normal movement
        

        if (isFPS || isCastingSpell || isFightModeEnabled)
        {
            // In First-Person, aiming, or fight mode, forward/backward and strafing are separate values
            targetForward = input.y * speedMultiplier;
            targetStrafe = input.x * speedMultiplier;
        }
        else
        {
            // In Third-Person, movement is always forward; we use input magnitude for speed
            targetForward = input.magnitude * speedMultiplier;
            targetStrafe = 0f;
        }

        // Apply values to the Animator with damping for smooth transitions
        if (animator != null)
        {
            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(IsSprintingHash, isSprinting);

            // Running is true when moving but not sprinting or walking slowly
            animator.SetBool(IsRunningHash, isMoving && !isSprinting && speedMultiplier >= 1f);

            animator.SetFloat(AfkTimeHash, afkTime);
            animator.SetFloat(ForwardHash, targetForward, animationDampTime, Time.deltaTime);
            animator.SetFloat(StrafeHash, targetStrafe, animationDampTime, Time.deltaTime);

            // TODO: maybe delete
            animator.SetBool(IsFightModeEnabledHash, isFightModeEnabled);

            float currentWeight = 0f;
            if (isCastingSpell)
            {
                currentWeight = animator.GetLayerWeight(1); // IMPORTANT: Assuming layer 1 is for aiming
                animator.SetLayerWeight(1, Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * 10f));
                animator.SetLayerWeight(3, 0); // Ensure fight mode layer is disabled when casting spells
            }
            else if (isFightModeEnabled)
            {
                currentWeight = animator.GetLayerWeight(3); // IMPORTANT: Assuming layer 3 is for fight mode
                animator.SetLayerWeight(3, Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * 10f));
                animator.SetLayerWeight(1, 0); // Ensure aiming layer is disabled when in fight mode
            }
            else
            {
                // When not casting or in fight mode, ensure both layers are disabled
                animator.SetLayerWeight(1, 0);
                animator.SetLayerWeight(3, 0);
            }
        }
    }

    public Vector3 GetDeltaPosition()
    {
        return animator != null ? animator.deltaPosition : Vector3.zero;
    }
}