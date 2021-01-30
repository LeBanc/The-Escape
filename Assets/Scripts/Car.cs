using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public AudioClip bipClip;
    public AudioClip alarmClip;
    public AudioClip openDoorClip;
    public AudioClip startEngineClip;

    public ParticleSystem bipParticles;
    public ParticleSystem alarmParticles;

    private bool isObjective;
    private bool carAlarm;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public bool IsObjective
    {
        get { return isObjective; }
        set { isObjective = value; }
    }

    public bool CarAlarm
    {
        get { return carAlarm; }
        set { carAlarm = value; }
    }

    public void TryOpeningDoor()
    {
        if(isObjective)
        {
            audioSource.clip = startEngineClip;
        }
        else if(carAlarm)
        {
            audioSource.clip = alarmClip;
            alarmParticles.Play();
        }
        else
        {
            audioSource.clip = openDoorClip;
        }
        audioSource.Play();
    }

    public void OpeningBip()
    {
        audioSource.clip = bipClip;
        audioSource.Play();

        bipParticles.Play();
    }

}
