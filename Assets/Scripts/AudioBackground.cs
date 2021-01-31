using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBackground : MonoBehaviour
{
    private AudioSource audioSource;
    private float spentTime = 0f;
    private float checkTime = 1f;
    private float pitch;
    private float delta;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        spentTime += Time.deltaTime;
        if(spentTime > checkTime)
        {
            pitch = Random.Range(0.6f, 1.2f);
            delta = Mathf.Lerp(audioSource.pitch,pitch,0.5f);
            spentTime = 0f;
            checkTime = Random.Range(1f, 10f);
        }

        if (Mathf.Abs(audioSource.pitch - pitch) > 0.1f)
        {
            if (audioSource.pitch > pitch)
            {
                audioSource.pitch -= delta * Time.deltaTime;
            }
            else
            {
                audioSource.pitch += delta * Time.deltaTime;
            }
        }
    }
}
