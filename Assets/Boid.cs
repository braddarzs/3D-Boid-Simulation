using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Boid : MonoBehaviour
{
    [SerializeField] BoidManager boidManager;
    Vector3 velocity;
    public BoidSettings boidSettings;
    public Vector3 separationForce = Vector3.zero;
    public Vector3 alignmentForce = Vector3.zero;
    public Vector3 cohesionForce = Vector3.zero;
    public int neighborCount = 0;
    public LayerMask obstacleLayer;
    public int boidType;
    public GameObject target;

    private void OnEnable()
    {
        EventBus.Subscribe(GameEventType.FoodTriggered, UpdateTarget);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe(GameEventType.FoodTriggered, UpdateTarget);
    }

    private void UpdateTarget(GameEventData eventData)
    {
        GameObject[] allFood = GameObject.FindGameObjectsWithTag("Food");

        if (allFood.Length == 0) 
        {
            target = null;
            return;
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;
        Vector3 position = transform.position;

        foreach(GameObject food in allFood)
        {
            float distance = (food.transform.position - position).sqrMagnitude;

            if ((distance < closestDistance) && (distance < 400f))
            {
                closestDistance = distance;
                closestFood = food;
            }
        }
        target = closestFood;
    }

    private void TryEatFood()
    {
        if (target == null) return;

        float sqrDist = (target.transform.position - transform.position).sqrMagnitude;
        float eatRangeSqr = boidSettings.eatRange * boidSettings.eatRange;

        if (sqrDist <= eatRangeSqr)
        {
            Destroy(target);
        }
    }


    public void UpdateBoid()
    {
        UpdateTarget(null);
        TryEatFood();
        Vector3 desiredDirection = Vector3.zero;
        //Initial Direction
        if (target != null) desiredDirection = (target.transform.position - transform.position).normalized * boidSettings.targetStrength;

        if (desiredDirection == Vector3.zero) desiredDirection = transform.forward;

        if(neighborCount > 0)
        {
            //Seperation
            Vector3 seperation = separationForce / neighborCount;
            if (seperation != Vector3.zero)
            {
                seperation = seperation.normalized * boidSettings.separationStrength;
                desiredDirection += seperation;
            }

            //Alignment
            Vector3 alignment = alignmentForce / neighborCount;
            if (alignment != Vector3.zero)
            {
                alignment = alignment.normalized * boidSettings.alignmentStrength;
                desiredDirection += alignment;
            }

            //Cohesion
            Vector3 averageBoidPosition = cohesionForce / neighborCount;
            Vector3 toCenter = averageBoidPosition - transform.position;
            if (toCenter != Vector3.zero)
            {
                Vector3 cohesion = toCenter.normalized * boidSettings.cohesionStrength;
                desiredDirection += cohesion;
            }
        }

        //Obstacle Avoidance
        if (isDirectionObstructed(desiredDirection))
        {
            desiredDirection = ObjectAvoidanceDirection(desiredDirection, 30); //.normalized * boidSettings.objectAvoidanceStrength;
        }

        desiredDirection.Normalize();

        Vector3 desiredVelocity = desiredDirection * boidSettings.maxSpeed;
        Vector3 acceleration = (desiredVelocity - velocity) * boidSettings.turnSpeed;
        velocity += acceleration * Time.deltaTime;

        float speed = velocity.magnitude;
        Vector3 dir = velocity / speed;
        speed = Mathf.Clamp(speed, boidSettings.minSpeed, boidSettings.maxSpeed);
        velocity = dir * speed;

        transform.position += velocity * Time.deltaTime;

        Vector3 lookDirection = velocity  * Time.deltaTime;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Mathf.Clamp01(boidSettings.TurnRate * Time.deltaTime));
    }

    private bool isDirectionObstructed(Vector3 desiredDir)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, desiredDir, out hit, boidSettings.obstacleAvoidanceDistance, obstacleLayer))
        {
            return true;
        }
        return false;
    }

    public Vector3 ObjectAvoidanceDirection(Vector3 desiredDir, int maxSamples)
    {
        if (desiredDir == Vector3.zero) return desiredDir;
        RaycastHit hit;
        float maxDist = boidSettings.obstacleAvoidanceDistance;
        float phi = Mathf.PI * (3f - Mathf.Sqrt(5f));
        Vector3 bestDir = desiredDir;
        float bestScore = -1f;

        for (int i = 0; i < maxSamples; i++)
        {
            float y = 1f - (i / (float)(maxSamples - 1)) * 2f;
            float radius = Mathf.Sqrt(1f - y * y);
            float theta = phi * i;
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;

            Vector3 sampleDir = new Vector3(x, y, z);
            sampleDir = Quaternion.FromToRotation(Vector3.forward, desiredDir) * sampleDir;

            bool sampleHit = Physics.Raycast(transform.position, sampleDir, out hit, maxDist, obstacleLayer);
            //Debug.DrawRay(transform.position, sampleDir.normalized * maxDist, sampleHit ? Color.red : Color.green);

            if (!sampleHit)
            {
                float score = Vector3.Dot(desiredDir, sampleDir);

                if (score > 0.5f) return sampleDir;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDir = sampleDir;
                }
            }
        }
        return bestDir;
    }
}