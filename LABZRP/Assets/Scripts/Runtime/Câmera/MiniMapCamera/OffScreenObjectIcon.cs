using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenObjectIcon : MonoBehaviour
{
    public Texture Icon = null;
    public Texture Arrow = null;
    public Color Color = Color.white;
    private Camera camera;
    private OffScreenCameraIcons _offScreenCamera;

    private void Start()
    {
        //find main gameManager by name
        GameObject gameManager = GameObject.Find("GameManager");
        MainGameManager mainGameManager = gameManager.GetComponent<MainGameManager>();
        camera = mainGameManager.getMiniMapCamera();
        _offScreenCamera = camera.GetComponent<OffScreenCameraIcons>();
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
