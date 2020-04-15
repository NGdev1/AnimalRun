using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Type
{
    car,
    big,
    mega,
    megamega,
    lift,
    barrier,
    moto,
    empty,
    Fbig,
    drain,
    animal,
    tree,
    roll,
    boat,
    barge,
    human
}

public class CarData
{
    public Type type = Type.car;
    public float carspeed;
    public float dist;
    public int bonusId = -1;
}

public class LineData
{
    public int count;
    public CarData[] carData;

    public LineData(int _count)
    {
        count = _count;
        carData = new CarData[count];
    }
}

public class SpawnData
{
    public LineData[] lineData;
    public float time = 0;

    public SpawnData()
    {
        lineData = new LineData[4];
    }
}

public class SpawnSequence
{
    public int count;
    public SpawnData[] spawnData;
    public bool free;

    public SpawnSequence(int _count)
    {
        count = _count;
        spawnData = new SpawnData[count];
    }
}

public class CarSpawner : MonoBehaviour
{
    private SpawnSequence[] spawnSeq;
    private SpawnSequence[] forestSeq;

    public List<Transform> spawnPoints;
    public GameObject[] carPrefab;
    public GameObject[] boatPrefab;
    public GameObject bargePrefab;
    public GameObject[] bonusPrefab;
    public GameObject[] stuffPrefab;
    public GameObject[] motoPrefab;
    public float speed;
    public GameObject accident;
    public GameObject TrafficLight;
    public GameObject empty;

    public GameObject carEngineSound;
    public GameObject bigCarEngineSound;
    public GameObject carBeepSound;
    public GameObject bigCarBeepSound;
    public GameObject scooterSound;

    [System.NonSerialized]
    public bool inTheForest;
    [System.NonSerialized]
    public bool inTheWater;
    [System.NonSerialized]
    public bool canSpawn;
    [System.NonSerialized]
    public bool SpawnBonuses;

    private List<Transform> fullSpawnPoints;
    private bool gameOver;
    private int density;
    private CarManager manager;

    private CarManager.Car[] spawnedCar;
    private CarManager.Car[] spawnedBonus;

    public void StartFromForest()
    {
        fullSpawnPoints = new List<Transform>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            fullSpawnPoints.Add(spawnPoints[i]);
        }

        spawnPoints.RemoveAt(spawnPoints.Count - 1);
        inTheForest = true;
        inTheWater = false;
        SpawnBonuses = true;
        density = 50;

        MyAwake();
    }

    public void StartFromRoute()
    {
        fullSpawnPoints = new List<Transform>();
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            fullSpawnPoints.Add(spawnPoints[i]);
        }

        inTheForest = false;
        SpawnBonuses = false;
        density = 100;

        MyAwake();
    }

    void MyAwake()
    {
        spawnedCar = new CarManager.Car[4];
        spawnedBonus = new CarManager.Car[4];
        for (int i = 0; i < 4; i++)
        {
            spawnedCar[i] = new CarManager.Car();
            spawnedCar[i].velocity.z = -speed;
        }
        for (int i = 0; i < 4; i++)
        {
            spawnedBonus[i] = new CarManager.Car();
        }
        manager = gameObject.GetComponent<CarManager>();

        gameOver = false;
        canSpawn = true;

        DataLoader dl = new DataLoader();
        dl.LoadData();
        spawnSeq = dl.spSeq;
        forestSeq = dl.forestSeq;

        spawnStartEntities();
    }

    IEnumerator SpawnNextDataCars()
    {
        canSpawn = false;
        yield return new WaitForSeconds(2.0f);

        int id;
        int curn;
        int i;
        int spSeqId; //или авария или едущая подстава
        if (Random.Range(0, 10) == 9)
        {
            spSeqId = 1;//авария
            SpawnAccident();
        }
        else
        {
            spSeqId = 0;//едущая подстава
            yield return new WaitForSeconds(1.5f);
        }
        id = Random.Range(0, spawnSeq[spSeqId].count);

        for (curn = 0; curn < spawnPoints.Count; curn++)
        {
            for (i = 0; i < spawnSeq[spSeqId].spawnData[id].lineData[curn].count; i++)
                SpawnCar(
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].carspeed,
                curn,
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].dist,
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].type,
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].bonusId);
        }

        for (curn = 0; curn < spawnPoints.Count; curn++)
            spawnedCar[curn].velocity = new Vector3(0, 0, -speed);

        yield return new WaitForSeconds(spawnSeq[spSeqId].spawnData[id].time);
        canSpawn = true;
    }

    public IEnumerator SpawnTrafficLight()
    {
        canSpawn = false;
        int curn;
        int i;
        int spSeqId = 2; //скопление машин на светофоре
        int id = Random.Range(0, spawnSeq[spSeqId].count);

        GameObject go = Instantiate(TrafficLight, spawnPoints[1].position, TrafficLight.transform.rotation) as GameObject;
        go.tag = "Car";
        manager.AddCar(go, Vector3.zero);

        for (curn = 0; curn < spawnPoints.Count; curn++)
        {
            for (i = 0; i < spawnSeq[spSeqId].spawnData[id].lineData[curn].count; i++)
                SpawnCar(
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].carspeed,
                curn,
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].dist,
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].type,
                spawnSeq[spSeqId].spawnData[id].lineData[curn].carData[i].bonusId);
        }

        for (curn = 0; curn < spawnPoints.Count; curn++)
            spawnedCar[curn].velocity = new Vector3(0, 0, -speed);

        yield return new WaitForSeconds(spawnSeq[spSeqId].spawnData[id].time);
        canSpawn = true;
    }

    public void StartSpawnBargeCoroutine()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnBarge());
    }

    private IEnumerator SpawnBarge()
    {
        canSpawn = false;
        yield return new WaitForSeconds(10.0f);

        SpawnCar(5, Random.Range(0, 4), 0, Type.barge, -1);

        yield return new WaitForSeconds(10.0f);
        canSpawn = true;
    }

    IEnumerator SpawnNextForestCars()
    {
        canSpawn = false;
        //yield return new WaitForSeconds(1.5f);

        int id;
        int curn;
        int i;
        int spSeqId = Random.Range(0, forestSeq.Length);
        id = Random.Range(0, forestSeq[spSeqId].count);

        yield return new WaitForSeconds(2.5f);

        for (curn = 0; curn < spawnPoints.Count; curn++)
        {
            for (i = 0; i < forestSeq[spSeqId].spawnData[id].lineData[curn].count; i++)
                SpawnCar(
                forestSeq[spSeqId].spawnData[id].lineData[curn].carData[i].carspeed,
                curn,
                forestSeq[spSeqId].spawnData[id].lineData[curn].carData[i].dist,
                forestSeq[spSeqId].spawnData[id].lineData[curn].carData[i].type,
                forestSeq[spSeqId].spawnData[id].lineData[curn].carData[i].bonusId);
        }

        for (curn = 0; curn < spawnPoints.Count; curn++)
            spawnedCar[curn].velocity = new Vector3(0, 0, -speed);

        yield return new WaitForSeconds(forestSeq[spSeqId].spawnData[id].time);
        canSpawn = true;
    }

    void SpawnNextBonuses()
    {
        for (int curn = 0; curn < spawnPoints.Count; curn++)
        {
            if (spawnedBonus[curn].entity)
            {
                if (spawnedBonus[curn].entity.transform.position.z < transform.position.z - 20)
                {
                    SpawnBonus(curn, Random.Range(0, 70));
                }
            }
            else
            {
                SpawnBonus(curn, Random.Range(0, 70));
            }
        }
    }

    void SpawnNextRandomCars()
    {
        for (int curn = 0; curn < spawnPoints.Count; curn++)
        {
            if (spawnedCar[curn].entity)
            {
                if (spawnedCar[curn].entity.transform.position.z < transform.position.z - density)
                {
                    if (inTheWater)
                    {
                        SpawnCar(5, curn, Random.Range(0, density / 4), Type.boat, -1);
                        return;
                    }

                    if (inTheForest && curn != 1)
                    {
                        SpawnCar(0, curn, Random.Range(0, density), ParseType(Random.Range(8, 20)), -1);
                    }
                    else
                    {
                        SpawnCar(Random.Range(speed, 41 / (-35 / spawnedCar[curn].velocity.z)), curn, Random.Range(0, 60), ParseType(Random.Range(0, 8)), -1);
                    }
                }
            }
            else
            {
                if (inTheWater)
                {
                    SpawnCar(5, curn, Random.Range(0, density / 4), Type.boat, -1);
                    return;
                }

                if (inTheForest && curn != 1)
                {
                    SpawnCar(0, curn, Random.Range(0, density), ParseType(Random.Range(8, 20)), -1);
                }
                else
                {
                    SpawnCar(speed, curn, Random.Range(0, density), ParseType(Random.Range(0, 8)), -1);
                }
            }
        }
    }

    void Update()
    {
        //Debug.Log("gameOver:" + gameOver.ToString() + " inTheForest:" + inTheForest.ToString() + " canSpawn:" + canSpawn.ToString());

        if (!gameOver && canSpawn)
        {
            if (Random.Range(0, 1000) == 100 && !inTheWater)
            {
                if (inTheForest)
                    StartCoroutine(SpawnNextForestCars());
                else
                    StartCoroutine(SpawnNextDataCars());
            }
            else
            {
                SpawnNextRandomCars();
            }
        }
        else if (canSpawn && gameOver)
        {
            SpawnNextRandomCars();
        }
        else if (SpawnBonuses)
        {
            SpawnNextBonuses();
        }

        //speed += 0.005f;
    }

    private void SpawnBonus(int line, float dist)
    {
        Vector3 spawnpos = spawnPoints[line].position;
        spawnpos.z += dist;

        spawnedBonus[line].entity = Instantiate(empty, spawnpos, Quaternion.identity) as GameObject;
        manager.AddCar(spawnedBonus[line]);

        int index;
        if (Random.Range(0, 10) == 1)
            index = Random.Range(0, bonusPrefab.Length);
        else
            index = 1;

        GameObject bonus = Instantiate(bonusPrefab[index], spawnpos, bonusPrefab[index].transform.rotation) as GameObject;
        bonus.transform.parent = spawnedBonus[line].entity.transform;
    }

    private void SpawnCar(float carspeed, int line, float dist, Type carType, int bonusId)
    {
        Vector3 spawnpos = spawnPoints[line].position;
        spawnpos.z += dist;
        int id;

        if (carType == Type.empty)
        {
            spawnedCar[line].entity = Instantiate(empty, spawnpos, Quaternion.identity) as GameObject;
            spawnedCar[line].velocity.z = -carspeed;
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.lift)
        {
            id = Random.Range(0, 2);
            spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity = Vector3.zero;
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if(carType == Type.barrier)
        {
            id = Random.Range(2, 9);
            spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity = Vector3.zero;
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.Fbig)
        {
            id = Random.Range(10, 13);
            spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity = Vector3.zero;
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.roll)
        {
            id = Random.Range(6, 9);
            spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity = Vector3.zero;
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.tree)
        {
            id = 9;
            spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity = Vector3.zero;
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.moto)
        {
            id = Random.Range(0, motoPrefab.Length);
            spawnedCar[line].entity = Instantiate(motoPrefab[id], spawnpos, motoPrefab[id].transform.rotation) as GameObject;

            if (carspeed == 0)
                spawnedCar[line].velocity.z = 0;
            else
            {
                spawnedCar[line].velocity.z = -17;
                spawnedCar[line].entity.tag = "RunScooter";

                NGUITools.AddChild(spawnedCar[line].entity, scooterSound);
            }
            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.boat)
        {
            id = Random.Range(0, boatPrefab.Length);

            spawnedCar[line].entity = Instantiate(boatPrefab[id], spawnpos, boatPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity.z = -carspeed;

            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if (carType == Type.barge)
        {
            spawnedCar[line].entity = Instantiate(bargePrefab, spawnpos, bargePrefab.transform.rotation) as GameObject;
            spawnedCar[line].velocity.z = -carspeed;

            manager.AddCar(spawnedCar[line]);
            return;
        }
        else if(carType == Type.animal)
        {
            id = 13;

            int chickensToSpawn = Random.Range(1, 6);

            for (int i = 0; i < chickensToSpawn; i++)
            {
                spawnpos.x += Random.Range(-1f, 1f);
                spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
                spawnedCar[line].velocity.z = -10;
                manager.AddCar(spawnedCar[line]);
                spawnpos.z -= 1;
            }
            return;
        }
        else if (carType == Type.human)
        {
            int humanToSpawn = Random.Range(1, 3);

            for (int i = 0; i < humanToSpawn; i++)
            {
                id = Random.Range(14, stuffPrefab.Length);
                
                spawnpos.x += Random.Range(-0.3f, 0.3f);
                spawnpos.z -= 20f;
                spawnedCar[line].entity = Instantiate(stuffPrefab[id], spawnpos, stuffPrefab[id].transform.rotation) as GameObject;
                spawnedCar[line].velocity.z = -5;
                manager.AddCar(spawnedCar[line]);
                spawnpos.z -= 1;
            }
            return;
        }
        else
        {
            if (carType == Type.car)
                id = Random.Range(0, 6);
            else if (carType == Type.big)
                id = Random.Range(6, 13);
            else if (carType == Type.mega)
                id = Random.Range(13, 18);
            else
                id = Random.Range(18, carPrefab.Length);

            spawnedCar[line].entity = Instantiate(carPrefab[id], spawnpos, carPrefab[id].transform.rotation) as GameObject;
            spawnedCar[line].velocity.z = -carspeed;

            if (carspeed != 0)
            {
                if (carType == Type.car)
                {
                    NGUITools.AddChild(spawnedCar[line].entity, carBeepSound);
                    NGUITools.AddChild(spawnedCar[line].entity, carEngineSound);
                }
                else
                {
                    NGUITools.AddChild(spawnedCar[line].entity, bigCarBeepSound);
                    NGUITools.AddChild(spawnedCar[line].entity, bigCarEngineSound);
                }
            }

            manager.AddCar(spawnedCar[line]);
        }

        if (bonusId != -1)
        {
            if (carType == Type.car || carType == Type.barrier || carType == Type.moto)
                spawnpos.y += 2;
            else if(carType != Type.empty)
                spawnpos.y += 4;

            GameObject bonus = Instantiate(bonusPrefab[bonusId], spawnpos, bonusPrefab[bonusId].transform.rotation) as GameObject;
            bonus.transform.parent = spawnedCar[line].entity.transform;
            return;
        }
        else
            if (carspeed == 0 && (carType == Type.Fbig || carType == Type.big || carType == Type.mega) && Random.Range(0, 5) == 0)
            {
                spawnpos.y += 4;
                id = Random.Range(0, bonusPrefab.Length);
                GameObject bonus = Instantiate(bonusPrefab[id], spawnpos, bonusPrefab[id].transform.rotation) as GameObject;
                bonus.transform.parent = spawnedCar[line].entity.transform;
                return;
            }
    }

    private void SpawnAccident()
    {
        if (inTheForest) return;
        GameObject go = Instantiate(accident, spawnPoints[1].position, accident.transform.rotation) as GameObject;
        manager.AddCar(go, Vector3.zero);
    }

    public void Left()
    {
        StopAllCoroutines();
        spawnPoints.RemoveAt(0);

        inTheForest = true;
        canSpawn = false;
        SpawnBonuses = true;
        density = 50;
    }

    public void RightWater()
    {
        Vector3 Pos = transform.position;
        Pos.y = -5.1f;
        transform.position = Pos;

        inTheWater = true;
        SpawnBonuses = true;
        density = 1000;
    }

    public void RightFromWaterToRoute()
    {
        StopAllCoroutines();
        Vector3 Pos = transform.position;
        Pos.y = 1.9f;
        transform.position = Pos;

        inTheWater = false;
        canSpawn = false;
        SpawnBonuses = false;
        density = 100;
        speed = 20;
        resetCarSpeedOnLines();
    }

    public void resetCarSpeedOnLines()
    {
        for(int i = 0; i < spawnPoints.Count; i++)
        {
            spawnedCar[i].velocity.z = -speed;
        }
    }

    public void Right()
    {
        StopAllCoroutines();
        if (inTheForest)
        {
            spawnPoints.Clear();
            for (int i = 0; i < fullSpawnPoints.Count; i++)
            {
                spawnPoints.Add(fullSpawnPoints[i]);
            }
            density = 100;
            SpawnBonuses = false;
        }
        else
        {
            spawnPoints.RemoveAt(spawnPoints.Count - 1);
            canSpawn = false;
            SpawnBonuses = true;
            density = 50;
        }
        inTheForest = !inTheForest;
    }

    public void GameOver()
    {
        if (canSpawn)
        {
            gameOver = true;
            if (inTheForest) GameObject.Destroy(gameObject);
        }
        else
        {
            IEnumerator<CarManager.Car> ie = manager.cars.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Current.velocity == Vector3.zero)
                    GameObject.Destroy(gameObject);
            }
        }
    }

    public void spawnStartEntities()
    {
        for (int curn = 0; curn < spawnPoints.Count; curn++)
            if (inTheForest && curn != 1)
            {
                SpawnCar(0, curn, Random.Range(0, 60), ParseType(Random.Range(8, 14)), -1);
            }
            else
            {
                SpawnCar(speed, curn, Random.Range(0, 60), ParseType(Random.Range(0, 8)), -1);
            }
    }

    Type ParseType(int index)//от 0 до 8 и от 8 до 13
    {
        switch (index)
        {
            case 0:
                return Type.car;
            case 1:
                return Type.car;
            case 2:
                return Type.car;
            case 3:
                return Type.big;
            case 4:
                return Type.big;
            case 5:
                return Type.moto;
            case 6:
                return Type.mega;
            case 7:
                return Type.megamega;
            case 8:
                return Type.barrier;
            case 9:
                return Type.lift;
            case 10:
                return Type.Fbig;
            case 11:
                return Type.animal;
            case 12:
                return Type.tree;
            case 13:
                return Type.roll;
            default:
                return Type.human;
            //default:
                //{
                    //Debug.Log("debil");
                    //return Type.car;
                //};
        }
    }

    //Type ParseType(int index)//от 0 до 5 и от 5 до 8
    //{
    //    switch (index)
    //    {
    //        case 0:
    //            return Type.car;
    //        case 1:
    //            return Type.big;
    //        case 2:
    //            return Type.moto;
    //        case 3:
    //            return Type.mega;
    //        case 4:
    //            return Type.megamega;
    //        case 5:
    //            return Type.barrier;
    //        case 6:
    //            return Type.lift;
    //        case 7:
    //            return Type.Fbig;
    //        case 8:
    //            return Type.drain;
    //        default:
    //            {
    //                Debug.Log("debil");
    //                return Type.car;
    //            };
    //    }
    //}
}