#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NitroStreet.Editor
{
    public sealed class HDAssetPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            var importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.importBlendShapes = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.generateSecondaryUV = true;
        }

        private void OnPostprocessModel(GameObject root)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (!material) continue;
                    if (material.shader == null || material.shader.name == "Standard")
                        material.shader = Shader.Find("Universal Render Pipeline/Lit");
                }
            }
        }
    }
}
#endif