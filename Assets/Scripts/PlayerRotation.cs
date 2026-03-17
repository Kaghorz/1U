using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform playerCamera; 

    // Update is called once per frame
    void Update()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        if (input.magnitude > 0.1f)
        {
            Vector3 targetDir = playerCamera.forward;
            
            //zero out the y component to keep the player upright
            targetDir.y = 0;

            if (targetDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
