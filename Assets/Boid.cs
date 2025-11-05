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

    public void UpdateBoid()
    {
        //Initial Direction
        Vector3 desiredDirection = (boidManager.targetCube.transform.position - transform.position).normalized * boidSettings.targetStrength;
        if (desiredDirection == Vector3.zero) desiredDirection = transform.forward;

        //Seperation 
        desiredDirection += (separationForce.normalized) * boidSettings.separationStrength;

        //Alignment
        desiredDirection += (alignmentForce.normalized) * boidSettings.alignmentStrength;

        //Cohesion
        Vector3 averageNeighbourPos = cohesionForce / neighborCount;
        desiredDirection += ((averageNeighbourPos - transform.position).normalized) * boidSettings.cohesionStrength;

        //Obstacle Avoidance
        if(isDirectionObstructed(desiredDirection))
        {
            desiredDirection += ObjectAvoidanceDirection(desiredDirection, 20).normalized * boidSettings.objectAvoidanceStrength;
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
        transform.forward = dir;
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
            float y = 1f - (i / (float)(maxSamples - 1)) * 2f; //Vertical point spread
            float radius = Mathf.Sqrt(1f - y * y); //Horizontal point spread
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