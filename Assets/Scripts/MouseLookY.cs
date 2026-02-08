using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookY : MonoBehaviour
{
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private float sensitivityVer = .5f;
    private float xRotation = 0f;
    private float minAngle = -50f;
    private float maxAngle = 50f;

    private void OnEnable()
    {
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        lookAction.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = lookAction.action.ReadValue<Vector2>();

        xRotation -= input.y * sensitivityVer;

        xRotation = Mathf.Clamp(xRotation, minAngle, maxAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
