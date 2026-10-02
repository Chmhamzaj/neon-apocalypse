using UnityEngine;

namespace NeonApocalypse.AI
{
    public enum AIStimulusType { Gunshot, Explosion, Impact, Alarm, VehicleCrash, AllyDown }

    public struct AIStimulus
    {
        public Vector3 position;
        public float radius;
        public float danger;
        public float time;
        public AIStimulusType type;
    }

    /// <summary>
    /// Allocation-free radio/hearing bus shared by AI agents.
    /// A bounded ring buffer prevents unbounded world-state growth on mobile.
    /// </summary>
    public static class AIStimulusBus
    {
        private const int Capacity = 96;
        private static readonly AIStimulus[] Buffer = new AIStimulus[Capacity];
        private static int writeIndex;
        private static int count;

        public static void Report(Vector3 position, float radius, float danger, AIStimulusType type)
        {
            Buffer[writeIndex] = new AIStimulus
            {
                position = position,
                radius = Mathf.Max(1f, radius),
                danger = Mathf.Clamp01(danger),
                time = Time.time,
                type = type
            };
            writeIndex = (writeIndex + 1) % Capacity;
            count = Mathf.Min(count + 1, Capacity);
        }

        public static bool TryGetBest(Vector3 listener, float maxAge, float maxRange, out AIStimulus result)
        {
            float bestScore = -1f;
            result = default;
            float maxRangeSqr = maxRange * maxRange;
            float now = Time.time;

            for (int i = 0; i < count; i++)
            {
                int idx = (writeIndex - 1 - i + Capacity) % Capacity;
                AIStimulus s = Buffer[idx];
                float age = now - s.time;
                if (age < 0f || age > maxAge) continue;
                Vector3 delta = s.position - listener;
                float distSqr = delta.sqrMagnitude;
                float hearingRadius = Mathf.Min(maxRange, s.radius);
                if (distSqr > hearingRadius * hearingRadius) continue;
                float score = s.danger * (1f - age / maxAge) * (1f - Mathf.Sqrt(distSqr) / Mathf.Max(1f, hearingRadius));
                if (score > bestScore)
                {
                    bestScore = score;
                    result = s;
                }
            }
            return bestScore >= 0f;
        }

        public static bool TryGetNearest(Vector3 listener, AIStimulusType type, float maxAge, float maxRange, out AIStimulus result)
        {
            float best = float.MaxValue;
            result = default;
            float now = Time.time;
            float maxRangeSqr = maxRange * maxRange;
            for (int i = 0; i < count; i++)
            {
                int idx = (writeIndex - 1 - i + Capacity) % Capacity;
                AIStimulus s = Buffer[idx];
                if (s.type != type || now - s.time > maxAge) continue;
                float d2 = (s.position - listener).sqrMagnitude;
                if (d2 <= maxRangeSqr && d2 < best)
                {
                    best = d2;
                    result = s;
                }
            }
            return best < float.MaxValue;
        }
    }
}
