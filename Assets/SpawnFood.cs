using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnFood : MonoBehaviour
{
    [SerializeField] InputActionReference spawnFoodAction;
    [SerializeField] GameObject foodPrefab;
    [SerializeField] private LayerMask spawnLayer;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        spawnFoodAction.action.performed += CreateFood;
    }

    private void OnDisable()
    {
        spawnFoodAction.action.performed -= CreateFood;
    }

    private void CreateFood(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, spawnLayer))
        {
            Instantiate(foodPrefab, hit.point, Quaternion.identity);
            EventBus.Raise(GameEventType.FoodTriggered, new FoodTriggeredEventData());
        }
    }
}
