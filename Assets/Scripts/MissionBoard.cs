using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissionBoard : MonoBehaviour
{
    [SerializeField] private GameObject missionUIManager;
    [SerializeField] private GameObject boardCamera;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject player;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference exitAction;

    private bool isInteracting = false;
    private bool isPlayerInRange = false;

    void Start()
    {
        if (missionUIManager != null) missionUIManager.SetActive(false);
        if (boardCamera != null) boardCamera.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInRange && interactAction.action.WasPressedThisFrame() && !isInteracting)
        {
            EnterBoardView();
        }
        else if (isInteracting && exitAction.action.WasPressedThisFrame())
        {
            ExitBoardView();
        }
    }

    public void EnterBoardView()
    {
        isInteracting = true;

        boardCamera.SetActive(true); // Cinemachine will see this camera is now active and has higher priority
        boardCamera.GetComponent<CinemachineCamera>().Priority = 100; 

        // UI logic
        playerHUD.SetActive(false);
        player.GetComponent<PlayerController_v3>().enabled = false;
        missionUIManager.SetActive(true);

        player.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z - 2f); // Position player in front of the board
        
        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitBoardView()
    {
        isInteracting = false;

        boardCamera.GetComponent<CinemachineCamera>().Priority = 0;
        boardCamera.SetActive(false);

        // UI logic
        playerHUD.SetActive(true);
        player.GetComponent<PlayerController_v3>().enabled = true;
        if (missionUIManager != null) missionUIManager.SetActive(false);

        // Restore cursor state
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Prevents the "Selection" highlight from staying on the last button hovered
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) isPlayerInRange = false; }
}