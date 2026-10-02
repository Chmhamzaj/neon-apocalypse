#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace NeonApocalypse.EditorTools
{
    public sealed class VisualAssetImportPolicy : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/", System.StringComparison.Ordinal)) return;
            if (assetPath.Contains("GeneratedTextures/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = true;
            importer.isReadable = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = assetPath.Contains("4K") ? 4096 : 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;

            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = assetPath.Contains("4K") ? 4096 : 2048;
            android.format = ASTCFormat(assetPath);
            android.compressionQuality = 70;
            importer.SetPlatformTextureSettings(android);
        }

        private static TextureImporterFormat ASTCFormat(string path)
        {
            if (path.Contains("HDMaterials")) return TextureImporterFormat.ASTC_6x6;
            return TextureImporterFormat.ASTC_6x6;
        }
    }
}
#endif
