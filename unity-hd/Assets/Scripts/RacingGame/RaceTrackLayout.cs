using UnityEngine;
using System.Collections.Generic;

namespace NitroStreetRush.Racing
{
    public class RaceTrackLayout : MonoBehaviour
    {
        [System.Serializable]
        public struct Segment
        {
            public Transform center;
            public float length;
            public float roadWidth;
            public int lanes;
        }

        [SerializeField] private Segment[] segments;
        [SerializeField] private float laneWidth = 3.5f;
        [SerializeField] private float totalRaceDistance = 2000f;

        public float TotalRaceDistance => totalRaceDistance;
        public float LaneWidth => laneWidth;
        public IReadOnlyList<Segment> Segments => segments;

        public Vector3 GetLanePosition(Vector3 roadCenter, int lane)
        {
            float offset = (lane - 1) * laneWidth;
            return roadCenter + transform.right * offset;
        }

        public void ConfigureDefaults(float distance = 2000f, int laneCount = 3)
        {
            totalRaceDistance = distance;
            laneWidth = 3.5f;
        }
    }
}
