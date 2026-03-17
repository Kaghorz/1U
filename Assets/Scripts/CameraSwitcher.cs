using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera fpsCamera;
    [SerializeField] private CinemachineCamera tpsCamera;

    [Header("Input")]
    [SerializeField] private InputActionReference switchAction;

    private bool isFPS = true;

    private void OnEnable()
    {
        if (switchAction != null)
            switchAction.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (switchAction != null && switchAction.action.WasPressedThisFrame())
        {
            ToggleCamera();
        }
    }

    private void ToggleCamera()
    {
        isFPS = !isFPS;

        fpsCamera.Priority = isFPS ? 10 : 5;
        tpsCamera.Priority = isFPS ? 5 : 10;
    }
}
