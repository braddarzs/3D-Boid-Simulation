using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Boid : MonoBehaviour
{
    [SerializeField] BoidManager boidManager;
    Vector3 velocity;

    public float viewRadius = 5f;
    public float viewAngle = 120f;
    public float moveSpeed = 5f;
    public float seperationRange = 2f;
    public float separationStrength = 5f;
    public float alignmentStrength = 5f;
    public float cohesionStrength = 5f;
    public float targetStrength = 5f;
    public float obstacleAvoidanceDistance = 3f;
    public float turnSpeed = 10f;

    public Vector3 separationForce = Vector3.zero;
    public Vector3 alignmentForce = Vector3.zero;
    public Vector3 cohesionForce = Vector3.zero;
    public int neighborCount = 0;

    public LayerMask obstacleLayer;
    public int boidType;
    public void UpdateBoid()
    {
        //Initial Direction
        Vector3 desiredDirection = (boidManager.targetCube.transform.position - transform.position).normalized * targetStrength;
        if (desiredDirection == Vector3.zero) desiredDirection = transform.forward;

        //Seperation 
        desiredDirection += ((separationForce / neighborCount).normalized) * separationStrength;

        //Alignment
        desiredDirection += ((alignmentForce / neighborCount).normalized) * alignmentStrength;

        //Cohesion
        Vector3 averageNeighbourPos = cohesionForce / neighborCount;
        desiredDirection += ((averageNeighbourPos - transform.position).normalized) * cohesionStrength;

        //Obstacle Avoidance
        desiredDirection = ObjectAvoidanceDirection(desiredDirection, 50);

        desiredDirection.Normalize();

        velocity = Vector3.Lerp(velocity, desiredDirection * moveSpeed, Time.deltaTime * turnSpeed);

        transform.position += velocity * Time.deltaTime;

        transform.forward = velocity.normalized;
    }

    public Vector3 ObjectAvoidanceDirection(Vector3 desiredDir, int maxSamples)
    {
        if (desiredDir == Vector3.zero) return desiredDir;
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, desiredDir, out hit, obstacleAvoidanceDistance, obstacleLayer)) return desiredDir;

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

            if (!Physics.Raycast(transform.position, sampleDir, out hit, obstacleAvoidanceDistance, obstacleLayer))
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