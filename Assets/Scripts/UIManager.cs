using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button openDoorButton;

    private static UIManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }

    private void OnDestroy()
    {
        if(instance == this) instance = null;
    }

    private void Start()
    {
        openDoorButton.gameObject.SetActive(false);
    }

    public static void OpenDoor(bool _can, Car _car)
    {
        if(_can)
        {
            instance.openDoorButton.onClick.AddListener(_car.TryOpeningDoor);
            instance.openDoorButton.gameObject.SetActive(true);
        }
        else
        {
            instance.openDoorButton.onClick.RemoveListener(_car.TryOpeningDoor);
            instance.openDoorButton.gameObject.SetActive(false);
        }
        

    }
}
