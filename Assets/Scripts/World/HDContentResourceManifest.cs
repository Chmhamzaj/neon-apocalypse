using UnityEngine;

namespace NeonApocalypse.World
{
    /// <summary>
    /// Keeps the full HD library inside the Android build while only warming a tiny,
    /// bounded set of materials. Do not use Resources.LoadAll here: that would load
    /// hundreds of megabytes into memory and defeat the mobile streaming strategy.
    /// </summary>
    public sealed class HDContentResourceManifest : MonoBehaviour
    {
        [SerializeField] private bool warmCoreMaterials = true;

        private static readonly string[] CoreMaterials =
        {
            "T_Asphalt_Wet_ALBEDO_2K",
            "T_Concrete_Aged_ALBEDO_2K",
            "T_Metal_Painted_ALBEDO_2K",
            "T_Metal_Rusted_ALBEDO_2K",
            "T_Panel_Future_ALBEDO_2K",
            "T_Rubble_Stone_ALBEDO_2K"
        };

        private readonly Texture2D[] warmCache = new Texture2D[CoreMaterials.Length];

        private void Awake()
        {
            if (!warmCoreMaterials) return;
            for (int i = 0; i < CoreMaterials.Length; i++)
                warmCache[i] = Resources.Load<Texture2D>("HDMaterials/" + CoreMaterials[i]);
        }
    }
}
