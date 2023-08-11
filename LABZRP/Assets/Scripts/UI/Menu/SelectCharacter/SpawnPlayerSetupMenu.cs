using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Menu.SelectCharacter
{
    public class SpawnPlayerSetupMenu : MonoBehaviour
    {
        [SerializeField] private PlayerInput input;
        private void Awake()
        {
            PlayerConfigurationManager PCM = FindObjectOfType<PlayerConfigurationManager>();
            if (PCM != null)
            {
                PlayerSetupMenuController menu;
                menu = PCM.setPlayerSetupMenuController(input);
                input.uiInputModule = menu.GetInputSystemUIInputModule(input);
                menu.SetPlayerIndex(input.playerIndex);
                PCM.deletePlayerSetupMenuController();
            }
        }
    }
}
