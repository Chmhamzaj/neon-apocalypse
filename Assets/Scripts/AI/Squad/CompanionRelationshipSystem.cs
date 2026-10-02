using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NeonApocalypse.AI.Squad
{
    [Serializable]
    public class CompanionRelationship
    {
        public string id;
        [Range(-100,100)] public int trust;
        [Range(-100,100)] public int loyalty;
        [Range(0,100)] public int bond;
        [Range(0,100)] public int morale = 70;
        public int missionsTogether;
    }

    [Serializable]
    public class CompanionRelationshipSave
    {
        public List<CompanionRelationship> companions = new List<CompanionRelationship>();
    }

    /// <summary>Small persistent relationship model. Uses rare event updates, never a per-frame planner.</summary>
    public sealed class CompanionRelationshipSystem : MonoBehaviour
    {
        public static CompanionRelationshipSystem Instance { get; private set; }
        private readonly Dictionary<string, CompanionRelationship> records = new Dictionary<string, CompanionRelationship>();
        private string SavePath => Path.Combine(Application.persistentDataPath, "neon_apocalypse_companions.json");

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public CompanionRelationship GetOrCreate(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) id = "anonymous_companion";
            if (!records.TryGetValue(id, out var record))
            {
                record = new CompanionRelationship { id = id, trust = 10, loyalty = 20, bond = 0, morale = 70 };
                records.Add(id, record);
            }
            return record;
        }

        public void RecordMission(string id, bool succeeded, bool companionSaved = true)
        {
            var r = GetOrCreate(id);
            r.missionsTogether++;
            r.trust = Mathf.Clamp(r.trust + (succeeded ? 4 : -2), -100, 100);
            r.loyalty = Mathf.Clamp(r.loyalty + (succeeded && companionSaved ? 5 : -6), -100, 100);
            r.bond = Mathf.Clamp(r.bond + (succeeded ? 7 : 1), 0, 100);
            r.morale = Mathf.Clamp(r.morale + (succeeded ? 4 : -8), 0, 100);
            Save();
        }

        public void RecordPlayerChoice(string id, int trustDelta, int loyaltyDelta, int bondDelta)
        {
            var r = GetOrCreate(id);
            r.trust = Mathf.Clamp(r.trust + trustDelta, -100, 100);
            r.loyalty = Mathf.Clamp(r.loyalty + loyaltyDelta, -100, 100);
            r.bond = Mathf.Clamp(r.bond + bondDelta, 0, 100);
            Save();
        }

        public string GetDisposition(string id)
        {
            var r = GetOrCreate(id);
            if (r.loyalty >= 70 && r.bond >= 60) return "Devoted";
            if (r.trust < -35 || r.loyalty < -20) return "Unstable";
            if (r.morale < 30) return "Shaken";
            return "Steady";
        }

        private void Save()
        {
            try
            {
                var data = new CompanionRelationshipSave();
                foreach (var kv in records) data.companions.Add(kv.Value);
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception e) { Debug.LogWarning($"Companion save failed: {e.Message}"); }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return;
                var data = JsonUtility.FromJson<CompanionRelationshipSave>(File.ReadAllText(SavePath));
                if (data?.companions == null) return;
                records.Clear();
                foreach (var r in data.companions) if (r != null && !string.IsNullOrEmpty(r.id)) records[r.id] = r;
            }
            catch (Exception e) { Debug.LogWarning($"Companion save load failed: {e.Message}"); }
        }

        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();
    }
}
