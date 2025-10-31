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
    public GameObject targetCube;

    [Header("Boid Variables")]
    ComputeBuffer boidBuffer;
    int threadGroups;

    private void Start()
    {

        GameObject[] boidObjects = GameObject.FindGameObjectsWithTag("Boid");
        boids = new Boid[boidObjects.Length];

        for(int i = 0; i < boids.Length; i++)
        {
            boids[i] = boidObjects[i].GetComponent<Boid>();
        }

        int stride = Marshal.SizeOf(typeof(BoidData));
        boidBuffer = new ComputeBuffer(boids.Length, stride);

        threadGroups = Mathf.CeilToInt(boids.Length / (float)threadGroupSize);

    }

    private void Update()
    {
        BoidData[] boidData = new BoidData[boids.Length];

        for (int i = 0; i < boids.Length; i++)
        {
            boidData[i].position = boids[i].transform.position;
            boidData[i].direction = boids[i].transform.forward;
            boidData[i].viewRadius = boids[i].viewRadius;
            boidData[i].viewAngle = boids[i].viewAngle;
            boidData[i].separationRange = boids[i].seperationRange;
            boidData[i].boidType = boids[i].boidType;
        }

        boidBuffer.SetData(boidData);
        computeShader.SetBuffer(0, "boids", boidBuffer);
        computeShader.SetInt("_BoidCount", boids.Length);

        computeShader.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);

        for (int i = 0; i < boids.Length; i++)
        {
            boids[i].alignmentForce = boidData[i].alignmentForce;
            boids[i].cohesionForce = boidData[i].cohesionForce;
            boids[i].separationForce = boidData[i].seperationForce;
            boids[i].neighborCount = boidData[i].numFlockmates;

            boids[i].UpdateBoid();
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
