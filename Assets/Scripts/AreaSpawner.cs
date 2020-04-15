using System.Collections.Generic;
using UnityEngine;

public class AreaSpawner : MonoBehaviour
{
    public GameObject[] areaPrefab;
    public GameObject[] rotAreaPrefab;
    public GameObject[] villageAreaPrefab;
    public GameObject[] rotVillageAreaPrefab;
    public GameObject[] rotWaterAreaPrefab;
    public GameObject[] rotForestAreaPrefab;
    public GameObject[] forestAreaPrefab;
    public GameObject[] waterAreaPrefab;
    public GameObject[] waterToRoadAreaPrefab;
    public float delayNextSpawnTime;
    public CarSpawner carSp;
    public GameObject pivot;

    private float mTime;
    private bool pause;
    private BetterList<GameObject> areas;
    private bool inTheForest;
    private bool inTheVillage;
    private bool inTheWater;
    private Transform toDelete;
    private eState state;
    private Transform player;
    private Transform thisTransform;

    enum eState
    {
        General,
        Agreed,
        SpawnRotateArea,
        SpawnLight,
        SpawnBarge,
        Deleting,
        DoNothing,
    }

    void Awake()
    {
        pause = false;
        mTime = delayNextSpawnTime;
    }

    void Init()
    {
        thisTransform = transform;
        state = eState.General;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        areas = new BetterList<GameObject>();
    }

    public void StartFromRoute()
    {
        inTheForest     = false;
        inTheVillage    = false;
        inTheWater      = false;

        Init();

        GameObject go;
        int index;

        Vector3 pos = new Vector3(0, 0, 51);
        index = Random.Range(0, areaPrefab.Length);
        go = GameObject.Instantiate(areaPrefab[index], pos, areaPrefab[index].transform.rotation) as GameObject;
        areas.Add(go);

        pos.z += 120;
        index = Random.Range(0, areaPrefab.Length);
        go = GameObject.Instantiate(areaPrefab[index], pos, areaPrefab[index].transform.rotation) as GameObject;
        areas.Add(go);

        pos.z += 120;
        index = Random.Range(0, areaPrefab.Length);
        go = GameObject.Instantiate(areaPrefab[index], pos, areaPrefab[index].transform.rotation) as GameObject;
        areas.Add(go);
    }

    public void StartFromForest()
    {
        inTheForest = true;
        inTheVillage = true;

        Init();

        GameObject go;
        int index;

        Vector3 pos = new Vector3(0, 0, 51);
        index = Random.Range(0, villageAreaPrefab.Length);
        go = GameObject.Instantiate(villageAreaPrefab[index], pos, villageAreaPrefab[index].transform.rotation) as GameObject;
        areas.Add(go);

        pos.z += 120;
        index = Random.Range(0, villageAreaPrefab.Length);
        go = GameObject.Instantiate(villageAreaPrefab[index], pos, villageAreaPrefab[index].transform.rotation) as GameObject;
        areas.Add(go);

        pos.z += 120;
        index = Random.Range(0, villageAreaPrefab.Length);
        go = GameObject.Instantiate(villageAreaPrefab[index], pos, villageAreaPrefab[index].transform.rotation) as GameObject;
        areas.Add(go);
    }

    void Update()
    {
        if (!pause)
        {
            mTime -= Time.deltaTime;
            if (mTime < 0)
            {
                mTime = delayNextSpawnTime;
                SpawnArea();
            }
        }
    }

    void SpawnArea()
    {
        Vector3 newpos = thisTransform.position;
        newpos.z = player.position.z + 290;
        thisTransform.position = newpos;

        ///for (int i = 0; i < areas.size; i++)
        ///{
            ///if (areas[i].transform.position.z < player.position.z - 60)
            ///{
                ///GameObject.Destroy(areas[i]);
                ///areas.RemoveAt(i);
            ///}
        ///}

        
        IEnumerator<GameObject> it = areas.GetEnumerator();
        while (it.MoveNext())
        {
            if(it.Current.transform.position.z < player.position.z - 60)
            {
                areas.Remove(it.Current);
                GameObject.Destroy(it.Current);
            }
        }
        

        GameObject go;
        int index;
        if (state == eState.Deleting)
        {
            state = eState.DoNothing;
            toDelete.DetachChildren();
            GameObject.Destroy(toDelete.gameObject);
            return;
        }
        if (state == eState.DoNothing)
        {
            return;
        }

        if (inTheWater)
        {
            if (state == eState.SpawnBarge)
            {
                state = eState.General;
                index = Random.Range(0, waterAreaPrefab.Length - 1);
                go = GameObject.Instantiate(waterAreaPrefab[index], thisTransform.position, waterAreaPrefab[index].transform.rotation) as GameObject;
                carSp.StartSpawnBargeCoroutine();
            }
            else
            {
                index = Random.Range(0, waterAreaPrefab.Length);
                go = GameObject.Instantiate(waterAreaPrefab[index], thisTransform.position, waterAreaPrefab[index].transform.rotation) as GameObject;

                if (Random.Range(0, 2) == 1)
                {
                    state = eState.SpawnBarge;
                }
            }
        }
        else if (inTheForest)
        {
            if (state == eState.Agreed)
            {
                state = eState.Deleting;
                mTime *= 2;
                if (inTheVillage)
                {
                    index = Random.Range(0, rotVillageAreaPrefab.Length);
                    go = Instantiate(rotVillageAreaPrefab[index], thisTransform.position, Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
                }
                else
                {
                    index = Random.Range(0, rotForestAreaPrefab.Length);
                    go = GameObject.Instantiate(rotForestAreaPrefab[index], thisTransform.position, Quaternion.Euler(-90f, -90f, 0f)) as GameObject;
                }
                toDelete = go.transform.FindChild("BCarSpawner");

                areas.Add(go);
                return;
            }

            if (inTheVillage)
                index = Random.Range(0, villageAreaPrefab.Length);
            else
                index = Random.Range(0, forestAreaPrefab.Length);

            if (Random.Range(0, 5) == 1)
            {
                state = eState.Agreed;
                carSp.StopAllCoroutines();
                carSp.canSpawn = false;
                carSp.SpawnBonuses = true;
            }

            if (inTheVillage)
                go = GameObject.Instantiate(villageAreaPrefab[index], thisTransform.position, villageAreaPrefab[index].transform.rotation) as GameObject;
            else
                go = GameObject.Instantiate(forestAreaPrefab[index], thisTransform.position, forestAreaPrefab[index].transform.rotation) as GameObject;
        }
        else
        {
            if (state == eState.Agreed)
            {
                carSp.StartCoroutine(carSp.SpawnTrafficLight());
                carSp.SpawnBonuses = false;
                index = Random.Range(0, areaPrefab.Length);
                go = GameObject.Instantiate(areaPrefab[index], thisTransform.position, areaPrefab[index].transform.rotation) as GameObject;
                state = eState.General;
            }
            else if (Random.Range(0, 2) == 1)
            {
                index = Random.Range(0, rotAreaPrefab.Length);
                go = GameObject.Instantiate(rotAreaPrefab[index], thisTransform.position, rotAreaPrefab[index].transform.rotation) as GameObject;
            }
            else if (Random.Range(0, 3) == 1)
            {
                index = Random.Range(0, areaPrefab.Length);
                state = eState.Agreed;
                carSp.StopAllCoroutines();
                carSp.canSpawn = false;
                carSp.SpawnBonuses = true;
                go = GameObject.Instantiate(areaPrefab[index], thisTransform.position, areaPrefab[index].transform.rotation) as GameObject;
            }
            else
            {
                index = 5;//Random.Range(0, areaPrefab.Length);
                go = GameObject.Instantiate(areaPrefab[index], thisTransform.position, areaPrefab[index].transform.rotation) as GameObject;
            }
        }
        if (go)
            areas.Add(go);
        else
            print("debil!");
    }

    public void Left(float z)
    {
        inTheVillage = true;

        mTime = 3.4f;

        GameObject go;
        int index;
        Vector3 newPos = player.position; //игрок уже переставлен в точную позицию
        newPos.x -= 114;
        newPos.z = z - 3.0f;
        newPos.y = 0;

        index = Random.Range(0, villageAreaPrefab.Length);
        go = GameObject.Instantiate(villageAreaPrefab[index], newPos, Quaternion.Euler(270f, 270f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        newPos.x -= 120;

        index = Random.Range(0, villageAreaPrefab.Length);
        go = GameObject.Instantiate(villageAreaPrefab[index], newPos, Quaternion.Euler(270f, 270f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        inTheForest = true;
        state = eState.General;

        newPos = thisTransform.position;
        newPos.x = -3.5f;
        thisTransform.position = newPos;
    }

    public void Right(float z)
    {
        mTime = 3.4f;

        GameObject go;
        int index;
        Vector3 newPos = player.position; //игрок уже переставлен в точную позицию
        newPos.x += 110;
        newPos.z = z;
        newPos.y = 0;

        inTheVillage = true;

        if (inTheForest)
        {
            index = Random.Range(1, areaPrefab.Length);
            go = GameObject.Instantiate(areaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
            areas.Add(go);
            go.transform.parent = pivot.transform;

            newPos.x += 120;

            index = Random.Range(1, areaPrefab.Length);
            go = GameObject.Instantiate(areaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
            areas.Add(go);
            go.transform.parent = pivot.transform;
        }
        else
        {
            index = Random.Range(0, villageAreaPrefab.Length);
            go = GameObject.Instantiate(villageAreaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
            areas.Add(go);
            go.transform.parent = pivot.transform;

            newPos.x += 120;

            index = Random.Range(0, villageAreaPrefab.Length);
            go = GameObject.Instantiate(villageAreaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
            areas.Add(go);
            go.transform.parent = pivot.transform;
        }
        inTheForest = !inTheForest;
        state = eState.General;

        newPos = thisTransform.position;
        newPos.x = 0.0f;
        thisTransform.position = newPos;
    }

    public void RightForest(float z)
    {
        inTheVillage = false;
        inTheForest = true;

        mTime = 3.4f;

        GameObject go;
        int index;
        Vector3 newPos = player.position; //игрок уже переставлен в точную позицию
        newPos.x += 110;
        newPos.z = z;
        newPos.y = 0;

        index = Random.Range(0, forestAreaPrefab.Length);
        go = GameObject.Instantiate(forestAreaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        newPos.x += 120;

        index = Random.Range(0, forestAreaPrefab.Length);
        go = GameObject.Instantiate(forestAreaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        state = eState.General;

        newPos = thisTransform.position;
        newPos.x = 0.0f;
        thisTransform.position = newPos;
    }

    public void LeftForest(float z)
    {
        inTheVillage = false;

        mTime = 3.4f;

        GameObject go;
        int index;
        Vector3 newPos = player.position; //игрок уже переставлен в точную позицию
        newPos.x -= 114;
        newPos.z = z - 3.0f;
        newPos.y = 0;

        index = Random.Range(0, forestAreaPrefab.Length);
        go = GameObject.Instantiate(forestAreaPrefab[index], newPos, Quaternion.Euler(270f, 270f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        newPos.x -= 120;

        index = Random.Range(0, forestAreaPrefab.Length);
        go = GameObject.Instantiate(forestAreaPrefab[index], newPos, Quaternion.Euler(270f, 270f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        inTheForest = true;
        state = eState.General;

        newPos = thisTransform.position;
        newPos.x = -3.5f;
        thisTransform.position = newPos;
    }

    public void RightWater(float z)
    {
        inTheWater = true;

        mTime = 3.4f;

        GameObject go;
        int index;
        Vector3 newPos = new Vector3(); //игрок уже переставлен в точную позицию
        newPos.Set(player.position.x + 110, 0.0f, z);

        index = 0;
        go = GameObject.Instantiate(waterAreaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        newPos.x += 120;

        index = Random.Range(0, waterAreaPrefab.Length);
        go = GameObject.Instantiate(waterAreaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        state = eState.General;

        newPos = thisTransform.position;
        newPos.x = 0.0f;
        thisTransform.position = newPos;
    }

    public void RightFromWaterToRoute(float z)
    {
        inTheWater = false;
        inTheVillage = false;
        inTheForest = false;

        mTime = 3.4f;

        GameObject go;
        int index;
        Vector3 newPos = player.position; //игрок уже переставлен в точную позицию
        newPos.x += 114;
        newPos.z = z;
        newPos.y = 0;

        index = Random.Range(1, areaPrefab.Length);
        go = GameObject.Instantiate(areaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        newPos.x += 120;

        index = Random.Range(1, areaPrefab.Length);
        go = GameObject.Instantiate(areaPrefab[index], newPos, Quaternion.Euler(270f, 90f, 0f)) as GameObject;
        areas.Add(go);
        go.transform.parent = pivot.transform;

        state = eState.General;

        newPos = thisTransform.position;
        newPos.x = 0.0f;
        thisTransform.position = newPos;
    }

    public void Pause()
    {
        pause = true;
    }

    public void Continue()
    {
        pause = false;
    }
}