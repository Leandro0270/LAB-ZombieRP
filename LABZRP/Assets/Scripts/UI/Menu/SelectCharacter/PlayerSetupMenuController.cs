using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{
    private int PlayerIndex;

    [SerializeField] private TextMeshProUGUI titletext;
    [SerializeField] private GameObject CharacterCustomizePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button readyButton;
    [SerializeField] private GameObject ReadyPanel;
    [SerializeField] private CustomizeSkinMenu playerPrefab;
    [SerializeField] private Button firstButton;
    [SerializeField] private List<Material> Skin;
    [SerializeField] private List<Material> Eyes;
    [SerializeField] private List<Material> tshirt;
    [SerializeField] private List<Material> pants;
    [SerializeField] private List<Material> Shoes;


    private int SkinIndex = 0;
    private int EyesIndex = 0;
    private int tshirtIndex = 0;
    private int pantsIndex = 0;
    private int ShoesIndex = 0;

    private float ignoreInputTime = 0.5f;

    private bool inputEnabled;
    
    public void SetPlayerIndex(int pi)
    {
        PlayerIndex = pi;
        titletext.SetText("Player" + (pi + 1).ToString());
        ignoreInputTime = Time.time + ignoreInputTime;
    }

    void Update()
    {
        if (Time.time > ignoreInputTime)
            inputEnabled = true;
    }

    public void SetClass(ScObPlayerStats player)

    {
        if (!inputEnabled)
            return;
        PlayerConfigurationManager.Instance.SetScObPlayerStats(PlayerIndex, player);
        CharacterCustomizePanel.SetActive(true);
        menuPanel.SetActive(false);
        firstButton.Select();
    }
    
    public void CancelSelectClass()
    {
        PlayerConfigurationManager.Instance.SetScObPlayerStats(PlayerIndex, null);
        CharacterCustomizePanel.SetActive(false);
        menuPanel.SetActive(true);
    }
    
   
    public void ReadyPlayer()
    {
        if (!inputEnabled)
            return;
        ScObPlayerCustom ScOb = ScriptableObject.CreateInstance<ScObPlayerCustom>();
        ScOb.Skin = Skin[SkinIndex];
        ScOb.Eyes = Eyes[EyesIndex];
  
        ScOb.tshirt = tshirt[tshirtIndex];
        ScOb.pants = pants[pantsIndex];
        ScOb.Shoes = Shoes[ShoesIndex];
        PlayerConfigurationManager.Instance.SetPlayerSkin(PlayerIndex, ScOb);
        
        PlayerConfigurationManager.Instance.ReadyPlayer(PlayerIndex);
        readyButton.gameObject.SetActive(false);
        CharacterCustomizePanel.SetActive(false);
        ReadyPanel.SetActive(true);
        
    }
    
    //================================================================================================
    //Player Customization
    public void SetNextSkin()
    {
        if (SkinIndex < (Skin.Count - 1))
            SkinIndex++;
        else
        {
            SkinIndex = 0;
        }
        
        playerPrefab.SetSkinMaterial(Skin[SkinIndex]);

    }
    
    
    public void SetNextEyes()
    {Debug.Log("EyesIndex: " + EyesIndex);
        if (EyesIndex < (Eyes.Count - 1))
        {
            
            EyesIndex++;
        }
        else
        {
            EyesIndex = 0;
        }
        playerPrefab.SetEyesMaterial(Eyes[EyesIndex]);

    }
    
    public void SetNextTshirt()
    {
        Debug.Log("ShirtIndex: " + tshirtIndex);
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
        Debug.Log("PantsIndex: " + pantsIndex);
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
        Debug.Log("ShoesIndex: " + ShoesIndex);
        if (ShoesIndex < (Shoes.Count - 1))
        {
            ShoesIndex++;
        }
        else
        {
            ShoesIndex = 0;
        }
        
        playerPrefab.SetShoesMaterial(Shoes[ShoesIndex]);

    }
}
