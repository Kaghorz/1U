using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Action References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("Movement Settings")]
    [SerializeField] private float groundCheckDistance = 0.05f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float friction = 10f;    
    
    private CharacterController charCntr;
    private Animator animator;
    private float afkTime = 0f;
    private float gravity = -9.81f;

    private Vector3 currentVelocity;
    private Vector3 smoothInput;
    private Vector2 inputVelocity;

    private void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charCntr = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {        
        Vector2 rawInput = moveAction.action.ReadValue<Vector2>();
        smoothInput = Vector2.SmoothDamp(smoothInput, rawInput, ref inputVelocity, 0.05f);

        //check if the player is moving
        bool isMoving = rawInput.magnitude > 0.1f;
        bool isSprinting = sprintAction.action.IsPressed() && rawInput.y > 0;

        
        //if Shift is pressed, we sprint
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 moveDir = transform.right * smoothInput.x + transform.forward * smoothInput.y;

        //apply friction
        Vector3 targetVelocity = moveDir * targetSpeed;
        currentVelocity.x = Mathf.Lerp(currentVelocity.x, targetVelocity.x, friction * Time.deltaTime);
        currentVelocity.z = Mathf.Lerp(currentVelocity.z, targetVelocity.z, friction * Time.deltaTime);

        if (charCntr.isGrounded && currentVelocity.y < 0)
        {
            currentVelocity.y = -2f; // small negative value to keep the player grounded
        }
        else
        {
            //if not grounded, apply gravity
            currentVelocity.y += gravity * Time.deltaTime;
        }


        //final movement
        charCntr.Move(currentVelocity * Time.deltaTime);

        if (isMoving)
        {
            afkTime = 0f;
        }
        else
        {
            afkTime += Time.deltaTime;
        }

        //update animator parameters
        if (animator != null)
        {
            float speedMultiplier = isSprinting ? 2f : 1f;

            animator.SetBool("isMoving", isMoving);
            animator.SetBool("isSprinting", isSprinting);
            animator.SetFloat("afkTime", afkTime);
            animator.SetFloat("forward", smoothInput.y * speedMultiplier);
            animator.SetFloat("strafe", smoothInput.x * speedMultiplier);
        }


    }
}
