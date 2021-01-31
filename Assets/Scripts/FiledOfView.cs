using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiledOfView : MonoBehaviour
{

    public delegate void FovEventHandler(Vector3 _position);
    public event FovEventHandler OnPlayerEnter;
    

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke(other.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke(other.transform.position);
        }
    }
}
