using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class DebrisSpawner : MonoBehaviour
{
    [SerializeField] int maxDebris;
    [SerializeField] float debrisSpawnrate;
    [SerializeField] List<GameObject> debrisPrefabs;
    [SerializeField] List<GameObject> currentDebris = new List<GameObject>();
    private List<Transform> spawnpoints = new List<Transform>();

    private void Start()
    {
        foreach(Transform child in transform)
        {
            spawnpoints.Add(child);
        }

        StartCoroutine(DebrisSpawnRoutine());
    }

    private IEnumerator DebrisSpawnRoutine()
    {
        while(true)
        {
            if(maxDebris > currentDebris.Count)
            {
                GameObject prefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Count)];
                Transform spawnpoint = spawnpoints[Random.Range(0, spawnpoints.Count)];


                GameObject debris = Instantiate(prefab,spawnpoint.position,Quaternion.identity);
                currentDebris.Add(debris);
            }

            yield return new WaitForSeconds(debrisSpawnrate);
        }
    }
}
