using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    
    
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button onlineButton;
        [SerializeField] private Button localButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject onlineMenuPanel;
    
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void showOptions()
        {
            optionsButton.interactable = false;
            onlineButton.interactable = false;
            localButton.interactable = false;
            quitButton.interactable = false;
            settingsPanel.SetActive(true);
            mainMenuPanel.SetActive(false);

        }

        public void enableOptions()
        {
            mainMenuPanel.SetActive(true);
            optionsButton.interactable = true;
            onlineButton.interactable = true;
            localButton.interactable = true;
            quitButton.interactable = true;
            settingsPanel.SetActive(false);
            onlineMenuPanel.SetActive(false);
        }
  
        public void QuitGame()
        {
            Application.Quit();
        }

        public void showOnlineMenu()
        {
            optionsButton.interactable = false;
            onlineButton.interactable = false;
            localButton.interactable = false;
            quitButton.interactable = false;
            onlineMenuPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
        }
    
}
