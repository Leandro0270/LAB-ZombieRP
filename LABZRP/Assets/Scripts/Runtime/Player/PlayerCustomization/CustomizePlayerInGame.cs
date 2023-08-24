using UnityEngine;

namespace Runtime.Player.PlayerCustomization
{
    public class CustomizePlayerInGame : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] SkinMesh;
        [SerializeField] private MeshRenderer EyesMesh;
        [SerializeField] private MeshRenderer[] HandsMesh;
        [SerializeField] private MeshRenderer BodyMesh;
        [SerializeField] private MeshRenderer[] ShoesMesh;
        private ScObPlayerCustom _playerCustom;

    
        public void SetSkin(ScObPlayerCustom playerCustom)
        {
            _playerCustom = playerCustom;
            SetTshirtMaterial(playerCustom.tshirt);
            SetPantsMaterial(playerCustom.pants);
            SetShoesMaterial(playerCustom.Shoes);
            SetEyesMaterial(playerCustom.Eyes);
            SetSkinMaterial(playerCustom.Skin);
        }


        private void SetTshirtMaterial(Material material)
        {
            BodyMesh.material = material;
            Material[] AuxMaterials = HandsMesh[0].materials;
            AuxMaterials[1] = material;
            HandsMesh[0].materials = AuxMaterials;
            HandsMesh[1].materials = AuxMaterials;
        }

        private void SetPantsMaterial(Material material)
        {
            Material[] AuxMaterials = BodyMesh.materials;
            AuxMaterials[1] = material;
            BodyMesh.materials = AuxMaterials;
        }

        private void SetShoesMaterial(Material material)
        {
            ShoesMesh[0].material = material;
            ShoesMesh[1].material = material;
        }

        private void SetEyesMaterial(Material material)
        {
            EyesMesh.material = material;
        }

        private void SetSkinMaterial(Material material)
        {
            SkinMesh[0].material = material;
            Material [] AuxMaterials = SkinMesh[1].materials;
            AuxMaterials[0] = material;
            SkinMesh[1].materials = AuxMaterials;
            SkinMesh[2].materials = AuxMaterials;
        }


        public ScObPlayerCustom GetPlayerCustom()
        {
            return _playerCustom;
        }

    }
}