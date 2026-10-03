using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class NitroUpgradeSystem : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private SaveGame saveGame;
        [SerializeField] private int baseCost = 500;
        [SerializeField] private float capacityStep = 15f;
        [SerializeField] private int maxLevel = 5;
        public int Level { get; private set; }
        public int Cost => baseCost * (Level + 1);

        private void Awake()
        {
            if (!car) car = FindFirstObjectByType<CarController>();
            if (!saveGame) saveGame = FindFirstObjectByType<SaveGame>();
            Level = saveGame ? Mathf.Clamp(Mathf.RoundToInt((saveGame.Data.nitroUpgrade - 1f) / 0.15f), 0, maxLevel) : 0;
            if (car) car.ApplyNitroMultiplier(saveGame ? saveGame.Data.nitroUpgrade : 1f);
        }

        public bool PurchaseUpgrade()
        {
            if (!saveGame || Level >= maxLevel || saveGame.Data.credits < Cost) return false;
            saveGame.Data.credits -= Cost;
            Level++;
            saveGame.Data.nitroUpgrade = 1f + Level * 0.15f;
            saveGame.Commit();
            return true;
        }
    }
}