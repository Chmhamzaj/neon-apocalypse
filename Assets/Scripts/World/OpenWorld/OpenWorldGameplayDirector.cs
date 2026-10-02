using UnityEngine;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class OpenWorldGameplayDirector : MonoBehaviour
    {
        public Transform player;

        [ContextMenu("Apply Open World Defaults")]
        public void ApplyDefaults()
        {
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            Ensure<TrafficAndPedestrianDirector>("TrafficAndPedestrianDirector");
            Ensure<WantedSystem>("WantedSystem");
            Ensure<PoliceDirector>("PoliceDirector");
            Ensure<WorldEconomy>("WorldEconomy");
            Ensure<FactionSystem>("FactionSystem");
            Ensure<WorldShopSystem>("WorldShopSystem");
            Ensure<FactionTerritoryDirector>("FactionTerritoryDirector");
            Ensure<DynamicEncounterDirector>("DynamicEncounterDirector");
            var activities = Ensure<WorldActivityDirector>("WorldActivityDirector");
            activities.player = player;
            var contracts = Ensure<CampaignContractDirector>("CampaignContractDirector");
            contracts.player = player;
            BuildInteriorPortals();
            BuildFacilityTerminals();
            BuildShops();
        }

        private void Start() => ApplyDefaults();

        private T Ensure<T>(string name) where T : Component
        {
            var existing = GetComponentInChildren<T>();
            if (existing) return existing;
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var component = go.AddComponent<T>();
            if (component is TrafficAndPedestrianDirector traffic) { traffic.trafficCount = 28; traffic.pedestrianCount = 36; }
            if (component is PoliceDirector police) { police.maxPolice = 6; }
            return component;
        }

        private void BuildFacilityTerminals()
        {
            if (transform.Find("Facility_Safehouse") || !player) return;
            CreateFacility("Facility_Safehouse", FacilityType.Safehouse, new Vector3(-60f, 1.2f, 28f));
            CreateFacility("Facility_BlackMarket", FacilityType.BlackMarket, new Vector3(58f, 1.2f, -8f));
            CreateFacility("Facility_Garage", FacilityType.Garage, new Vector3(18f, 1.2f, 62f));
        }

        private void CreateFacility(string name, FacilityType type, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(2.2f, 1.0f, 2.2f);
            var collider = go.GetComponent<Collider>();
            if (collider) collider.isTrigger = true;
            var terminal = go.AddComponent<FacilityTerminal>();
            terminal.facilityType = type;
        }

        private void BuildShops()
        {
            if (!player || transform.Find("WorldShops")) return;
            var root = new GameObject("WorldShops");
            root.transform.SetParent(transform, false);
            CreateShop(root.transform, "Armory", WorldShopSystem.ShopType.Armory, new Vector3(-18f, 0.9f, 46f));
            CreateShop(root.transform, "MedBay", WorldShopSystem.ShopType.MedBay, new Vector3(-60f, 0.9f, 36f));
            CreateShop(root.transform, "BlackMarketShop", WorldShopSystem.ShopType.BlackMarket, new Vector3(64f, 0.9f, -12f));
            CreateShop(root.transform, "GarageService", WorldShopSystem.ShopType.Garage, new Vector3(18f, 0.9f, 62f));
        }

        private static void CreateShop(Transform parent, string name, WorldShopSystem.ShopType type, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Shop_" + name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(2.8f, 1.8f, 2.8f);
            var collider = go.GetComponent<Collider>();
            if (collider) collider.isTrigger = true;
            var shop = go.AddComponent<WorldShopSystem>();
            shop.shopType = type;
        }

        private void BuildInteriorPortals()
        {
            if (!player || transform.Find("InteriorPortal_A") || transform.Find("InteriorPortal_B")) return;
            Vector3[] positions = { new Vector3(-45f, 0f, 12f), new Vector3(42f, 0f, 35f), new Vector3(75f, 0f, -18f) };
            string[] names = { "SAFEHOUSE", "BLACK_MARKET", "NIGHTCLUB" };
            for (int i = 0; i < positions.Length; i++)
            {
                var portal = new GameObject("InteriorPortal_" + (char)('A' + i));
                portal.transform.SetParent(transform, false);
                portal.transform.position = positions[i];
                var col = portal.AddComponent<BoxCollider>();
                col.isTrigger = true;
                col.size = new Vector3(3f, 3f, 2f);
                var script = portal.AddComponent<InteriorPortal>();
                script.interiorName = names[i];
                script.interiorOffset = new Vector3(0f, 0f, 500f + i * 80f);
            }
        }
    }
}
