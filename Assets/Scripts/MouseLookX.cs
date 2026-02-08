using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookX : MonoBehaviour
{
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private float sensitivityHor = .5f;

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

        transform.Rotate(new Vector3(0f, input.x, 0f) * sensitivityHor);
    }
}
