using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public PlayerController player;

    private static GameController instance;
    private Car objectiveCar;
    private Car[] carArray;

    private Enemy[] enemiesArray;

    private int carBipTurn = 0;

    public delegate void GameControllerEventHandler();
    public static event GameControllerEventHandler OnPlayerTurnStart;
    public static event GameControllerEventHandler OnPlayerTurnEnd;
    public static event GameControllerEventHandler OnEnemyTurnStart;
    public static event GameControllerEventHandler OnEnemyTurnEnd;
    public static event GameControllerEventHandler OnBipUsed;
    public static event GameControllerEventHandler OnBipAvailable;
    public static event GameControllerEventHandler OnWin;
    public static event GameControllerEventHandler OnLoose;


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

        enemiesArray = FindObjectsOfType<Enemy>();

        StartPlayerTurn();
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public void CarBip()
    {
        objectiveCar.OpeningBip();
        carBipTurn = 3;
        OnBipUsed?.Invoke();
        EndPlayerTurn();
    }

    private void StartPlayerTurn()
    {
        if (carBipTurn > 0) carBipTurn--;
        if (carBipTurn == 0) OnBipAvailable?.Invoke();
        OnPlayerTurnStart?.Invoke();
        StartCoroutine(PauseAtPlayerTurn());
    }

    private IEnumerator PauseAtPlayerTurn()
    {
        yield return new WaitForSeconds(1.5f);
        player.StartTurn();
    }    

    public static void EndTurn()
    {
        instance.EndPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        player.EndTurn();
        OnPlayerTurnEnd?.Invoke();
        StartCoroutine(WaitForEnemyTurn());
    }

    private void EnemyTurn()
    {
        OnEnemyTurnStart?.Invoke();
        Enemy.StartEnemyTurn();
        StartCoroutine(EnemyTurnSequence());
    }

    private IEnumerator WaitForEnemyTurn()
    {
        yield return new WaitForSeconds(3f);
        EnemyTurn();
    }
    private IEnumerator EnemyTurnSequence()
    {
        yield return new WaitForSeconds(1.5f);

        foreach (Enemy _enemy in enemiesArray)
        {
            _enemy.StartTurn();
            while(!_enemy.IsStopped)
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.5f);

        Enemy.EndEnemyTurn();
        foreach (Enemy _enemy in enemiesArray)
        {
            _enemy.StopAllCoroutines();
        }

        StartPlayerTurn();
    }

    public static void Win()
    {
        OnWin?.Invoke();
    }

    public static void Loose()
    {
        OnLoose?.Invoke();
    }

    public void Retry()
    {
        SceneManager.LoadScene("Level");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
