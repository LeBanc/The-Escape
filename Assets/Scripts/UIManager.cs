using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject hudPanel;
    public Button bipButton;
    public Button openDoorButton;
    public GameObject blockingPlane;
    public TMP_Text playerTurnText;
    public TMP_Text enemyTurnText;
    public TMP_Text winText;
    public TMP_Text looseText;
    public Button retryButton;
    public Button goBackButton;
    public Image fadeToBlack;

    private static UIManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
        GameController.OnBipAvailable += BipButtonAvailable;
        GameController.OnBipUsed += BipButtonUnavailable;
        GameController.OnPlayerTurnStart += ShowPlayerTurn;
        GameController.OnEnemyTurnStart += ShowEnemyTurn;
        GameController.OnPlayerTurnEnd += HideUI;
        GameController.OnWin += WinUI;
        GameController.OnLoose += LooseUI;

        bipButton.interactable = true;
        playerTurnText.enabled = false;
        enemyTurnText.enabled = false;
        winText.enabled = false;
        looseText.enabled = false;
        retryButton.gameObject.SetActive(false);
        goBackButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if(instance == this) instance = null;
        GameController.OnBipAvailable -= BipButtonAvailable;
        GameController.OnBipUsed -= BipButtonUnavailable;
        GameController.OnPlayerTurnStart -= ShowPlayerTurn;
        GameController.OnEnemyTurnStart -= ShowEnemyTurn;
        GameController.OnPlayerTurnEnd -= HideUI;
        GameController.OnWin -= WinUI;
        GameController.OnLoose -= LooseUI;
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

    private void BipButtonAvailable()
    {
        bipButton.interactable = true;
    }

    private void BipButtonUnavailable()
    {
        bipButton.interactable = false;
    }

    private void ShowUI()
    {
        hudPanel.gameObject.SetActive(true);
        blockingPlane.gameObject.SetActive(true);
    }

    private void HideUI()
    {
        hudPanel.gameObject.SetActive(false);
        blockingPlane.gameObject.SetActive(false);
    }

    public static void HideUIWhenMoving()
    {
        instance.HideUI();
    }

    private void ShowPlayerTurn()
    {
        StartCoroutine(PlayerTurnText());
        ShowUI();
    }

    private IEnumerator PlayerTurnText()
    {
        playerTurnText.enabled = true;
        yield return new WaitForSeconds(1.5f);
        playerTurnText.enabled = false;
    }

    private void ShowEnemyTurn()
    {
        StartCoroutine(EnemyTurnText());
    }

    private IEnumerator EnemyTurnText()
    {
        enemyTurnText.enabled = true;
        yield return new WaitForSeconds(1.5f);
        enemyTurnText.enabled = false;
    }

    private void WinUI()
    {
        GameController.OnPlayerTurnStart -= ShowPlayerTurn;
        GameController.OnEnemyTurnStart -= ShowEnemyTurn;
        HideUI();

        // Show win text, restart/menu buttons and fade out to black
        winText.enabled = true;
        retryButton.gameObject.SetActive(true);
        goBackButton.gameObject.SetActive(true);
        StartCoroutine(Fading());
    }

    private void LooseUI()
    {
        GameController.OnPlayerTurnStart -= ShowPlayerTurn;
        GameController.OnEnemyTurnStart -= ShowEnemyTurn;
        HideUI();

        StartCoroutine(Loose());
    }

    IEnumerator Loose()
    {
        yield return new WaitForSeconds(2f);
        // Show loose text, restart/menu buttons and fade out to black
        looseText.enabled = true;
        retryButton.gameObject.SetActive(true);
        goBackButton.gameObject.SetActive(true);
        StartCoroutine(Fading());
    }

    IEnumerator Fading()
    {
        Color _color = fadeToBlack.color;
        while(fadeToBlack.color.a < 1)
        {
            float _a = fadeToBlack.color.a + 0.01f;
            fadeToBlack.color = new Color(_color.r, _color.g, _color.b, _a);
            yield return null;
        }
    }
}
