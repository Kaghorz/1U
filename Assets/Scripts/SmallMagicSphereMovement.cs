using UnityEngine;

public class SmallMagicSphereMovement : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private float orbitSpeed = 100f;
    private Vector3 _dynamicRotation = new Vector3(1, 1, 0);
    private float _randomTimeOffset;
    private Vector3 _randomAxis;

    void Start()
    {
        _randomTimeOffset = Random.Range(0f, 100f);
        _randomAxis = new Vector3(
            Random.Range(-1f, 1f), 
            Random.Range(-1f, 1f), 
            Random.Range(-1f, 1f)
            ).normalized;
    }

    // Update is called once per frame
    void Update()
    {
        if (targetObject != null)
        {
            float randomTime = _randomTimeOffset + Time.time;
            float tilt = Mathf.Sin(randomTime) * .5f;
            _dynamicRotation = (_randomAxis + new Vector3(tilt, 1, 0)).normalized;

            transform.RotateAround(targetObject.position, _dynamicRotation, orbitSpeed * Time.deltaTime);    
        }
    }
}
