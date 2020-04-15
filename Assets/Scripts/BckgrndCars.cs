using UnityEngine;

public class BckgrndCars : MonoBehaviour
{
    public GameObject[] spawnPoints;
    public GameObject[] carPrefab;
    public Vector3 velocity;
    public int density;

    public bool canSpawn;

    private GameObject[] spawnedCar;

    void Start()
    {
        canSpawn = true;
        spawnedCar = new GameObject[spawnPoints.Length];

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnCar(velocity, i, Random.Range(0, 60), Random.Range(0, carPrefab.Length - 1));
        }
    }

    void Update()
    {
        if (canSpawn)
        {
            for (int curn = 0; curn < spawnPoints.Length; curn++)
            {
                if (spawnedCar[curn])
                {
                    if (Distance(spawnedCar[curn].transform.position, transform.position) > density)
                    {
                        SpawnCar(velocity, curn, Random.Range(0, 35), Random.Range(0, carPrefab.Length));
                    }
                }
                else
                {
                    SpawnCar(velocity, curn, Random.Range(0, 35), Random.Range(0, carPrefab.Length));
                }
            }
        }
    }

    private void SpawnCar(Vector3 velocity, int line, float dist, int id)
    {
        Vector3 spawnpos = spawnPoints[line].transform.position;
        Quaternion rotation;

        if (velocity.z == 0)
        {
            spawnpos.x -= dist;
            if (velocity.x > 0)
                rotation = Quaternion.Euler(270, 0, 0);
            else
                rotation = Quaternion.Euler(270, 180, 0);
        }
        else
        {
            spawnpos.z += dist;
            rotation = carPrefab[id].transform.rotation;
        }

        spawnedCar[line] = Instantiate(carPrefab[id], spawnpos, rotation) as GameObject;
        BckgrndCar bckgrndCar = spawnedCar[line].AddComponent<BckgrndCar>();

        bckgrndCar.velocity = velocity;
        bckgrndCar.startPos = transform.position;
        spawnedCar[line].transform.parent = transform;
        spawnedCar[line].tag = "Car";
    }

    public static int Distance(Vector3 a, Vector3 b)
    {
        return System.Convert.ToInt32(System.Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z)));
    }
}