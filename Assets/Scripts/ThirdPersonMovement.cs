using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("Action References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private Transform cam;

    [Header("Movement Settings")]
    [SerializeField] private float groundCheckDistance = 0.05f;
    //[SerializeField] private LayerMask groundLayer;
    //[SerializeField] private bool isGrounded = true;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;


    private CharacterController charCntr;
    private Animator animator;
    private float gravity = -9.81f;
    private float verticalVelocity = 0f;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        //bool isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.y + right * input.x).normalized;

        //check if the player is moving
        bool isMoving = input.magnitude > 0.1f;
        bool isSprinting = sprintAction.action.IsPressed();


        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isSprinting", isSprinting);


        Vector3 finalVelocity = Vector3.zero;

        if (isMoving)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            //if Shift is pressed, we sprint
            float currentSpeed = isSprinting ? speed * 2 : speed;
            finalVelocity = transform.forward * currentSpeed;
        }

        if (charCntr.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        finalVelocity.y = verticalVelocity;

        charCntr.Move(finalVelocity * Time.deltaTime);
    }
}
