using UnityEngine;
using NeonApocalypse.World.OpenWorld;
using NeonApocalypse.Save;

namespace NeonApocalypse.Save
{
    public sealed class WorldProgressPersistence : MonoBehaviour
    {
        private void Start()
        {
            ApplyLoadedState();
            WorldEconomy.Instance?.CreditsChanged += OnChanged;
            FactionSystem.Instance?.ReputationChanged += OnFactionChanged;
        }

        private void OnDestroy()
        {
            if (WorldEconomy.Instance) WorldEconomy.Instance.CreditsChanged -= OnChanged;
            if (FactionSystem.Instance) FactionSystem.Instance.ReputationChanged -= OnFactionChanged;
        }

        private void OnChanged(int _) => Save();
        private void OnFactionChanged(FactionId _, int __) => Save();

        private void ApplyLoadedState()
        {
            SaveData data = SaveSystem.Load();
            if (data == null) return;
            WorldEconomy.Instance?.SetForLoad(Mathf.Max(0, data.credits));
            if (FactionSystem.Instance)
            {
                FactionSystem.Instance.SetForLoad(FactionId.HelixAuthority, data.helixRep);
                FactionSystem.Instance.SetForLoad(FactionId.ChromeSerpents, data.chromeRep);
                FactionSystem.Instance.SetForLoad(FactionId.NullCult, data.nullRep);
                FactionSystem.Instance.SetForLoad(FactionId.FreeRunners, data.freeRunnerRep);
            }
        }

        public void Save()
        {
            var gm = NeonApocalypse.Core.GameManager.Instance;
            SaveData current = SaveSystem.Load() ?? new SaveData();
            if (gm)
            {
                current.level = gm.PlayerLevel;
                current.xp = gm.PlayerXP;
                current.perkPoints = gm.PerkPoints;
            }
            current.credits = WorldEconomy.Instance ? WorldEconomy.Instance.Credits : current.credits;
            if (NeonApocalypse.Campaign.CampaignMissionDirector.Instance)
                current.campaignMissionIndex = NeonApocalypse.Campaign.CampaignMissionDirector.Instance.CurrentMissionIndex;
            if (FactionSystem.Instance)
            {
                current.helixRep = FactionSystem.Instance.Get(FactionId.HelixAuthority);
                current.chromeRep = FactionSystem.Instance.Get(FactionId.ChromeSerpents);
                current.nullRep = FactionSystem.Instance.Get(FactionId.NullCult);
                current.freeRunnerRep = FactionSystem.Instance.Get(FactionId.FreeRunners);
            }
            SaveSystem.Save(current);
        }

        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationQuit() => Save();
    }
}
