using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class SceneryAnchor : MonoBehaviour
    {
        public enum Side { Left, Right, Both }
        [SerializeField] private Side side = Side.Both;
        [SerializeField] private float spacing = 14f;
        [SerializeField] private int seedOffset;
        public Side PlacementSide => side;
        public float Spacing => spacing;
        public int SeedOffset => seedOffset;
    }
}
