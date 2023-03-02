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


    private int SkinIndex = 1;
    private int EyesIndex = 1;
    private int tshirtIndex = 1;
    private int pantsIndex = 1;
    private int ShoesIndex = 1;

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
        Debug.Log("EyesIndex: " + EyesIndex);
        ScOb.Eyes = Eyes[EyesIndex];
        Debug.Log("Eyes: " + tshirtIndex);
        ScOb.tshirt = tshirt[tshirtIndex];
        Debug.Log("Eyes: " + pantsIndex);
        ScOb.pants = pants[pantsIndex];
        Debug.Log("Eyes: " + ShoesIndex);
        ScOb.Shoes = Shoes[ShoesIndex];
        Debug.Log("Eyes: " + ScOb.Shoes);
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
        if (SkinIndex <= Skin.Count - 1)
        {
            playerPrefab.SetSkinMaterial(Skin[SkinIndex]);
            SkinIndex++;
        }
        else
        {
            SkinIndex = 0;
            playerPrefab.SetSkinMaterial(Skin[SkinIndex]);
        }
    }
    
    
    public void SetNextEyes()
    {
        Debug.Log("Seleciono olhos");
        if (EyesIndex <= Eyes.Count - 1)
        {
            playerPrefab.SetEyesMaterial(Eyes[EyesIndex]);
            EyesIndex++;
        }
        else
        {
            EyesIndex = 0;
            playerPrefab.SetEyesMaterial(Eyes[EyesIndex]);
        }
    }
    
    public void SetNextTshirt()
    {
        Debug.Log("Seleciono camisa");
        if (tshirtIndex <= tshirt.Count - 1)
        {
            playerPrefab.SetTshirtMaterial(tshirt[tshirtIndex]);
            tshirtIndex++;
        }
        else
        {
            tshirtIndex = 0;
            playerPrefab.SetTshirtMaterial(tshirt[tshirtIndex]);
        }
    }
    
    public void SetNextPants()
    {
        Debug.Log("Seleciono calças");
        if (pantsIndex <= pants.Count - 1)
        {
            playerPrefab.SetPantsMaterial(pants[pantsIndex]);
            pantsIndex++;
        }
        else
        {
            pantsIndex = 0;
            playerPrefab.SetPantsMaterial(pants[pantsIndex]);
        }
    }
    
    public void SetNextShoes()
    {
        Debug.Log("Seleciono sapatos");
        if (ShoesIndex <= Shoes.Count - 1)
        {
            playerPrefab.SetShoesMaterial(Shoes[ShoesIndex]);
            ShoesIndex++;
        }
        else
        {
            ShoesIndex = 0;
            playerPrefab.SetShoesMaterial(Shoes[ShoesIndex]);
        }
    }
}
