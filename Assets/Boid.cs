using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{

    [SerializeField] float viewRadius;
    [SerializeField] float viewAngle;

    private void Seperation(Boid otherBoid)
    {
        Vector3 vectorDistance = otherBoid.transform.position - transform.position;
        float magnitudeDistance = vectorDistance.magnitude;

        if (magnitudeDistance < viewRadius)
        {
            float angle = Vector3.Angle(transform.forward, vectorDistance);
            if (angle < viewAngle * 0.5f)
            {

            }
        }
    }
    

    private void Update()
    {
        List<Boid> boids = Events.GetBoids?.Invoke();

        foreach(Boid boid in boids)
        {
            Seperation(boid);
        }
    }
}
