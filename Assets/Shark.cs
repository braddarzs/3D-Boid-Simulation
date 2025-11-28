using Unity.VisualScripting;
using UnityEngine;

public class Shark : Boid
{

    public override void UpdateTarget(GameEventData eventData)
    {
        //float range = boidSettings.eatRange * boidSettings.eatRange;
        if (target != null) return;

        target = boidManager.boids[Random.Range(0, boidManager.boids.Length - 1)].gameObject;
    }

    public override void TryEatFood()
    {
        if (target == null) return;

        float sqrDist = (target.transform.position - transform.position).sqrMagnitude;
        float eatRangeSqr = boidSettings.eatRange * boidSettings.eatRange;

        if (sqrDist <= eatRangeSqr)
        {
            Destroy(target);
            EventBus.Raise(GameEventType.BoidSpawned, new FoodTriggeredEventData());
            StartCoroutine(EatCooldown());
        }

    }

}
