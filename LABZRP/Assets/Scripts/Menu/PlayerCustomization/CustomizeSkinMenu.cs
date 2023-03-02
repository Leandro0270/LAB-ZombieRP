using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizeSkinMenu : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] SkinMesh;
    [SerializeField] private MeshRenderer EyesMesh;
    [SerializeField] private MeshRenderer[] HandsMesh;
    [SerializeField] private MeshRenderer BodyMesh;
    [SerializeField] private MeshRenderer[] ShoesMesh;

    
    
    public void SetTshirtMaterial(Material material)
    {
        BodyMesh.material = material;
        Material[] AuxMaterials = HandsMesh[0].materials;
        AuxMaterials[1] = material;
       HandsMesh[0].materials = AuxMaterials;
        HandsMesh[1].materials = AuxMaterials;
    }
    
    public void SetPantsMaterial(Material material)
    {
        Material[] AuxMaterials = BodyMesh.materials;
        AuxMaterials[1] = material;
        BodyMesh.materials = AuxMaterials;
    }
    
    public void SetShoesMaterial(Material material)
    {
        ShoesMesh[0].material = material;
        ShoesMesh[1].material = material;
    }
    
    public void SetEyesMaterial(Material material)
    {
        EyesMesh.material = material;
    }
    
    public void SetSkinMaterial(Material material)
    {
        SkinMesh[0].material = material;
        Material [] AuxMaterials = SkinMesh[1].materials;
        AuxMaterials[0] = material;
        SkinMesh[1].materials = AuxMaterials;
        SkinMesh[2].materials = AuxMaterials;
    }



}
