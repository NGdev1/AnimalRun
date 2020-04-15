using UnityEngine;
using System.Collections;
using System;

public class BckgrndCar : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 startPos;

    public static BetterList<BckgrndCar> cars = new BetterList<BckgrndCar>();

    public void Start()
    {
        cars.Add(this);
    }

    void Update()
    {
        if (Distance(transform.position, startPos) > 100)
        {
            Destroy(gameObject);
            return;
        }
        gameObject.transform.position += velocity * Time.deltaTime;
    }

    int Distance(Vector3 a, Vector3 b)
    {
        return System.Convert.ToInt32(System.Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z)));
    }

    public void OnDestroy()
    {
        cars.Remove(this);
    }

    public static void DeleteAllBckgrndCars()
    {
        IEnumerator it = cars.GetEnumerator();

        while (it.MoveNext())
        {
            GameObject.Destroy(((BckgrndCar)it.Current).gameObject);
        }

        cars.Clear();
    }

    public static void DeleteAllBckgrndCarSpawners()
    {
        GameObject bcgrndCarSp;

        bcgrndCarSp = GameObject.Find("BCarSpawner");
        //while (bcgrndCarSp != null){ //for some reason it doesn't work
        Destroy(bcgrndCarSp);
        //bcgrndCarSp = GameObject.Find("BCarSpawner");
        //}

        //for (int i = 0; i < 3; i++)
        //{
            //bcgrndCarSp = GameObject.Find("BCarSpawner");
            //Destroy(bcgrndCarSp);
        //}
    }
}