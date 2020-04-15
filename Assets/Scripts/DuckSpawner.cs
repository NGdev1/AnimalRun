//8.07.2015
using UnityEngine;
using System.Collections;

public class DuckSpawner : MonoBehaviour {

    public GameObject DuckPrefab;
    public GameObject SeaGullPrefab;
    public CarManager duckManager;
    public int duckSpeed;

    private Transform player;
    private Transform thisTransform;
    private Transform lastSpawned;
    private float delayNextSpawn;
    private bool spawnDuck;

    [System.NonSerialized]
    public bool canSpawn;

	void Start () {
        canSpawn        = false;
        spawnDuck       = true;
        delayNextSpawn  = 5;
        player          = GameObject.FindGameObjectWithTag("Player").transform;
        thisTransform   = transform;
	}
	
	void Update () {
	    if(canSpawn)
        {
            if (lastSpawned)
            {
                if (BckgrndCars.Distance(thisTransform.position, lastSpawned.position) > delayNextSpawn)
                {
                    SpawnDuck();
                }
            }
            else SpawnDuck();
        }
	}

    public void StopSpawningDucks()
    {
        canSpawn = false;
    }

    public void BeginSpawningDucks()
    {
        Vector3 pos = player.position;

        if(spawnDuck)
            pos.y = 25;
        else
            pos.y = -6.7f;

        float z = pos.z + 300;
        while (pos.z < z)
        {
            pos.z += Random.Range(5, 30);

            pos.x = Random.Range(-5, 5);

            if(spawnDuck)
                lastSpawned = (GameObject.Instantiate(DuckPrefab, pos, DuckPrefab.transform.rotation) as GameObject).transform;
            else
                lastSpawned = (GameObject.Instantiate(SeaGullPrefab, pos, SeaGullPrefab.transform.rotation) as GameObject).transform;

            duckManager.AddCar(lastSpawned.gameObject, new Vector3(0, 0, -duckSpeed));
        }

        thisTransform.position = pos;
        delayNextSpawn = Random.Range(0, 20);
        canSpawn = true;
    }

    void SpawnDuck()
    {
        Vector3 pos = player.position;

        if (spawnDuck)
            pos.y = 25;
        else
            pos.y = -6.7f;

        pos.z += 300;
        thisTransform.position = pos;

        pos.x = Random.Range(-5, 5);

        if (spawnDuck)
            lastSpawned = (GameObject.Instantiate(DuckPrefab, pos, DuckPrefab.transform.rotation) as GameObject).transform;
        else
            lastSpawned = (GameObject.Instantiate(SeaGullPrefab, pos, SeaGullPrefab.transform.rotation) as GameObject).transform;

        duckManager.AddCar(lastSpawned.gameObject, new Vector3(0, 0, -duckSpeed));

        delayNextSpawn = Random.Range(0, 20);
    }

    public void changeBird(bool isDuck)
    {
        spawnDuck = isDuck;

        if (spawnDuck)
            delayNextSpawn = 5;
        else
            delayNextSpawn = 10;
    }
}
