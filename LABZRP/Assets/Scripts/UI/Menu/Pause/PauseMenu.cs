using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    private bool GameIsPaused = false;
    public GameObject pauseMenuUI;


    public void EscButton()
    {
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Resume()
    {

        Time.timeScale = 1f;
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);

    }

    public void Pause()
    {

        Time.timeScale = 0f;
        GameIsPaused = true;
        pauseMenuUI.SetActive(true);

    }


    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("PlayerSetup");
    }

}
