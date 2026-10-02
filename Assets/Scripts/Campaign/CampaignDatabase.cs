using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.Campaign
{
    [System.Serializable] public class CampaignRegion { public string id, name, description; public int main_mission_count, side_mission_count, bosses; }
    [System.Serializable] public class CampaignRegionList { public List<CampaignRegion> items; }

    public class CampaignDatabase : MonoBehaviour
    {
        public static CampaignDatabase Instance { get; private set; }
        public List<CampaignRegion> Regions { get; private set; } = new();

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadRegions();
        }

        private void LoadRegions()
        {
            var asset = Resources.Load<TextAsset>("Campaign/regions");
            if (!asset) return;
            var json = "{\"items\":" + asset.text + "}";
            var data = JsonUtility.FromJson<CampaignRegionList>(json);
            if (data?.items != null) Regions = data.items;
        }
    }
}
