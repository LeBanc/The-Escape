using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footstep : MonoBehaviour
{
    public float steadyVolume;
    public float walkingVolume;
    public float runningVolume;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Ground"))
        {
            audioSource.Stop();
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.Play();
        }
    }

    public void SetVolume(int _phase)
    {
        switch(_phase)
        {
            case 1:
                audioSource.volume = steadyVolume;
                break;
            case 2:
                audioSource.volume = walkingVolume;
                break;
            case 3:
                audioSource.volume = runningVolume;
                break;
            default:
                audioSource.volume = steadyVolume;
                break;
        }
    }
}
