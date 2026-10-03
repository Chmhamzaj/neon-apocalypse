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
            if (!raceCamera) raceCamera = FindFirstObjectByType<RaceCamera>();
            if (!raceProgress) raceProgress = FindFirstObjectByType<RaceProgress>();
            if (!touchInput) touchInput = FindFirstObjectByType<TouchInput>();

            if (raceCamera && car) raceCamera.SetTarget(car.transform);
            if (raceProgress && car) raceProgress.SetPlayer(car.transform);
            if (touchInput && car) touchInput.SetTarget(car);
        }
    }
}
