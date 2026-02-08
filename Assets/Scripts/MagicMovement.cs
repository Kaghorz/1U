using System.Security;
using UnityEngine;

public class MagicMovement : MonoBehaviour
{
    [Header("Magic Sphere Movement Settings")]
    [SerializeField] private float spiralSpeed = 1.5f;
    [SerializeField] private float verticalSpeed = .5f;
    [SerializeField] private float maxHeight = 1f;
    [SerializeField] private float spiralRadius = .5f;
    private float _currentAngle = 0f;
    private float _currentHeight = 0f;
    private bool _isGoingUp = true;
    private Vector3 _startPos;

    void Start()
    {
        _startPos = transform.position;
    }
    
    // Update is called once per frame
    void Update()
    {
        _currentAngle += spiralSpeed * Time.deltaTime;

        if (_isGoingUp)
        {
            _currentHeight += verticalSpeed * Time.deltaTime;

            if (_currentHeight >= maxHeight)
                _isGoingUp = false;
        }
        else
        {
            _currentHeight -= verticalSpeed * Time.deltaTime;

            if (_currentHeight <= 0)
                _isGoingUp = true;
        }

        float x = Mathf.Cos(_currentAngle) * spiralRadius;
        float z = Mathf.Sin(_currentAngle) * spiralRadius;

        transform.position = _startPos + new Vector3(x, _currentHeight, z);
    }
}
