using UnityEngine;
using UnityEngine.InputSystem;

public class DoorRotation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private float openAngle = -80f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private InputActionReference interactAction;

    private bool isOpen = false;
    private bool isPlayerNearby = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
    }

    void Start()
    {
        // Save the starting rotation so we know what "closed" looks like
        closedRotation = transform.rotation;
        targetRotation = closedRotation;
    }

    void Update()
    {
        // Smoothly rotate the door toward the target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);

        // Check for input
        if (isPlayerNearby && interactAction.action.WasPressedThisFrame())
        {
            ToggleDoor();
        }
    }

    private void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            // Rotate the current rotation by the openAngle around the Y axis
            targetRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        }
        else
        {
            targetRotation = closedRotation;
        }
    }

    // Use these with a Trigger Collider on the door or an area
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}