using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class AndroidQualityProfile : MonoBehaviour
    {
        public enum Tier { Balanced, High, Ultra }
        [SerializeField] private Tier tier = Tier.High;
        [SerializeField] private LODGroup[] lodGroups;
        public Tier CurrentTier => tier;

        private void Awake()
        {
            if (tier == Tier.Balanced) Application.targetFrameRate = 30;
            else Application.targetFrameRate = 60;
            foreach (var lod in lodGroups)
            {
                if (!lod) continue;
                var distances = tier == Tier.Ultra ? new[] { 0.65f, 0.28f, 0.08f } : new[] { 0.5f, 0.2f, 0.05f };
                lod.SetLODs(new[] {
                    new LOD(distances[0], lod.GetLODs()[0].renderers),
                    new LOD(distances[1], lod.GetLODs()[Mathf.Min(1,lod.GetLODs().Length-1)].renderers)
                });
            }
        }
    }
}
