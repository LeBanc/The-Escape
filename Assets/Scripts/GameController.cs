using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public PlayerController player;

    private static GameController instance;
    private Car objectiveCar;
    private Car[] carArray;

    public static Car[] Cars
    {
        get { return instance.carArray; }
    }

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }

    private void Start()
    {
        carArray = FindObjectsOfType<Car>();
        foreach(Car _car in carArray)
        {
            if (Random.Range(0, 100) >= 85) _car.CarAlarm = true;
        }

        objectiveCar = carArray[Random.Range(0, carArray.Length - 1)];
        objectiveCar.IsObjective = true;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void CarBip()
    {
        objectiveCar.OpeningBip();
    }
}
