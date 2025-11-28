using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class ControllableCamera : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] InputActionReference horizontalInput;
    [SerializeField] InputActionReference upInput;
    [SerializeField] InputActionReference downInput;
    [SerializeField] InputActionReference lookInput;
    [SerializeField] InputActionReference toggleCameraInput;
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float verticalSpeed = 3f;
    [SerializeField] float mouseSensitivity = 0.2f;

    private float verticalMovement = 0f;
    private float yaw = 0f;
    private float pitch = 0f;
    private bool cameraToggled = false;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        upInput.action.performed += OnUpPressed;
        upInput.action.canceled += OnVerticalCanceled;

        downInput.action.performed += OnDownPressed;
        downInput.action.canceled += OnVerticalCanceled;

        toggleCameraInput.action.performed += ToggleCamera;
    }

    private void OnDisable()
    {
        upInput.action.performed -= OnUpPressed;
        upInput.action.canceled -= OnVerticalCanceled;

        downInput.action.performed -= OnDownPressed;
        downInput.action.canceled -= OnVerticalCanceled;

        toggleCameraInput.action.performed -= ToggleCamera;
    }

    private void ToggleCamera(InputAction.CallbackContext ctx)
    {
        cameraToggled = !cameraToggled;

        if (cameraToggled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        if(cameraToggled)
        {
            
            HandleLook();
            HandleMovement();
        }
    }

    private void HandleLook()
    {
        Vector2 delta = lookInput.action.ReadValue<Vector2>();

        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -85f, 85f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleMovement()
    {
        Vector2 input = horizontalInput.action.ReadValue<Vector2>();

        Vector3 move =
            (transform.forward * input.y +
             transform.right * input.x) * moveSpeed;

        move += Vector3.up * (verticalMovement * verticalSpeed);

        controller.Move(move * Time.deltaTime);
    }

    private void OnUpPressed(InputAction.CallbackContext ctx)
    {
        verticalMovement = 1f;
    }

    private void OnDownPressed(InputAction.CallbackContext ctx)
    {
        verticalMovement = -1f;
    }

    private void OnVerticalCanceled(InputAction.CallbackContext ctx)
    {
        verticalMovement = 0f;
    }
}