using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceWorldBootstrap : MonoBehaviour
    {
        [SerializeField] private CityRoadBuilder worldBuilder;
        [SerializeField] private PlayerVehicleBootstrap playerBootstrap;
        [SerializeField] private RaceProgress progress;
        [SerializeField] private RaceMarkers markers;
        [SerializeField] private RaceSplinePath racePath;
        [SerializeField] private RaceStateEvents raceEvents;
        [SerializeField] private Transform finish;
        [SerializeField] private float raceDistance = 2000f;

        private void Start()
        {
            worldBuilder?.Build();
            if (!playerBootstrap) playerBootstrap = FindFirstObjectByType<PlayerVehicleBootstrap>();
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
            if (!markers) markers = FindFirstObjectByType<RaceMarkers>();
            if (!racePath) racePath = FindFirstObjectByType<RaceSplinePath>();
            if (!raceEvents) raceEvents = FindFirstObjectByType<RaceStateEvents>();
            var car = playerBootstrap ? playerBootstrap.Car : FindFirstObjectByType<CarController>();
            if (!progress || !car) return;
            if (racePath) { progress.SetPath(racePath); raceDistance = racePath.TotalLength; }
            if (finish) finish.position = new Vector3(finish.position.x, finish.position.y, car.transform.position.z + raceDistance);
            progress.SetPlayer(car.transform);
            progress.SetFinish(finish);
            progress.SetRaceDistance(raceDistance);
            markers?.Configure(car.transform, finish, raceDistance);
            // RaceFlowController owns the actual start event after countdown.
        }
    }
}
