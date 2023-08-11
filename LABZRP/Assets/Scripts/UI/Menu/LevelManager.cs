using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviourPunCallbacks
{
    
    public static LevelManager Instance;

    [SerializeField] private GameObject _loaderCanvaObjcect;
    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject WalkingZombieOnSlider;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show loading screen
        _loaderCanvaObjcect.SetActive(true);

        // Start loading the scene
        var loading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        loading.allowSceneActivation = false;

        // While the scene is still loading...
        while (!loading.isDone)
        {
            // Update progress bar
            progressBar.fillAmount = loading.progress;

            // If the scene is loaded
            if (loading.progress >= 0.9f)
            {
                // Hide loading screen
                _loaderCanvaObjcect.SetActive(false);

                // Allow scene activation
                loading.allowSceneActivation = true;

                // Call the RPC to tell everyone this client is ready
                photonView.RPC("ClientReady", RpcTarget.AllBuffered);
            }
            

            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    public void ClientReady()
    {
        // This is where you handle what happens when a client is ready
        // You might want to start a countdown when all clients are ready
    }
}
