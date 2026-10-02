using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.AI.Tactics
{
    /// <summary>Bounded cover query service. Uses inexpensive probes rather than per-frame path planning.</summary>
    public sealed class TacticalCoverDirector : MonoBehaviour
    {
        public static TacticalCoverDirector Instance { get; private set; }
        public LayerMask obstacleMask = ~0;
        public float candidateRadius = 7f;
        public int maxCandidates = 18;
        private readonly Collider[] overlap = new Collider[32];
        private readonly List<Vector3> candidates = new List<Vector3>(24);

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool TryFindCover(Vector3 seeker, Vector3 threat, Vector3 preferredSide, out Vector3 result)
        {
            result = seeker;
            candidates.Clear();
            Vector3 forward = (threat - seeker); forward.y = 0f;
            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
            for (int i = 0; i < maxCandidates; i++)
            {
                float t = i / Mathf.Max(1f, maxCandidates - 1f);
                float angle = Mathf.Lerp(-150f, 150f, t);
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                float radius = Mathf.Lerp(2.5f, candidateRadius, (i % 5) / 4f);
                Vector3 p = seeker + dir * radius;
                p.y = seeker.y;
                candidates.Add(p);
            }

            float best = float.MinValue;
            Vector3 threatEye = threat + Vector3.up * 1.2f;
            for (int i = 0; i < candidates.Count; i++)
            {
                Vector3 p = candidates[i];
                if (Physics.CheckSphere(p + Vector3.up * 0.9f, 0.45f, obstacleMask, QueryTriggerInteraction.Ignore)) continue;
                Vector3 coverDir = (threatEye - (p + Vector3.up * 1.0f)).normalized;
                if (!Physics.Raycast(p + Vector3.up * 1.0f, coverDir, out RaycastHit hit, 8f, obstacleMask, QueryTriggerInteraction.Ignore)) continue;
                float threatDot = Vector3.Dot((p - threat).normalized, (seeker - threat).normalized);
                float sideScore = preferredSide.sqrMagnitude > 0.01f ? Vector3.Dot((p - seeker).normalized, preferredSide.normalized) : 0f;
                float travel = Vector3.Distance(seeker, p);
                float score = threatDot * 2.4f + sideScore * 0.8f - travel * 0.16f;
                if (score > best) { best = score; result = p; }
            }
            return best > 0.25f;
        }
    }
}
