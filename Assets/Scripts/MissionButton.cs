using UnityEngine;

public class MissionButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private string missionName; // e.g., "Exorcism: Yasohachi Bridge"
    [SerializeField] private MissionUIManager uiManager;

    public void OnClick()
    {
        // Debug
        Debug.Log("MissionButton clicked! Target scene: " + targetSceneName + ", Mission name: " + missionName);

        // Pass both strings to the manager
        uiManager.RequestMission(targetSceneName, missionName);
    }
}