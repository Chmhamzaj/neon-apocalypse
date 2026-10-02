using System;
using System.Collections.Generic;
using UnityEngine;
using NeonApocalypse.Core;
using NeonApocalypse.Progression;
using NeonApocalypse.World.OpenWorld;

namespace NeonApocalypse.Campaign
{
    [Serializable]
    public sealed class StoryMissionDefinition
    {
        public string id;
        public string title;
        public string briefing;
        public string objectiveType;
        public string objectiveId;
        public int required = 1;
        public int creditsReward = 1000;
        public int xpReward = 750;
        public int factionRep = 2;
        public string faction = "FreeRunners";
        public Vector3 location;
    }

    [Serializable]
    public sealed class StoryMissionList
    {
        public List<StoryMissionDefinition> items = new List<StoryMissionDefinition>();
    }

    public sealed class CampaignMissionDirector : MonoBehaviour
    {
        public static CampaignMissionDirector Instance { get; private set; }
        public int CurrentMissionIndex { get; private set; }
        public StoryMissionDefinition CurrentMission { get; private set; }
        public bool CampaignComplete { get; private set; }

        private readonly List<StoryMissionDefinition> missions = new List<StoryMissionDefinition>(64);
        private MissionSystem missionSystem;
        private Transform player;
        private GameObject marker;
        private bool waitingForReach;
        private float nextSave;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            missionSystem = MissionSystem.Global ? MissionSystem.Global : gameObject.AddComponent<MissionSystem>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            missionSystem.MissionCompleted += OnMissionSystemCompleted;
            LoadDefinitions();
            LoadProgress();
            StartCurrentMission();
        }

        private void OnDestroy()
        {
            if (missionSystem) missionSystem.MissionCompleted -= OnMissionSystemCompleted;
            if (marker) Destroy(marker);
        }

        private void Update()
        {
            if (CampaignComplete || !CurrentMission || !player) return;
            if (waitingForReach && Vector3.SqrMagnitude(player.position - CurrentMission.location) <= 6.5f * 6.5f)
            {
                waitingForReach = false;
                missionSystem.Advance(ObjectiveType.Reach, CurrentMission.objectiveId, 1);
            }
            if (Time.time >= nextSave)
            {
                nextSave = Time.time + 10f;
                PersistProgress();
            }
        }

        public void StartCurrentMission()
        {
            if (missions.Count == 0) return;
            if (CurrentMissionIndex >= missions.Count)
            {
                CampaignComplete = true;
                MobileHudBridge("CAMPAIGN ARC COMPLETE • REGION 01 ONLINE", 5f);
                return;
            }

            CurrentMission = missions[Mathf.Clamp(CurrentMissionIndex, 0, missions.Count - 1)];
            waitingForReach = string.Equals(CurrentMission.objectiveType, "Reach", StringComparison.OrdinalIgnoreCase);
            missionSystem.activeObjectives = new[]
            {
                new Objective
                {
                    type = ParseObjective(CurrentMission.objectiveType),
                    id = CurrentMission.objectiveId,
                    required = Mathf.Max(1, CurrentMission.required),
                    progress = 0
                }
            };
            BuildMarker();
            MobileHudBridge($"{CurrentMission.title}\n{CurrentMission.briefing}", 6f);
            NeonApocalypse.UI.MobileHud.Instance?.SetMission(CurrentMission.title);
        }

        private void OnMissionSystemCompleted()
        {
            if (!CurrentMission) return;
            WorldEconomy.Instance?.Add(CurrentMission.creditsReward);
            GameManager.Instance?.AddXP(CurrentMission.xpReward);
            if (Enum.TryParse(CurrentMission.faction, true, out FactionId faction))
                FactionSystem.Instance?.AddReputation(faction, CurrentMission.factionRep);
            MobileHudBridge($"MISSION COMPLETE • +{CurrentMission.creditsReward} CR • +{CurrentMission.xpReward} XP", 5f);
            CurrentMissionIndex++;
            PersistProgress();
            if (marker) Destroy(marker);
            marker = null;
            if (CurrentMissionIndex >= missions.Count)
            {
                CampaignComplete = true;
                CurrentMission = null;
                MobileHudBridge("REGION 01 ARC COMPLETE • THE CITY REMEMBERS", 7f);
            }
            else
            {
                StartCurrentMission();
            }
        }

        private void BuildMarker()
        {
            if (marker) Destroy(marker);
            marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "MissionMarker_" + CurrentMission.id;
            marker.transform.position = CurrentMission.location;
            marker.transform.localScale = new Vector3(2.5f, 0.18f, 2.5f);
            var renderer = marker.GetComponent<Renderer>();
            if (renderer) renderer.sharedMaterial = BuildMarkerMaterial();
            var collider = marker.GetComponent<Collider>();
            if (collider) Destroy(collider);
        }

        private static Material BuildMarkerMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader);
            mat.color = new Color(0.1f, 0.8f, 1f);
            mat.enableInstancing = true;
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", new Color(0f, 1.5f, 2f));
            mat.EnableKeyword("_EMISSION");
            return mat;
        }

        private void LoadDefinitions()
        {
            var asset = Resources.Load<TextAsset>("Campaign/region01_missions");
            if (asset)
            {
                var parsed = JsonUtility.FromJson<StoryMissionList>(asset.text);
                if (parsed?.items != null) missions.AddRange(parsed.items);
            }
            if (missions.Count == 0) BuildFallbackDefinitions();
        }

        private void BuildFallbackDefinitions()
        {
            string[] titles =
            {
                "BLACKOUT", "MARKET OF GHOSTS", "THE FIRST HUNTER", "DEAD CHANNEL", "UNDERCITY DESCENT",
                "CHOIR FRAGMENT", "ASHEN HIGHWAY", "CHROME SERPENTS", "NULL CATHEDRAL", "FALLING SKY",
                "BLACK ICE", "MEMORY BREACH"
            };
            Vector3[] locations =
            {
                new Vector3(-24,0,-4), new Vector3(-8,0,6), new Vector3(10,0,15), new Vector3(0,0,28),
                new Vector3(28,0,20), new Vector3(-38,0,32), new Vector3(46,0,40), new Vector3(55,0,-12),
                new Vector3(-55,0,-28), new Vector3(12,0,-56), new Vector3(-22,0,-66), new Vector3(0,0,72)
            };
            for (int i = 0; i < titles.Length; i++)
            {
                missions.Add(new StoryMissionDefinition
                {
                    id = $"R01_{i + 1:00}",
                    title = titles[i],
                    briefing = i == 0 ? "Wake beneath the drowned transit line and find out who killed the city lights." : "Follow the signal. The city is changing around you.",
                    objectiveType = "Reach",
                    objectiveId = $"R01_REACH_{i + 1:00}",
                    required = 1,
                    location = locations[i],
                    creditsReward = 1000 + i * 175,
                    xpReward = 800 + i * 125,
                    factionRep = 2 + (i % 3),
                    faction = ((FactionId)(i % 4)).ToString()
                });
            }
        }

        private void LoadProgress()
        {
            var data = SaveSystem.Load();
            if (data != null) CurrentMissionIndex = Mathf.Clamp(data.campaignMissionIndex, 0, missions.Count);
            CampaignComplete = CurrentMissionIndex >= missions.Count && missions.Count > 0;
        }

        public void PersistProgress()
        {
            var data = SaveSystem.Load() ?? new SaveData();
            data.campaignMissionIndex = CurrentMissionIndex;
            SaveSystem.Save(data);
        }

        private static ObjectiveType ParseObjective(string value)
        {
            if (Enum.TryParse(value, true, out ObjectiveType result)) return result;
            return ObjectiveType.Reach;
        }

        private static void MobileHudBridge(string message, float seconds)
        {
            NeonApocalypse.UI.MobileHud.Instance?.ShowMessage(message, seconds);
        }
    }
}
