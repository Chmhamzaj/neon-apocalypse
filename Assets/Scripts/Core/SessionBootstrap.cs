using UnityEngine;
using NeonApocalypse.UI;
using NeonApocalypse.Performance;
using NeonApocalypse.Save;
using NeonApocalypse.Campaign;
using NeonApocalypse.World.OpenWorld;
using NeonApocalypse.Progression;

namespace NeonApocalypse.Core
{
    public class SessionBootstrap : MonoBehaviour
    {
        [SerializeField] private bool lockCursorOnStart = true;

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            if (!FindFirstObjectByType<MobileStabilityDirector>()) new GameObject("MobileStabilityDirector").AddComponent<MobileStabilityDirector>();
            if (!FindFirstObjectByType<MobileQualityTierController>()) new GameObject("MobileQualityTierController").AddComponent<MobileQualityTierController>();
            if (!FindFirstObjectByType<PerformanceDirector>()) new GameObject("PerformanceDirector").AddComponent<PerformanceDirector>();
            if (!FindFirstObjectByType<PerformanceDirectorV2>()) new GameObject("PerformanceDirectorV2").AddComponent<PerformanceDirectorV2>();
            if (!FindFirstObjectByType<SpawnBudget>()) new GameObject("SpawnBudget").AddComponent<SpawnBudget>();
            if (!FindFirstObjectByType<PerformanceOverlay>()) new GameObject("PerformanceOverlay").AddComponent<PerformanceOverlay>();
            if (!FindFirstObjectByType<WorldProgressPersistence>()) new GameObject("WorldProgressPersistence").AddComponent<WorldProgressPersistence>();
            EnsureSystem<CampaignMissionDirector>("CampaignMissionDirector");
            EnsureSystem<FactionTerritoryDirector>("FactionTerritoryDirector");
            EnsureSystem<DynamicEncounterDirector>("DynamicEncounterDirector");
            EnsureSystem<NeonApocalypse.AI.NPCRumorNetwork>("NPCRumorNetwork");
            EnsureSystem<NeonApocalypse.AI.FactionAIDirector>("FactionAIDirector");
            EnsureSystem<InteriorPoolService>("InteriorPoolService");
            EnsureSystem<WorldInteractionBudget>("WorldInteractionBudget");
            if (lockCursorOnStart) Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = !lockCursorOnStart;
            MobileHud.Instance?.BindToPlayer();
        }

        private static void EnsureSystem<T>(string objectName) where T : Component
        {
            if (!FindFirstObjectByType<T>()) new GameObject(objectName).AddComponent<T>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool unlock = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = unlock ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = unlock;
            }
        }
    }
}
