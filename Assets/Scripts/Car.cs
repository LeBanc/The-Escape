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
            GameController.Win();
        }
        else if(carAlarm)
        {
            audioSource.clip = alarmClip;
            alarmParticles.Play();
            GameController.EndTurn();

            Collider[] _colls = Physics.OverlapSphere(transform.position, 10f);
            foreach (Collider _c in _colls)
            {
                if (_c.CompareTag("Enemy"))
                {
                    _c.GetComponent<Enemy>().Alert(transform.position);
                }
            }
        }
        else
        {
            audioSource.clip = openDoorClip;
            GameController.EndTurn();
        }
        audioSource.Play();
    }

    public void OpeningBip()
    {
        audioSource.clip = bipClip;
        audioSource.Play();
        bipParticles.Play();

        Collider[] _colls = Physics.OverlapSphere(transform.position, 8f);
        foreach(Collider _c in _colls)
        {
            if(_c.CompareTag("Enemy"))
            {
                _c.GetComponent<Enemy>().Alert(transform.position);
            }
        }
    }

}
