using UnityEngine;
using System.Collections.Generic;

public class CarManager : MonoBehaviour
{
    public BetterList<Car> cars;

    private GameObject player;//Awake()
    private static CarManager instance;

    public CarManager()
    {
        cars = new BetterList<Car>();
        instance = this;
    }

    public static CarManager getInstance()
    {
        return instance;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public class Car
    {
        public Vector3 velocity;
        public GameObject entity;

        public Car(GameObject car, Vector3 vel)
        {
            entity = car;
            velocity = vel;
        }

        public Car(Car _car)
        {
            entity = _car.entity;
            velocity = _car.velocity;
        }

        public Car()
        {
            entity = new GameObject();
            velocity = Vector3.zero;
        }
    }

    void Update()
    {
        IEnumerator<CarManager.Car> carsIE = cars.GetEnumerator();
        while (carsIE.MoveNext())
        {
            carsIE.Current.entity.transform.position += carsIE.Current.velocity * Time.deltaTime;
            if (carsIE.Current.entity.transform.position.z < player.transform.position.z - 60)
            {
                GameObject.Destroy(carsIE.Current.entity);
                cars.Remove(carsIE.Current);
            }
        }

        /*
        for (int i = 0; i < cars.size; i++)
        {
            cars[i].entity.transform.position += cars[i].velocity * Time.deltaTime;
            if (cars[i].entity.transform.position.z < player.transform.position.z - 30)
            {
                GameObject.Destroy(cars[i].entity);
                cars.RemoveAt(i);
            }
        }
        */
    }

    public void AddCar(GameObject car, Vector3 velocity)
    {
        Car go = new Car(car, velocity);
        cars.Add(go);
    }

    public void AddCar(Car car)
    {
        Car go = new Car(car);
        cars.Add(go);
    }
}