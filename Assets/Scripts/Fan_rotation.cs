using UnityEngine;

public class Fan_rotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, 0, 0);
    }
}
