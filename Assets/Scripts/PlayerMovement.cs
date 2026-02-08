using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;
    private CharacterController charCntr;

    [SerializeField] private float speed = 5f;
    private float gravity = -9.8f;

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        //if Shift is pressed, we sprint
        float currentSpeed = sprintAction.action.IsPressed() ? speed * 2: speed;
        
        //firstly, compute the horizontal movement with/without sprinting
        Vector3 horMovement = new Vector3(input.x, 0, input.y);
        horMovement = transform.TransformDirection(horMovement) * (currentSpeed * Time.deltaTime);
        //then, add gravity
        Vector3 gravityMovement = new Vector3(0, gravity, 0) * Time.deltaTime;
        //finally, combine them
        charCntr.Move(horMovement + gravityMovement);
    }
}
