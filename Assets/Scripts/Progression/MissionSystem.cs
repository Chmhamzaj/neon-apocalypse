using System;
using UnityEngine;

namespace NeonApocalypse.Progression
{
    public enum ObjectiveType { Kill, Reach, Collect }

    [Serializable]
    public class Objective
    {
        public ObjectiveType type;
        public string id;
        public int required = 1;
        public int progress;
        public bool Complete => progress >= required;
    }

    public class MissionSystem : MonoBehaviour
    {
        public static MissionSystem Global { get; private set; }
        public Objective[] activeObjectives;
        public string missionTitle = "THE FALLEN CITY";
        public event Action MissionCompleted;
        private bool completionRaised;

        public void SetObjectives(string title, Objective[] objectives)
        {
            missionTitle = title;
            activeObjectives = objectives ?? Array.Empty<Objective>();
            completionRaised = false;
        }

        public void ResetObjectives()
        {
            activeObjectives = Array.Empty<Objective>();
            completionRaised = false;
        }

        private void Awake()
        {
            if (Global != null && Global != this) { Destroy(gameObject); return; }
            Global = this;
        }

        public void Advance(ObjectiveType type, string id, int amount = 1)
        {
            if (activeObjectives == null) return;
            bool changed = false;
            foreach (var o in activeObjectives)
            {
                if (o.type == type && o.id == id && !o.Complete)
                {
                    o.progress = Mathf.Min(o.required, o.progress + amount);
                    changed = true;
                }
            }
            if (changed) CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (activeObjectives.Length == 0) return;
            bool complete = true;
            foreach (var o in activeObjectives) complete &= o.Complete;
            if (complete && !completionRaised)
            {
                completionRaised = true;
                MissionCompleted?.Invoke();
                GameManagerBridge.RewardMission();
            }
        }

        private static class GameManagerBridge
        {
            public static void RewardMission()
            {
                var gm = NeonApocalypse.Core.GameManager.Instance;
                gm?.AddXP(500);
                NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("MISSION COMPLETE +500 XP", 3f);
            }
        }
    }
}
