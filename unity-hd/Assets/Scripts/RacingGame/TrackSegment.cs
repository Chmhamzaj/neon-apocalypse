using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class TrackSegment : MonoBehaviour
    {
        [SerializeField] private float length = 100f;
        [SerializeField] private float roadWidth = 11f;
        [SerializeField] private int lanes = 3;
        [SerializeField] private Transform sceneryAnchor;
        public float Length => length;
        public float RoadWidth => roadWidth;
        public int Lanes => lanes;
        public Transform SceneryAnchor => sceneryAnchor;
        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0f, length * 0.5f), new Vector3(roadWidth, 0.1f, length));
        }
    }
}
