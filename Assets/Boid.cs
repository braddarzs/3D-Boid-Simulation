using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Boid : MonoBehaviour
{
    [SerializeField] BoidManager boidManager;
    Vector3 velocity;

    private void Update()
    {
        int neighborCount = 0;
        List<Boid> boids = boidManager.boids;
        if (boids == null || boids.Count == 0)
            return;

        Vector3 separationForce = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        Vector3 cohesionForce = Vector3.zero;

        foreach (Boid boid in boids)
        {
            Vector3 vectorDistance = transform.position - boid.transform.position;
            float magnitudeDistance = vectorDistance.magnitude;
            float angle = Vector3.Angle(transform.forward, vectorDistance);
            if (boid == this || magnitudeDistance > boidManager.viewRadius || angle > boidManager.viewAngle * 0.5f) continue;

            neighborCount++; 
            if(magnitudeDistance <= boidManager.seperationRange) separationForce += vectorDistance.normalized / magnitudeDistance;
            alignmentForce += boid.transform.forward;
            cohesionForce += boid.transform.position;
        }

        //Initial Direction
        Vector3 desiredDirection = (boidManager.targetCube.transform.position - transform.position).normalized * boidManager.targetStrength;//transform.forward;

        //Seperation 
        desiredDirection += ((separationForce / neighborCount).normalized) * boidManager.separationStrength;

        //Alignment
        desiredDirection += ((alignmentForce / neighborCount).normalized) * boidManager.alignmentStrength;

        //Cohesion
        Vector3 averageNeighbourPos = cohesionForce / neighborCount;
        desiredDirection += ((averageNeighbourPos - transform.position).normalized) * boidManager.cohesionStrength;

        //print(desiredDirection);
        desiredDirection.Normalize();

        velocity = Vector3.Lerp(velocity, desiredDirection * boidManager.moveSpeed, Time.deltaTime * 10f);

        transform.position += velocity * Time.deltaTime;
        transform.position = boidManager.WrapPosition(transform.position);

        transform.forward = velocity.normalized;
    }

}
