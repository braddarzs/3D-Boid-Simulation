using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    const int threadGroupSize = 1024;
    public List<Boid> boids;
    public ComputeShader computeShader;
    public GameObject targetCube;
    public Vector3 bounds = new Vector3(25f, 25f, 25f);

    [Header("Boid Variables")]
    public float viewRadius = 5f;
    public float viewAngle = 120f;
    public float moveSpeed = 5f;
    public float seperationRange = 2f;
    public float separationStrength = 5f;
    public float alignmentStrength = 5f;
    public float cohesionStrength = 5f;
    public float targetStrength = 5f;

    private void Start()
    {
        boids = new List<Boid>();

        GameObject[] boidObjects = GameObject.FindGameObjectsWithTag("Boid");

        foreach (GameObject boid in boidObjects)
        {
            boids.Add(boid.GetComponent<Boid>());
        }
    }

    /*
    private void Update()
    {
        int numBoids = boids.Count;

        BoidData[] boidData = new BoidData[numBoids];

        for(int i = 0; i < numBoids; i++)
        {
            boidData[i].position = boids[i].transform.position;
            boidData[i].direction = boids[i].transform.forward;
        }

        ComputeBuffer boidBuffer = new ComputeBuffer(numBoids, 64);
        boidBuffer.SetData(boidData);
        computeShader.SetBuffer(0, "boids", boidBuffer);
        computeShader.SetInt("numBoids", boids.Count);
        computeShader.SetFloat("viewRadius", viewRadius);
        computeShader.SetFloat("viewAngle", viewAngle);
        computeShader.SetFloat("seperationRadius", seperationRange);

        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);
    }
    */

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, bounds * 2);
    }

    public Vector3 WrapPosition(Vector3 startingPos)
    {
        Vector3 newPos = startingPos;

        if (startingPos.x > bounds.x) newPos.x = -bounds.x;
        else if (startingPos.x < -bounds.x) newPos.x = bounds.x;

        if (startingPos.y > bounds.y) newPos.y = -bounds.y;
        else if (startingPos.y < -bounds.y) newPos.y = bounds.y;

        if (startingPos.z > bounds.z) newPos.z = -bounds.z;
        else if (startingPos.z < -bounds.z) newPos.z = bounds.z;

        return newPos;
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 alignmentDirection;
        public Vector3 cohesionDirection;
        public Vector3 seperationDirection;
        public int numFlockmates;
    }
}
