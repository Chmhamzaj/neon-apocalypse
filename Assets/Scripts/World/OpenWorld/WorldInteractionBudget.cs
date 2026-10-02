using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    /// <summary>Central, low-frequency interaction scan for mobile-friendly world prompts.</summary>
    public sealed class WorldInteractionBudget : MonoBehaviour
    {
        public Transform player;
        public float scanEvery = 0.12f;
        public float radius = 5.5f;
        public int layerMask = ~0;
        private float nextScan;
        private static readonly Collider[] Hits = new Collider[32];
        public Transform Nearest { get; private set; }
        public float NearestDistanceSqr { get; private set; } = float.PositiveInfinity;

        private void Start()
        {
            if (!player) player = NeonApocalypse.Core.GameManager.Instance?.PlayerTransform;
        }

        private void Update()
        {
            if (Time.time < nextScan) return;
            nextScan = Time.time + Mathf.Max(0.05f, scanEvery);
            if (!player) player = NeonApocalypse.Core.GameManager.Instance?.PlayerTransform;
            if (!player) return;

            int count = Physics.OverlapSphereNonAlloc(player.position, radius, Hits, layerMask, QueryTriggerInteraction.Collide);
            Transform best = null;
            float bestSqr = radius * radius;
            for (int i = 0; i < count; i++)
            {
                Collider hit = Hits[i];
                if (!hit) continue;
                Transform candidate = hit.attachedRigidbody ? hit.attachedRigidbody.transform : hit.transform;
                float d = (candidate.position - player.position).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = candidate; }
            }
            Nearest = best;
            NearestDistanceSqr = best ? bestSqr : float.PositiveInfinity;
        }
    }
}
