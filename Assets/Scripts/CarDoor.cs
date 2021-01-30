using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDoor : MonoBehaviour
{

    private Car car;

    private void Start()
    {
        car = GetComponentInParent<Car>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.OpenDoor(true, car);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.OpenDoor(false, car);
        }
    }

}
