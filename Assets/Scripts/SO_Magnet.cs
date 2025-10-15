using UnityEngine;

[CreateAssetMenu(fileName = "SO_Magnet", menuName = "Scriptable Objects/SO_Magnet")]
public class SO_Magnet : ScriptableObject
{
    public Poles pole;

    public float magneticForce;
}
