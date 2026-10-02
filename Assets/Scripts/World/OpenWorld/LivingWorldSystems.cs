using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public enum FactionId { HelixAuthority, ChromeSerpents, NullCult, FreeRunners }

    public sealed class WorldEconomy : MonoBehaviour
    {
        public static WorldEconomy Instance { get; private set; }
        [SerializeField] private int credits = 5000;
        public int Credits => credits;
        public event Action<int> CreditsChanged;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetForLoad(int value)
        {
            credits = Mathf.Max(0, value);
            CreditsChanged?.Invoke(credits);
        }

        public void Add(int amount)
        {
            if (amount == 0) return;
            credits = Mathf.Max(0, credits + amount);
            CreditsChanged?.Invoke(credits);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || credits < amount) return false;
            credits -= amount;
            CreditsChanged?.Invoke(credits);
            return true;
        }
    }

    public sealed class FactionSystem : MonoBehaviour
    {
        public static FactionSystem Instance { get; private set; }
        private readonly Dictionary<FactionId, int> reputation = new Dictionary<FactionId, int>();
        public event Action<FactionId, int> ReputationChanged;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            foreach (FactionId faction in Enum.GetValues(typeof(FactionId))) reputation[faction] = 0;
            DontDestroyOnLoad(gameObject);
        }

        public int Get(FactionId faction) => reputation.TryGetValue(faction, out int value) ? value : 0;

        public void SetForLoad(FactionId faction, int value)
        {
            reputation[faction] = Mathf.Clamp(value, -100, 100);
            ReputationChanged?.Invoke(faction, reputation[faction]);
        }

        public void AddReputation(FactionId faction, int amount)
        {
            if (amount == 0) return;
            int next = Mathf.Clamp(Get(faction) + amount, -100, 100);
            reputation[faction] = next;
            ReputationChanged?.Invoke(faction, next);
        }
    }

    public sealed class WorldActivityDirector : MonoBehaviour
    {
        private enum ActivityType { Convoy, Extraction, Ambush }
        private sealed class Activity
        {
            public GameObject root;
            public ActivityType type;
            public float endAt;
            public FactionId faction;
        }

        public Transform player;
        public int maxActiveActivities = 3;
        public float startEverySeconds = 35f;
        public float activityRadius = 85f;
        private float nextActivity;
        private readonly List<Activity> active = new List<Activity>(4);
        private static readonly Vector3[] Offsets =
        {
            new Vector3(70f, 0f, 20f), new Vector3(-65f, 0f, 40f), new Vector3(25f, 0f, -80f), new Vector3(-45f, 0f, -70f)
        };

        private void Start()
        {
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            nextActivity = Time.time + 8f;
        }

        private void Update()
        {
            if (!player) return;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                Activity a = active[i];
                if (!a.root || Time.time >= a.endAt || Vector3.SqrMagnitude(a.root.transform.position - player.position) > 150f * 150f)
                {
                    if (a.root) Destroy(a.root);
                    active.RemoveAt(i);
                }
            }
            if (Time.time < nextActivity || active.Count >= Mathf.Clamp(maxActiveActivities, 1, 6)) return;
            nextActivity = Time.time + Mathf.Max(15f, startEverySeconds);
            SpawnActivity();
        }

        private void SpawnActivity()
        {
            int index = UnityEngine.Random.Range(0, Offsets.Length);
            ActivityType type = (ActivityType)UnityEngine.Random.Range(0, 3);
            FactionId faction = (FactionId)UnityEngine.Random.Range(0, 4);
            Vector3 pos = player.position + Offsets[index];
            var root = new GameObject($"WorldActivity_{type}_{active.Count}");
            root.transform.position = pos;
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "ActivityMarker";
            marker.transform.SetParent(root.transform, false);
            marker.transform.localScale = new Vector3(2.2f, 0.15f, 2.2f);
            marker.GetComponent<Renderer>().sharedMaterial = null;
            Destroy(marker.GetComponent<Collider>());
            if (type == ActivityType.Convoy)
            {
                CreateActivityVehicle(root.transform, Vector3.zero, 0f);
                CreateActivityVehicle(root.transform, new Vector3(7f, 0f, 10f), 12f);
                WorldEconomy.Instance?.Add(0);
            }
            else
            {
                for (int i = 0; i < 3; i++) CreateActivityAgent(root.transform, new Vector3((i - 1) * 5f, 1f, i * 4f));
            }
            active.Add(new Activity { root = root, type = type, endAt = Time.time + 55f, faction = faction });
            NeonApocalypse.UI.MobileHud.Instance?.ShowMessage(type == ActivityType.Convoy ? "WORLD EVENT: ARMORED CONVOY" : type == ActivityType.Extraction ? "WORLD EVENT: EXTRACTION" : "WORLD EVENT: GANG AMBUSH", 4f);
        }

        private static void CreateActivityVehicle(Transform parent, Vector3 local, float yaw)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "EventVehicle";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = new Vector3(1.9f, 1.1f, 4.2f);
            var traffic = go.AddComponent<NeonApocalypse.Vehicles.TrafficVehicle>();
            traffic.Initialize(GameObject.FindGameObjectWithTag("Player")?.transform, Mathf.Abs(go.GetInstanceID()));
        }

        private static void CreateActivityAgent(Transform parent, Vector3 local)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "EventAgent";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = local;
            var col = go.GetComponent<CapsuleCollider>();
            if (col) col.isTrigger = true;
            var agent = go.AddComponent<NeonApocalypse.AI.Civilians.CivilianAgent>();
            agent.Initialize(GameObject.FindGameObjectWithTag("Player")?.transform, Mathf.Abs(go.GetInstanceID()));
        }
    }

    public sealed class CampaignContractDirector : MonoBehaviour
    {
        private sealed class Contract
        {
            public string title;
            public FactionId faction;
            public int reward;
            public int repReward;
            public Vector3 location;
            public float expires;
        }

        public Transform player;
        public float contractEverySeconds = 42f;
        public float contractRadius = 95f;
        private float nextContract;
        private Contract current;
        private GameObject marker;
        private static readonly string[] Titles =
        {
            "BLACK ICE RUN", "GHOST TRAIN", "BROKEN CROWN", "LAST WITNESS", "REDLINE EXTRACTION", "DEAD DROP"
        };

        private void Start()
        {
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            nextContract = Time.time + 18f;
        }

        private void Update()
        {
            if (!player) return;
            if (current != null)
            {
                if (Time.time > current.expires)
                {
                    ClearContract("CONTRACT EXPIRED");
                }
                else if (Vector3.SqrMagnitude(player.position - current.location) <= contractRadius * contractRadius)
                {
                    CompleteCurrent();
                }
                return;
            }
            if (Time.time >= nextContract)
            {
                nextContract = Time.time + contractEverySeconds;
                OfferContract();
            }
        }

        private void OfferContract()
        {
            int index = Random.Range(0, 6);
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(55f, 125f);
            current = new Contract
            {
                title = Titles[index],
                faction = (FactionId)Random.Range(0, 4),
                reward = 900 + Random.Range(0, 7) * 150,
                repReward = 5 + Random.Range(0, 4),
                location = player.position + new Vector3(circle.x, 0f, circle.y),
                expires = Time.time + 70f
            };
            marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "ContractMarker_" + current.title.Replace(" ", "_");
            marker.transform.position = current.location;
            marker.transform.localScale = new Vector3(2.4f, 0.2f, 2.4f);
            Destroy(marker.GetComponent<Collider>());
            NeonApocalypse.UI.MobileHud.Instance?.ShowMessage($"CONTRACT: {current.title} • REWARD {current.reward} CR", 5f);
        }

        private void CompleteCurrent()
        {
            if (current == null) return;
            int reward = current.reward;
            WorldEconomy.Instance?.Add(reward);
            FactionSystem.Instance?.AddReputation(current.faction, current.repReward);
            NeonApocalypse.Core.GameManager.Instance?.AddXP(750);
            NeonApocalypse.World.OpenWorld.WantedSystem.Instance?.AddHeat(0.35f);
            NeonApocalypse.UI.MobileHud.Instance?.ShowMessage($"CONTRACT COMPLETE +{reward} CR +750 XP", 4f);
            ClearContract(null);
        }

        private void ClearContract(string message)
        {
            if (marker) Destroy(marker);
            marker = null;
            current = null;
            if (!string.IsNullOrEmpty(message)) NeonApocalypse.UI.MobileHud.Instance?.ShowMessage(message, 3f);
        }
    }

    public enum FacilityType { Safehouse, BlackMarket, Garage }

    public sealed class FacilityTerminal : MonoBehaviour
    {
        public FacilityType facilityType;
        public float radius = 4f;
        private Transform player;
        private float nextUse;

        private void Start() => player = GameObject.FindGameObjectWithTag("Player")?.transform;

        private void Update()
        {
            if (!player || Time.time < nextUse) return;
            if (Vector3.SqrMagnitude(player.position - transform.position) <= radius * radius && Input.GetKeyDown(KeyCode.F))
            {
                nextUse = Time.time + 0.75f;
                UseFacility();
            }
        }

        private void UseFacility()
        {
            switch (facilityType)
            {
                case FacilityType.Garage:
                    WorldEconomy.Instance?.TrySpend(250);
                    if (VehicleEnterExit.ActiveVehicle)
                    {
                        var health = VehicleEnterExit.ActiveVehicle.GetComponent<NeonApocalypse.Combat.Health>();
                        if (health) health.Heal(99999f);
                    }
                    WantedSystem.Instance?.ClearWanted();
                    NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("GARAGE: VEHICLE REPAIRED • 250 CR", 3f);
                    break;
                case FacilityType.BlackMarket:
                    if (WorldEconomy.Instance && WorldEconomy.Instance.TrySpend(750))
                        FactionSystem.Instance?.AddReputation(FactionId.FreeRunners, 4);
                    NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("BLACK MARKET: CONTRABAND ACQUIRED • 750 CR", 3f);
                    break;
                default:
                    WantedSystem.Instance?.ClearWanted();
                    NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("SAFEHOUSE: HEAT CLEARED", 3f);
                    break;
            }
        }
    }
}
