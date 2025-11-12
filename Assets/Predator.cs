using UnityEngine;

public class Predator : MonoBehaviour
{
    private Boid boid;

    public float detectionRange = 20f;
    public float eatRange = 3f;
    public LayerMask preyLayer;


    private void Start()
    {
        boid = GetComponent<Boid>();
    }


    void Update()
    {
        boid.target = FindTarget();

        TryEat();
    }



    GameObject FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, preyLayer);

        if (hits.Length == 0)
        {
            return null;
        }

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (var hit in hits)
        {
            if (hit.transform == transform)
                continue;

            float dist = Vector3.SqrMagnitude(hit.transform.position - transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = hit.transform;
            }
        }

        return closest.gameObject;
    }

    void TryEat()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, eatRange, preyLayer))
        {
            Boid prey = hit.collider.GetComponent<Boid>();
            if (prey != null)
            {
                Eat(prey);
            }
        }
    }

    void Eat(Boid prey)
    {
        Debug.Log($"{name} ate {prey.name}");

        Destroy(prey.gameObject);
    }

}
