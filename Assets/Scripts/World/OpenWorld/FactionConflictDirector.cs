using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class FactionConflictDirector : MonoBehaviour
    {
        [Serializable]
        private sealed class TerritoryState
        {
            public string name;
            public FactionId owner;
            public float stability = 0.72f;
            public float conflict;
        }

        public float tickEvery = 3f;
        public float conflictRate = 0.018f;
        private float nextTick;
        private readonly List<TerritoryState> states = new List<TerritoryState>(8);
        private readonly Dictionary<FactionId, int> wins = new Dictionary<FactionId, int>();
        private System.Random rng = new System.Random(8142);

        private void Start()
        {
            var director = FactionTerritoryDirector.Instance;
            if (!director || director.territories == null) return;
            for (int i = 0; i < director.territories.Length; i++)
            {
                var t = director.territories[i];
                if (t == null) continue;
                states.Add(new TerritoryState { name = t.name, owner = t.faction, stability = 0.62f + i * 0.04f });
            }
            foreach (FactionId id in Enum.GetValues(typeof(FactionId))) wins[id] = 0;
        }

        private void Update()
        {
            if (Time.time < nextTick || states.Count == 0) return;
            nextTick = Time.time + tickEvery;
            int wanted = WantedSystem.Instance ? WantedSystem.Instance.WantedLevel : 0;
            var factions = (FactionId[])Enum.GetValues(typeof(FactionId));
            for (int i = 0; i < states.Count; i++)
            {
                TerritoryState s = states[i];
                float volatility = conflictRate + wanted * 0.006f;
                s.conflict = Mathf.Clamp01(s.conflict + volatility * (float)(rng.NextDouble() * 1.4 - 0.3));
                s.stability = Mathf.Clamp01(s.stability + (0.5f - s.conflict) * 0.01f);
                if (s.conflict > 0.93f)
                {
                    FactionId next = factions[rng.Next(factions.Length)];
                    if (next != s.owner)
                    {
                        s.owner = next;
                        wins[next]++;
                        NeonApocalypse.UI.MobileHud.Instance?.ShowMessage($"FACTION SHIFT: {s.name}\n{next}", 3f);
                    }
                    s.conflict = 0.45f;
                }
            }
        }
    }
}
