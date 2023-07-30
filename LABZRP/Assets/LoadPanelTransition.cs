using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadPanelTransition : MonoBehaviour
{
    [SerializeField] private float _transitionTime = 1f;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private Image _panelImage;
    [SerializeField] private GameObject WhitePlayer;
    [SerializeField] private GameObject WhiteZombie;
    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private float _loadingDotsTime = 0.5f;
    [SerializeField] private int _maxDotsAmount = 3;
    private float currentLoadingTime = 0f;
    private int _dotsAmount = 0;
    private bool showingPanel = false;
    private Color _originalPanelColor;
    public static LoadPanelTransition Instance;
    
    void Start()
    {
        currentLoadingTime = _loadingDotsTime;
        _loadingText.text = "Loading";
    }

    void Update()
    {
        currentLoadingTime += Time.deltaTime;
        
        if (showingPanel)
        {

            if (currentLoadingTime >= _loadingDotsTime)
            {
                if (_dotsAmount >= _maxDotsAmount)
                {
                    _loadingText.text = "Loading";
                    _dotsAmount = 0;
                }
                else
                {
                    _loadingText.text += ".";
                    _dotsAmount++;
                }
                currentLoadingTime = 0f;
            }
        }
    }

    public void setShowingLoadingPanel(bool isShowing)
    {
        isShowing = showingPanel;
        _panelImage.gameObject.SetActive(isShowing);
        _loadingText.gameObject.SetActive(isShowing);
        WhitePlayer.SetActive(isShowing);
        WhiteZombie.SetActive(isShowing);
       
    }

}
