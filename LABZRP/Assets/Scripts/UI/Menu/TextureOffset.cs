using UnityEngine;

public class TextureOffset : MonoBehaviour {

    public float yOffset;
    public float speed;
    public Material material;

    // Update is called once per frame
    void Update() {
        material.mainTextureOffset = new Vector2(0, yOffset);
        yOffset -= speed * Time.deltaTime;
    }
}