using UnityEngine;

namespace Runtime.Câmera.MiniMapCamera
{
    public class OffScreenObjectIcon : MonoBehaviour
    {
        public Texture Icon = null;
        public Texture Arrow = null;
        public Color Color = Color.white;
        private Camera mainCamera;
        private OffScreenCameraIcons _offScreenCamera;

        private void Start()
        {
            //find main gameManager by nickname
            GameObject gameManager = GameObject.Find("GameManager");
            MainGameManager mainGameManager = gameManager.GetComponent<MainGameManager>();
            mainCamera = mainGameManager.getMiniMapCamera();
            _offScreenCamera = mainCamera.GetComponent<OffScreenCameraIcons>();
            if (_offScreenCamera)
            {
                _offScreenCamera.AddTrackedObject(this);
            }
        }

        private void OnDestroy()
        {
            if (_offScreenCamera)
            {
                _offScreenCamera.removeTrackedObject(this);
            }
        }
    
    }
}
