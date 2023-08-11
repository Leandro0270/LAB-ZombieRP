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

            if (newMaterials.Length != originalMaterials.Length)
            {
                Debug.LogWarning("O número de materiais originais e novos não é igual. Por favor, verifique se a quantidade de materiais corresponde.");
            }
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