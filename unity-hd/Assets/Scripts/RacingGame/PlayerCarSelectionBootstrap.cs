using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class PlayerCarSelectionBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject[] carPrefabs;
        [SerializeField] private SaveGame saveGame;
        [SerializeField] private Transform spawnPoint;
        private GameObject activeCar;
        public GameObject ActiveCar => activeCar;

        private void Awake()
        {
            if (!saveGame) saveGame = FindFirstObjectByType<SaveGame>();
            SpawnSelected();
        }

        public void SpawnSelected()
        {
            if (carPrefabs == null || carPrefabs.Length == 0) return;
            int index = saveGame ? Mathf.Clamp(saveGame.Data.selectedCar, 0, carPrefabs.Length - 1) : 0;
            if (activeCar) Destroy(activeCar);
            Transform point = spawnPoint ? spawnPoint : transform;
            activeCar = Instantiate(carPrefabs[index], point.position, point.rotation);
            var bootstrap = activeCar.GetComponent<PlayerVehicleBootstrap>();
            if (!bootstrap) bootstrap = activeCar.AddComponent<PlayerVehicleBootstrap>();
        }
    }
}