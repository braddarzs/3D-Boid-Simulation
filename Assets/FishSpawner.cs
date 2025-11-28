using UnityEngine;
using UnityEngine.InputSystem;

public class FishSpawner : MonoBehaviour
{
    [SerializeField] BoidManager boidManager;
    [SerializeField] InputActionReference SpawnFishInput;
    [SerializeField] InputActionReference SpawnSharkInput;

    [SerializeField] GameObject fishPrefab;
    [SerializeField] GameObject sharkPrefab;

    private void Start()
    {
        for(int i = 0; i < 1000; i++)
        {
            SpawnFish();
        }
    }

    private void OnEnable()
    {
        SpawnFishInput.action.performed += OnSpawnFish;
        SpawnSharkInput.action.performed += OnSpawnShark;
    }

    private void OnDisable()
    {
        SpawnFishInput.action.performed -= OnSpawnFish;
        SpawnSharkInput.action.performed -= OnSpawnShark;
    }

    private void OnSpawnFish(InputAction.CallbackContext ctx)
    {
        SpawnFish();
    }

    private void OnSpawnShark(InputAction.CallbackContext ctx)
    {
        SpawnShark();
    }


    private void SpawnFish()
    {
        if(boidManager.boids.Length > 1300) return;

        Vector3 pos = GetRandomPoint();
        GameObject fish = Instantiate(fishPrefab, pos, Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
        EventBus.Raise(GameEventType.BoidSpawned, new FoodTriggeredEventData());
    }

    private void SpawnShark()
    {
        if (boidManager.boids.Length > 1300) return;
        Vector3 pos = GetRandomPoint();
        GameObject shark = Instantiate(sharkPrefab, pos, Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));
        EventBus.Raise(GameEventType.BoidSpawned, new FoodTriggeredEventData());
    }

    private Vector3 GetRandomPoint()
    {
        Vector3 half = transform.localScale * 0.5f;

        return new Vector3(Random.Range(-half.x, half.x),Random.Range(-half.y, half.y),Random.Range(-half.z, half.z)) + transform.position;
    }
}
