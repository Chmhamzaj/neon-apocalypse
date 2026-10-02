using System;
using UnityEngine;
using NeonApocalypse.Save;

namespace NeonApocalypse.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public int PlayerLevel { get; private set; } = 1;
        public int PlayerXP { get; private set; }
        public int PerkPoints { get; private set; }
        public event Action<int> LevelChanged;
        public event Action<int, int> XPChanged;
        public Transform PlayerTransform { get; private set; }

        public void RegisterPlayer(Transform player) => PlayerTransform = player;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance == null)
                new GameObject("GameManager").AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }

        public int XPForNextLevel => 100 + (PlayerLevel - 1) * 85 + PlayerLevel * 15;

        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            PlayerXP += amount;
            while (PlayerXP >= XPForNextLevel)
            {
                PlayerXP -= XPForNextLevel;
                PlayerLevel++;
                PerkPoints++;
                LevelChanged?.Invoke(PlayerLevel);
            }
            XPChanged?.Invoke(PlayerXP, XPForNextLevel);
            SaveProgress();
        }

        public bool SpendPerkPoint()
        {
            if (PerkPoints <= 0) return false;
            PerkPoints--;
            SaveProgress();
            return true;
        }

        public void SaveProgress()
        {
            SaveSystem.Save(new SaveData { level = PlayerLevel, xp = PlayerXP, perkPoints = PerkPoints });
        }

        public void LoadProgress()
        {
            var data = SaveSystem.Load();
            if (data == null) return;
            PlayerLevel = Mathf.Max(1, data.level);
            PlayerXP = Mathf.Max(0, data.xp);
            PerkPoints = Mathf.Max(0, data.perkPoints);
        }
    }
}
