using UnityEngine;

namespace NeonApocalypse.Vehicles
{
    public sealed class TrafficScheduleDirector : MonoBehaviour
    {
        public float updateEvery = 8f;
        public float rushMultiplier = 1.16f;
        public float curfewMultiplier = 0.62f;
        private float nextUpdate;
        private TrafficVehicle[] vehicles = System.Array.Empty<TrafficVehicle>();

        private void Update()
        {
            if (Time.time < nextUpdate) return;
            nextUpdate = Time.time + updateEvery;
            vehicles = FindObjectsOfType<TrafficVehicle>(true);
            // The simulation clock is intentionally independent of wall-clock time.
            float hour = (Time.time / 60f) % 24f;
            float multiplier = (hour >= 7f && hour < 9.5f) || (hour >= 16.5f && hour < 19.5f) ? rushMultiplier :
                               (hour >= 0f && hour < 6f) ? curfewMultiplier : 1f;
            for (int i = 0; i < vehicles.Length; i++)
            {
                TrafficVehicle v = vehicles[i];
                if (v) v.SetScheduleMultiplier(multiplier);
            }
        }
    }
}
