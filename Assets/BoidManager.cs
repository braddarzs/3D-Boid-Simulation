using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    const int threadGroupSize = 1024;
    public Boid[] boids;
    public ComputeShader computeShader;

    [Header("Boid Variables")]
    ComputeBuffer boidBuffer;
    int threadGroups;
    BoidData[] boidData;

    private void Start()
    {
        GameObject[] boidObjects = GameObject.FindGameObjectsWithTag("Boid");
        boids = new Boid[boidObjects.Length];

        for(int i = 0; i < boids.Length; i++)
        {
            boids[i] = boidObjects[i].GetComponent<Boid>();
        }
        boidData = new BoidData[boids.Length];
        int stride = Marshal.SizeOf(typeof(BoidData));
        boidBuffer = new ComputeBuffer(boids.Length, stride);

        threadGroups = Mathf.CeilToInt(boids.Length / (float)threadGroupSize);

    }

    private void Update()
    {

        GameObject[] boidObjects = GameObject.FindGameObjectsWithTag("Boid");
        boids = new Boid[boidObjects.Length];

        for (int i = 0; i < boids.Length; i++)
        {
            boids[i] = boidObjects[i].GetComponent<Boid>();
        }

        BoidData[] boidData = new BoidData[boids.Length];

        for (int i = 0; i < boids.Length; i++)
        {
            Boid boid = boids[i];
            boidData[i] = new BoidData
            {
                position = boid.transform.position,
                direction = boid.transform.forward,
                viewRadius = boid.boidSettings.viewRadius,
                viewAngle = boid.boidSettings.viewAngle,
                separationRange = boid.boidSettings.seperationRange,
                boidType = boid.boidType
            };
        }

        boidBuffer.SetData(boidData);
        computeShader.SetBuffer(0, "boids", boidBuffer);
        computeShader.SetInt("_BoidCount", boids.Length);

        computeShader.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);

        for (int i = 0; i < boids.Length; i++)
        {
            Boid boid = boids[i];
            boid.alignmentForce = boidData[i].alignmentForce;
            boid.cohesionForce = boidData[i].cohesionForce;
            boid.separationForce = boidData[i].seperationForce;
            boid.neighborCount = boidData[i].numFlockmates;

            boid.UpdateBoid();
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 alignmentForce;
        public Vector3 cohesionForce;
        public Vector3 seperationForce;
        public int numFlockmates;

        public float viewRadius;
        public float viewAngle;
        public float separationRange;

        public int boidType;
    }

    private void OnDestroy()
    {
        if (boidBuffer != null) boidBuffer.Release();
    }
}
