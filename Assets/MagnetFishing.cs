using UnityEngine;
using UnityEngine.InputSystem;

public class MagnetFishing : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] InputActionReference horizontalInput;
    [SerializeField] InputActionReference upInput;
    [SerializeField] InputActionReference downInput;
    [SerializeField] InputActionReference magnetToggleInput;

    [Header("Movement Settings")]
    [SerializeField] float horizontalSpeed = 5f;
    [SerializeField] float verticalSpeed = 3f;

    private float verticalMovement = 0f;
    private bool magnetToggled = true;

    private void OnEnable()
    {

        upInput.action.performed += OnUpPressed;
        upInput.action.canceled += OnVerticalCanceled;

        downInput.action.performed += OnDownPressed;
        downInput.action.canceled += OnVerticalCanceled;

        magnetToggleInput.action.performed += ToggleMagnet;
    }

    private void OnDisable()
    {
        upInput.action.performed -= OnUpPressed;
        upInput.action.canceled -= OnVerticalCanceled;

        downInput.action.performed -= OnDownPressed;
        downInput.action.canceled -= OnVerticalCanceled;

        magnetToggleInput.action.performed -= ToggleMagnet;

    }

    private void ToggleMagnet(InputAction.CallbackContext ctx)
    {
        magnetToggled = !magnetToggled;
    }

    private void Update()
    {
        if (!magnetToggled) return;

        Vector2 horizontalMovement = -(horizontalInput.action.ReadValue<Vector2>());

        // Apply movement
        Vector3 move = new Vector3(horizontalMovement.x * horizontalSpeed,
                                   verticalMovement * verticalSpeed,
                                   horizontalMovement.y * horizontalSpeed);

        transform.Translate(move * Time.deltaTime);
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