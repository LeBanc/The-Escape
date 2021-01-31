using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Canvas credits;
    public Canvas play;

    public void Play()
    {
        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowCredits()
    {
        credits.enabled = true;
        credits.transform.SetAsLastSibling();
    }

    public void HideCredits()
    {
        credits.enabled = false;
        credits.transform.SetAsFirstSibling();
    }

    public void ShowPlayCanvas()
    {
        play.enabled = true;
        play.transform.SetAsLastSibling();
    }
}
