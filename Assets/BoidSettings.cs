using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Scriptable Objects/BoidSettings")]
public class BoidSettings : ScriptableObject
{
    public float TurnRate = 8f;
    public float viewRadius = 5f;
    public float viewAngle = 120f;
    public float minSpeed = 3f;
    public float maxSpeed = 6f;
    public float seperationRange = 2f;
    public float separationStrength = 5f;
    public float alignmentStrength = 5f;
    public float cohesionStrength = 5f;
    public float targetStrength = 5f;
    public float objectAvoidanceStrength = 5f;
    public float obstacleAvoidanceDistance = 3f;
    public float turnSpeed = 10f;
    public float eatRange = 3f;
    public float eatCooldown = 2f;
    public float predatorRange = 3f;
    public float predatorStrength = 5f;
}
