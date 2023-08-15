using UnityEngine;

namespace Runtime.Câmera.MainCamera
{
    public class WallTransparent : MonoBehaviour
    {
        public Material[] newMaterials;
        private Material[] originalMaterials;
        private Renderer objectRenderer;

        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            originalMaterials = objectRenderer.materials;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ChangeMaterials(newMaterials);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ChangeMaterials(originalMaterials);
            }
        }

        private void ChangeMaterials(Material[] materials)
        {
            objectRenderer.materials = materials;
        }
    }
}