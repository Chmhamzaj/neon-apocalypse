using UnityEngine;

namespace NitroStreetRush.Racing
{
    public class PlayerVehicleBootstrap : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private RaceCamera raceCamera;
        [SerializeField] private RaceProgress raceProgress;
        [SerializeField] private TouchInput touchInput;

        public CarController Car => car;

        private void Awake()
        {
            if (!car) car = GetComponentInChildren<CarController>();
            if (!car) return;
            if (!raceCamera) raceCamera = FindFirstObjectByType<RaceCamera>();
            if (!raceProgress) raceProgress = FindFirstObjectByType<RaceProgress>();
            if (!touchInput) touchInput = FindFirstObjectByType<TouchInput>();

            EnsureComponent<VehicleStabilityAssist>();
            EnsureComponent<CollisionRecovery>();
            EnsureComponent<VehicleDamageFeedback>();
            EnsureComponent<DrivingTrickSystem>();
            EnsureComponent<VehicleEventFX>();

            if (raceCamera) raceCamera.SetTarget(car.transform);
            if (raceProgress) raceProgress.SetPlayer(car.transform);
            if (touchInput) touchInput.SetTarget(car);
        }

        private void EnsureComponent<T>() where T : Component
        {
            if (!car.GetComponent<T>()) car.gameObject.AddComponent<T>();
        }
    }
}
