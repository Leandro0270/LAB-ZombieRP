using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{
    private int PlayerIndex;
    
    [SerializeField] private MultiplayerEventSystem eventSystem;
    [SerializeField] private GameObject pressAnyKeyToJoinText;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private TextMeshProUGUI titletext;
    [SerializeField] private GameObject characterCustomizePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private CustomizeSkinMenu playerPrefab;
    [SerializeField] private Button firstClassButon;
    [SerializeField] private Button firstButton;
    [SerializeField] private List<Material> skin;
    [SerializeField] private List<Material> eyes;
    [SerializeField] private List<Material> tshirt;
    [SerializeField] private List<Material> pants;
    [SerializeField] private List<Material> shoes;
    [SerializeField] private InputSystemUIInputModule inputSystemUiInputModule;
    [SerializeField] private GameObject returnPanel;
    [SerializeField] private GameObject cancelButton;
    [SerializeField] PlayerInput playerInput;
    private PlayerController _playerController;


    private int _skinIndex = 0;
    private int EyesIndex = 0;
    private int tshirtIndex = 0;
    private int pantsIndex = 0;
    private int ShoesIndex = 0;
    [SerializeField] private float ignoreInputTime = 1.5f;
    private bool inputEnabled = false;
    private bool isClassSelected = false;
    private bool isReady = false;


    public void SetPlayerIndex(int pi)
    {

        inputEnabled = false;
        _playerController = new PlayerController();
        playerInput.onActionTriggered += Input_onActionTriggered;
        PlayerIndex = pi;
        titletext.SetText("Player " + (pi + 1));
        ignoreInputTime = Time.time + ignoreInputTime;
        pressAnyKeyToJoinText.SetActive(false);
        joinPanel.SetActive(true);
        
    }

    public MultiplayerEventSystem GetEventSystem()
    {
        return eventSystem;
    }
    public int GetPlayerIndex()
    {
        return PlayerIndex;
    }
    
    public GameObject GetCancelButton()
    {
        return cancelButton;
    }
    
    void Update()
    {
        if (Time.time > ignoreInputTime)
                inputEnabled = true;
    }

    
    public InputSystemUIInputModule GetInputSystemUIInputModule(PlayerInput playerInput)
    {
        this.playerInput = playerInput;
        return inputSystemUiInputModule;
    }
    public void SetClass(ScObPlayerStats player)

    {
        if (!inputEnabled)
            return;
        PlayerConfigurationManager.Instance.SetScObPlayerStats(PlayerIndex, player);
        characterCustomizePanel.SetActive(true);
        menuPanel.SetActive(false);
        firstButton.Select();
        isClassSelected = true;
    }
    
    void cancelSelectClass()
    {
        isClassSelected = false;
        PlayerConfigurationManager.Instance.SetScObPlayerStats(PlayerIndex, null);
        characterCustomizePanel.SetActive(false);
        menuPanel.SetActive(true);
    }
    
   
    public void ReadyPlayer()
    {
        if (!inputEnabled)
            return;
        ScObPlayerCustom ScOb = ScriptableObject.CreateInstance<ScObPlayerCustom>();
        ScOb.Skin = skin[_skinIndex];
        ScOb.Eyes = eyes[EyesIndex];
        ScOb.tshirt = tshirt[tshirtIndex];
        ScOb.pants = pants[pantsIndex];
        ScOb.Shoes = shoes[ShoesIndex];
        PlayerConfigurationManager.Instance.SetPlayerSkin(PlayerIndex, ScOb);
        PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex);
        readyButton.gameObject.SetActive(false);
        characterCustomizePanel.SetActive(false);
        readyPanel.SetActive(true);
        isReady = true;
        
    }
    
    //================================================================================================
    //Player Customization
    public void SetNextSkin()
    {
        if (_skinIndex < (skin.Count - 1))
            _skinIndex++;
        else
        {
            _skinIndex = 0;
        }
        
        playerPrefab.SetSkinMaterial(skin[_skinIndex]);

    }
    
    
    public void SetNextEyes()
    {
        if (EyesIndex < (eyes.Count - 1))
        {
            
            EyesIndex++;
        }
        else
        {
            EyesIndex = 0;
        }
        playerPrefab.SetEyesMaterial(eyes[EyesIndex]);

    }
    
    public void SetNextTshirt()
    {
       
        if (tshirtIndex < (tshirt.Count - 1))
        {
            tshirtIndex++;
        }
        else
        {
            tshirtIndex = 0;
        }
        
        playerPrefab.SetTshirtMaterial(tshirt[tshirtIndex]);

    }
    
    public void SetNextPants()
    {
        if (pantsIndex < (pants.Count - 1))
        {
            pantsIndex++;
        }
        else
        {
            pantsIndex = 0;
        }
        playerPrefab.SetPantsMaterial(pants[pantsIndex]);

    }
    
    public void SetNextShoes()
    {
        if (ShoesIndex < (shoes.Count - 1))
        {
            ShoesIndex++;
        }
        else
        {
            ShoesIndex = 0;
        }
        
        playerPrefab.SetShoesMaterial(shoes[ShoesIndex]);

    }

    private void OnDisable()
    {
        if(playerInput != null)
            playerInput.onActionTriggered -= Input_onActionTriggered;

    }

    private void OnDestroy()
    {
        if(playerInput != null)
            playerInput.onActionTriggered -= Input_onActionTriggered;
    }

    public void SelectFirstClass()
    {
        eventSystem.playerRoot = gameObject;
        eventSystem.SetSelectedGameObject(firstClassButon.gameObject);
    }

    private void Input_onActionTriggered(InputAction.CallbackContext obj)
    {
        if (obj.action.name == _playerController.Menu.Cancel.name)
        {
            OnCancel(obj);
        }
    }

    
    public void OnCancel(InputAction.CallbackContext context)
    {
        if(inputEnabled){
            if (context.started)
            {
                if (isClassSelected && !isReady)
                    cancelSelectClass();
                else if (isReady)
                    PlayerConfigurationManager.Instance.CancelReadyPlayer(PlayerIndex);
                else
                {
                    bool canShow = PlayerConfigurationManager.Instance.ShowReturnMenu(PlayerIndex, this);
                    if (canShow)
                    {
                        eventSystem.playerRoot = returnPanel;
                        eventSystem.SetSelectedGameObject(cancelButton);
                    }
                }
            }
        }
    }
}
