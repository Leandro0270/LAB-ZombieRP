using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OnlinePlayerSetupMenuController : MonoBehaviour
{
    private int PlayerIndex;
    [SerializeField] private OnlinePlayerConfigurationManager playerConfigurationManager;
    [SerializeField] private Button readyClassButton;
    [SerializeField] private TextMeshProUGUI titletext;
    [SerializeField] private GameObject CharacterCustomizePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button readySkinButton;
    [SerializeField] private GameObject ReadyPanel;
    [SerializeField] private CustomizeSkinMenu playerPrefab;
    [SerializeField] private Button firstButton;
    [SerializeField] private List<Material> Skin;
    [SerializeField] private List<Material> Eyes;
    [SerializeField] private List<Material> tshirt;
    [SerializeField] private List<Material> pants;
    [SerializeField] private List<Material> Shoes;


    [SerializeField] private TextMeshProUGUI SkinIndexText;
    [SerializeField] private TextMeshProUGUI EyesIndexText;
    [SerializeField] private TextMeshProUGUI tshirtIndexText;
    [SerializeField] private TextMeshProUGUI pantsIndexText;
    [SerializeField] private TextMeshProUGUI ShoesIndexText;



    private int SkinIndex = 0;
    private int EyesIndex = 0;
    private int tshirtIndex = 0;
    private int pantsIndex = 0;
    private int ShoesIndex = 0;
    private String name;


    [SerializeField] private GameObject OnlineLobbyReady;
    [SerializeField] private GameObject ClientCustomizationPanel;

    private ScObPlayerStats playerStats;

    private OnlinePlayerConfiguration playerConfig;

    [SerializeField] private GameObject playerModel;
    [SerializeField] private Animator playerModelAnimator;
    private bool _isReady;
    public void SetPlayerIndex(int pi, string ClientPlayerName)
    {
        PlayerIndex = pi;
        name = ClientPlayerName;
        titletext.SetText(name);
    }
    
    
    public void SelectClass(ScObPlayerStats playerClass)
    {
        playerStats = playerClass;
        readyClassButton.interactable = true;
    }

    public void OnCancel()
    {
        if (_isReady)
        {
                ClientCustomizationPanel.SetActive(true);
                transform.SetParent(ClientCustomizationPanel.transform);
                OnlineLobbyReady.SetActive(false);
                CharacterCustomizePanel.SetActive(true);
                readySkinButton.gameObject.SetActive(true);
                ReadyPanel.SetActive(false);
                OnlineLobbyReady.SetActive(false);
                _isReady = false;
                OnlinePlayerConfigurationManager.Instance.cancelReadyPlayer(PlayerIndex);
        }
        else if (playerStats != null)
        {
            CancelSelectClass();
        }
        else
        {
            OnlinePlayerConfigurationManager.Instance.showDisconnectWindow();
        }
    }

    public void SetClass()
    {
        Debug.Log("PlayerINdex" +PlayerIndex + "classindex" + playerStats.classIndex);
        playerConfigurationManager.PunSetPlayerStats(PlayerIndex, playerStats.classIndex);
        readyClassButton.gameObject.SetActive(false);
        CharacterCustomizePanel.SetActive(true);
        menuPanel.SetActive(false);
        playerModel.SetActive(true);
        readySkinButton.gameObject.SetActive(true);
        firstButton.Select();
    }

    public void CancelSelectClass()
    {
        playerStats = null;
        CharacterCustomizePanel.SetActive(false);
        readySkinButton.gameObject.SetActive(false);
        readyClassButton.gameObject.SetActive(true);
        menuPanel.SetActive(true);
        playerModel.SetActive(false);

    }


    public void ReadyPlayer()
    {
        ScObPlayerCustom ScOb = ScriptableObject.CreateInstance<ScObPlayerCustom>();
        ScOb.Skin = Skin[SkinIndex];
        ScOb.SkinIndex = SkinIndex;
        ScOb.Eyes = Eyes[EyesIndex];
        ScOb.EyesIndex = EyesIndex;
        ScOb.tshirt = tshirt[tshirtIndex];
        ScOb.tshirtIndex = tshirtIndex;
        ScOb.pants = pants[pantsIndex];
        ScOb.pantsIndex = pantsIndex;
        ScOb.Shoes = Shoes[ShoesIndex];
        ScOb.ShoesIndex = ShoesIndex;
        playerConfigurationManager.PunSetPlayerSkin(PlayerIndex, ScOb);
        playerConfigurationManager.PunReadyPlayer(PlayerIndex);
            

    readySkinButton.gameObject.SetActive(false);
        CharacterCustomizePanel.SetActive(false);
        ReadyPanel.SetActive(true);
        OnlineLobbyReady.SetActive(true);
        ClientCustomizationPanel.SetActive(false);
        transform.SetParent(OnlineLobbyReady.transform);
        _isReady = true;
    }

    
    

    //================================================================================================
    //Player Customization

    public void SetPreviousSkin()
    {
        if (SkinIndex > 0)
            SkinIndex--;
        else
        {
            SkinIndex = (Skin.Count - 1);
        }

        SkinIndexText.SetText((SkinIndex + 1).ToString());
        playerPrefab.SetSkinMaterial(Skin[SkinIndex]);

    }

    public void SetNextSkin()
    {
        if (SkinIndex < (Skin.Count - 1))
            SkinIndex++;
        else
        {
            SkinIndex = 0;
        }

        SkinIndexText.SetText((SkinIndex + 1).ToString());
        playerPrefab.SetSkinMaterial(Skin[SkinIndex]);

    }

    public void SetPreviousEyes()
    {
        if (EyesIndex > 0)
        {
            EyesIndex--;
        }
        else
        {
            EyesIndex = (Eyes.Count - 1);
        }

        EyesIndexText.SetText((EyesIndex + 1).ToString());
        playerPrefab.SetEyesMaterial(Eyes[EyesIndex]);

    }


    public void SetNextEyes()
    {
        if (EyesIndex < (Eyes.Count - 1))
        {

            EyesIndex++;
        }
        else
        {
            EyesIndex = 0;
        }

        EyesIndexText.SetText((EyesIndex + 1).ToString());
        playerPrefab.SetEyesMaterial(Eyes[EyesIndex]);

    }

    public void SetPreviousTshirt()
    {
        if (tshirtIndex > 0)
        {
            tshirtIndex--;
        }
        else
        {
            tshirtIndex = (tshirt.Count - 1);
        }

        tshirtIndexText.SetText((tshirtIndex + 1).ToString());
        playerPrefab.SetTshirtMaterial(tshirt[tshirtIndex]);

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

        tshirtIndexText.SetText((tshirtIndex + 1).ToString());
        playerPrefab.SetTshirtMaterial(tshirt[tshirtIndex]);

    }

    public void SetPreviousPants()
    {
        if (pantsIndex > 0)
        {
            pantsIndex--;
        }
        else
        {
            pantsIndex = (pants.Count - 1);
        }

        pantsIndexText.SetText((pantsIndex + 1).ToString());
        playerPrefab.SetPantsMaterial(pants[pantsIndex]);

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

        pantsIndexText.SetText((pantsIndex + 1).ToString());
        playerPrefab.SetPantsMaterial(pants[pantsIndex]);

    }

    public void SetPreviousShoes()
    {
        if (ShoesIndex > 0)
        {
            ShoesIndex--;
        }
        else
        {
            ShoesIndex = (Shoes.Count - 1);
        }

        ShoesIndexText.SetText((ShoesIndex + 1).ToString());
        playerPrefab.SetShoesMaterial(Shoes[ShoesIndex]);

    }

    public void SetNextShoes()
    {
        if (ShoesIndex < (Shoes.Count - 1))
        {
            ShoesIndex++;
        }
        else
        {
            ShoesIndex = 0;
        }

        ShoesIndexText.SetText((ShoesIndex + 1).ToString());
        playerPrefab.SetShoesMaterial(Shoes[ShoesIndex]);

    }
}
