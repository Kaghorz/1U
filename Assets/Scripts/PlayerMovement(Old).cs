using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementOld : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    private CharacterController charCntr;

    [SerializeField] private float speed = 5f;
    private float gravity = -9.8f;

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
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

        Vector3 movement = new Vector3(input.x, gravity, input.y) * (speed * Time.deltaTime);

        movement = transform.TransformDirection(movement);

        charCntr.Move(movement);
    }
}
