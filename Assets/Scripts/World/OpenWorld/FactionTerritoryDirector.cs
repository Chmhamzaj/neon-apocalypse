using System;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class FactionTerritoryDirector : MonoBehaviour
    {
        [Serializable]
        public sealed class Territory
        {
            public string name;
            public FactionId faction;
            public Vector3 center;
            public float radius = 220f;
            public float heatMultiplier = 1f;
        }

        public static FactionTerritoryDirector Instance { get; private set; }
        public Territory[] territories;
        public Territory CurrentTerritory { get; private set; }
        public event Action<Territory> TerritoryChanged;
        private Transform player;
        private float nextCheck;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildDefaultTerritories();
        }

        private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

        private void Update()
        {
            if (!player || Time.time < nextCheck) return;
            nextCheck = Time.time + 0.5f;
            Territory found = null;
            float best = float.MaxValue;
            foreach (var territory in territories)
            {
                if (territory == null) continue;
                float d = Vector3.SqrMagnitude(player.position - territory.center);
                float r2 = territory.radius * territory.radius;
                if (d <= r2 && d < best) { best = d; found = territory; }
            }
            if (found == CurrentTerritory) return;
            CurrentTerritory = found;
            TerritoryChanged?.Invoke(CurrentTerritory);
            if (CurrentTerritory != null)
                NeonApocalypse.UI.MobileHud.Instance?.ShowMessage($"TERRITORY: {CurrentTerritory.name}\n{CurrentTerritory.faction}", 3f);
        }

        public float GetCurrentHeatMultiplier() => CurrentTerritory?.heatMultiplier ?? 1f;

        private void BuildDefaultTerritories()
        {
            territories = new[]
            {
                new Territory { name="HELIX CORE", faction=FactionId.HelixAuthority, center=new Vector3(0,0,180), radius=260f, heatMultiplier=1.35f },
                new Territory { name="CHROME DOCKS", faction=FactionId.ChromeSerpents, center=new Vector3(320,0,-160), radius=300f, heatMultiplier=1.20f },
                new Territory { name="NULL WASTES", faction=FactionId.NullCult, center=new Vector3(-360,0,-220), radius=330f, heatMultiplier=1.05f },
                new Territory { name="FREERUNNER GRID", faction=FactionId.FreeRunners, center=new Vector3(220,0,300), radius=250f, heatMultiplier=0.70f },
                new Territory { name="NEUTRAL INDUSTRIAL", faction=FactionId.FreeRunners, center=new Vector3(-140,0,120), radius=180f, heatMultiplier=0.90f }
            };
        }
    }
}
