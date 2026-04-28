using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    private void Awake()
    {
        // This keeps the object alive when SceneManager.LoadScene is called
        DontDestroyOnLoad(gameObject);
    }
}