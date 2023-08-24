using UnityEngine;

namespace Runtime.Câmera.MainCamera
{
    public class WallTransparent : MonoBehaviour
    {
        public Material[] newMaterials;
        private Material[] _originalMaterials;
        private Renderer _objectRenderer;

        private void Start()
        {
            _objectRenderer = GetComponent<Renderer>();
            _originalMaterials = _objectRenderer.materials;
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
                ChangeMaterials(_originalMaterials);
            }
        }

        private void ChangeMaterials(Material[] materials)
        {
            _objectRenderer.materials = materials;
        }
    }
}