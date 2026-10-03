using System.Collections.Generic;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceSplinePath : MonoBehaviour
    {
        [SerializeField] private Transform[] controlPoints;
        [SerializeField] private int samplesPerSection = 24;
        private readonly List<float> distances = new();
        private readonly List<Vector3> points = new();
        private float totalLength;
        public float TotalLength => totalLength;
        public int ControlPointCount => controlPoints != null ? controlPoints.Length : 0;

        private void Awake() => Rebuild();
        public void Rebuild()
        {
            distances.Clear(); points.Clear(); totalLength = 0f;
            if (controlPoints == null || controlPoints.Length < 2) return;
            Vector3 previous = Evaluate(0f);
            points.Add(previous); distances.Add(0f);
            int count = Mathf.Max(8, (controlPoints.Length - 1) * samplesPerSection);
            for (int i = 1; i <= count; i++)
            {
                float u = i / (float)count;
                Vector3 current = Evaluate(u);
                totalLength += Vector3.Distance(previous, current);
                points.Add(current); distances.Add(totalLength);
                previous = current;
            }
        }

        public Vector3 GetPoint(float distance)
        {
            if (points.Count == 0) return transform.position;
            distance = Mathf.Clamp(distance, 0f, totalLength);
            int index = FindSegment(distance);
            if (index >= distances.Count - 1) return points[points.Count - 1];
            float a = distances[index], b = distances[index + 1];
            return Vector3.Lerp(points[index], points[index + 1], Mathf.InverseLerp(a, b, distance));
        }

        public Vector3 GetTangent(float distance)
        {
            float delta = Mathf.Max(0.5f, totalLength * 0.0025f);
            Vector3 before = GetPoint(Mathf.Max(0f, distance - delta));
            Vector3 after = GetPoint(Mathf.Min(totalLength, distance + delta));
            Vector3 tangent = after - before;
            return tangent.sqrMagnitude > 0.0001f ? tangent.normalized : transform.forward;
        }

        public Vector3 GetRight(float distance)
        {
            Vector3 tangent = GetTangent(distance);
            Vector3 right = Vector3.Cross(Vector3.up, tangent);
            return right.sqrMagnitude > 0.0001f ? right.normalized : transform.right;
        }

        private int FindSegment(float distance)
        {
            int lo = 0, hi = distances.Count - 1;
            while (lo < hi - 1)
            {
                int mid = (lo + hi) >> 1;
                if (distances[mid] <= distance) lo = mid; else hi = mid;
            }
            return lo;
        }

        private Vector3 Evaluate(float u)
        {
            float scaled = u * (controlPoints.Length - 1);
            int i = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, controlPoints.Length - 2);
            float t = scaled - i;
            Vector3 p0 = controlPoints[Mathf.Max(0, i - 1)].position;
            Vector3 p1 = controlPoints[i].position;
            Vector3 p2 = controlPoints[Mathf.Min(controlPoints.Length - 1, i + 1)].position;
            Vector3 p3 = controlPoints[Mathf.Min(controlPoints.Length - 1, i + 2)].position;
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f*p0 - 5f*p1 + 4f*p2 - p3) * t2 + (-p0 + 3f*p1 - 3f*p2 + p3) * t3);
        }

        private void OnDrawGizmosSelected()
        {
            if (controlPoints == null || controlPoints.Length < 2) return;
            Vector3 last = controlPoints[0].position;
            for (int i = 1; i <= 48; i++)
            {
                Vector3 current = Evaluate(i / 48f);
                Gizmos.DrawLine(last, current);
                last = current;
            }
        }
    }
}
