using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Enemy.HorderMode
{
    public class HordeModeGameOverPlayer : MonoBehaviour
    {
        [FormerlySerializedAs("_nickName")] public TextMeshProUGUI nickname;
        public TextMeshProUGUI points;
        public TextMeshProUGUI kills;
        public TextMeshProUGUI downs;
        public TextMeshProUGUI revives;
    
    }
}
