using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public enum EvidenceType { Gunshot, VehicleImpact, WitnessReport, CameraCapture, Body, Theft }

    [Serializable]
    public struct CrimeEvidence
    {
        public EvidenceType type;
        public Vector3 position;
        public float strength;
        public float timestamp;
        public int faction;
        public int serial;
    }

    /// <summary>Bounded evidence trail used by police and missions. Evidence decays and is cheap to query.</summary>
    public sealed class CrimeEvidenceSystem : MonoBehaviour
    {
        public static CrimeEvidenceSystem Instance { get; private set; }
        public int maxEvidence = 64;
        public float evidenceLifetime = 180f;
        private readonly List<CrimeEvidence> evidence = new List<CrimeEvidence>(64);
        private int serial;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Report(EvidenceType type, Vector3 position, float strength, FactionId faction = FactionId.HelixAuthority)
        {
            evidence.Add(new CrimeEvidence { type = type, position = position, strength = Mathf.Clamp01(strength), timestamp = Time.time, faction = (int)faction, serial = ++serial });
            while (evidence.Count > Mathf.Clamp(maxEvidence, 8, 256)) evidence.RemoveAt(0);
        }

        public void ReportCrime(EvidenceType type, Vector3 position, float strength, float heatAmount, FactionId faction = FactionId.HelixAuthority)
        {
            Report(type, position, strength, faction);
            if (WantedSystem.Instance && heatAmount > 0f) WantedSystem.Instance.AddHeat(heatAmount);
        }

        public bool TryGetStrongestNearby(Vector3 position, float radius, out CrimeEvidence result)
        {
            result = default(CrimeEvidence);
            float best = 0f;
            for (int i = evidence.Count - 1; i >= 0; i--)
            {
                CrimeEvidence e = evidence[i];
                if (Time.time - e.timestamp > evidenceLifetime) { evidence.RemoveAt(i); continue; }
                float d = Vector3.Distance(position, e.position);
                if (d > radius) continue;
                float recency = Mathf.Pow(0.5f, (Time.time - e.timestamp) / 40f);
                float score = e.strength * recency * (1f - d / Mathf.Max(0.1f, radius));
                if (score > best) { best = score; result = e; }
            }
            return best > 0.08f;
        }
    }
}
