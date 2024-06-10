using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public float lookSpeedY = 4.0f;
    public float lookSpeedX = 3.0f;
    public Transform playerBody;
    private float xRotation = 0f;

    private CinemachineVirtualCamera virtualCamera;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        virtualCamera = GameManager.Instance.VirtualCamera;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
        playerInputActions.Player.Look.performed += OnLook;
        playerInputActions.Player.Look.canceled += OnLook;
    }

    private void OnDisable()
    {
        playerInputActions.Player.Look.canceled -= OnLook;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Disable();
    }

    private void LateUpdate()
    {
        virtualCamera.transform.rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();

        float mouseX = lookInput.x * lookSpeedX * Time.deltaTime;
        float mouseY = lookInput.y * lookSpeedY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        virtualCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
