using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public PlayerController player;

    private Vector3 offset;

    private void Awake()
    {
        offset = player.transform.position - transform.position;
    }

    private void Update()
    {
        // Camera movement by keyboard
        //transform.position = transform.position + new Vector3(Input.GetAxis("Horizontal")/10, 0f, Input.GetAxis("Vertical")/10);


        // Camera movement over player
        transform.position = player.transform.position - offset;
    }

}
