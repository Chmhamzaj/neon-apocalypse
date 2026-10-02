using System;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class WantedSystem : MonoBehaviour
    {
        public static WantedSystem Instance { get; private set; }
        [Range(0f, 5f)] public float heat;
        public float decayPerSecond = 0.055f;
        public event Action<int> WantedLevelChanged;
        private int lastLevel;

        public int WantedLevel => Mathf.Clamp(Mathf.CeilToInt(heat), 0, 5);

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (heat > 0f) heat = Mathf.Max(0f, heat - decayPerSecond * Time.deltaTime);
            int level = WantedLevel;
            if (level != lastLevel)
            {
                lastLevel = level;
                WantedLevelChanged?.Invoke(level);
            }
        }

        public void AddHeat(float amount)
        {
            if (amount <= 0f) return;
            int before = WantedLevel;
            heat = Mathf.Clamp(heat + amount, 0f, 5f);
            int after = WantedLevel;
            if (after != before) WantedLevelChanged?.Invoke(after);
            lastLevel = after;
        }

        public void ClearWanted() { heat = 0f; lastLevel = 0; WantedLevelChanged?.Invoke(0); }
    }
}
