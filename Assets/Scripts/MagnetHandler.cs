using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagnetHandler : MonoBehaviour
{

    private List<Magnet> magnets;
    public static float Permeability = 0.05f;
    public static float MaxForce = 10000.0f;

    public bool UseScaleForDebugDraw;

    void Start()
    {
        magnets = new List<Magnet>();

        GameObject[] magnetObjects = GameObject.FindGameObjectsWithTag("Magnet");

        foreach (GameObject magnet in magnetObjects)
        {
            magnets.Add(magnet.GetComponent<Magnet>());
        }
    }

    Vector3 GetMagnetForce(Magnet magnet, Magnet otherMagnet) //Uses Gilberts foruma
    {
        var magnet1Position = magnet.transform.position;
        var magnet2Position = otherMagnet.transform.position;
        var positionDifference = magnet2Position - magnet1Position;
        var distance = positionDifference.magnitude;
        var numerator = Permeability * magnet.magnetData.magneticForce * otherMagnet.magnetData.magneticForce;
        var denominator = 4 * Mathf.PI * distance;

        var formula = (numerator / denominator);

        if (magnet.magnetData.pole == otherMagnet.magnetData.pole)
            formula = -formula;

        return formula * positionDifference.normalized;
    }

    void FixedUpdate()
    {
        for (int i = 0; i < magnets.Count; i++)
        {
            var m1 = magnets[i];

            var  magnet1Rigidbody = m1.rigidBody;
            var magnet1Force = Vector3.zero;
            var magnet2Force = Vector3.zero;
            for (int j = 0; j < magnets.Count; j++)
            {
                if (i == j)
                    continue;

                var m2 = magnets[j];

                if (m2.magnetData.magneticForce < 5.0f)
                    continue;

                var f = GetMagnetForce(m1, m2);
                var magnetForce = m1.magnetData.magneticForce * m2.magnetData.magneticForce;

                magnet1Force += f * magnetForce;
            }

            if (magnet1Force.magnitude > MaxForce)
            {
                magnet1Force = magnet1Force.normalized * MaxForce;
            }
            magnet1Rigidbody.AddForceAtPosition(magnet1Force, m1.transform.position);
        }
    }
}
