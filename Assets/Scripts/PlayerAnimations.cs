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


    public void UpdateMovementParameters(Vector2 input, float speedMultiplier, bool isMoving, bool isGrounded, bool isSprinting, bool isFPS, bool isSpellSelected, bool isAiming)
    {
        // Handle AFK timer logic
        if (!isMoving && isGrounded && !isSpellSelected)
            afkTime += Time.deltaTime;
        else
            afkTime = 0f;

        float targetForward;
        float targetStrafe;

        // Determine the target weight for aiming animations
        float targetWeight = isAiming ? 1f : 0f; // Full weight for aiming, no weight for normal movement
        

        if (isFPS || isAiming)
        {
            // In First-Person or aiming mode, forward/backward and strafing are separate values
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

            float currentWeight = animator.GetLayerWeight(1); // IMPORTANT: Assuming layer 1 is for aiming
            animator.SetLayerWeight(1, Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * 10f));
        }
    }

    /// <summary>
    /// Returns the movement delta proposed by the current animation state.
    /// This is used by the Hub to apply Root Motion to the Character Controller.
    /// </summary>
    public Vector3 GetDeltaPosition()
    {
        return animator != null ? animator.deltaPosition : Vector3.zero;
    }

    /// <summary>
    /// Triggers a specific animation state.
    /// </summary>
    public void SetTrigger(int triggerHash)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerHash);
        }
    }

    public bool isCastingHollowPurple() 
    {
        return animator.GetCurrentAnimatorStateInfo(1).IsName("HollowPurple");
    }
}