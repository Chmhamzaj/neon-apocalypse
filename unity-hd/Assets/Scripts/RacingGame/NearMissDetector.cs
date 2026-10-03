using System;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class NearMissDetector : MonoBehaviour
    {
        public static event Action NearMiss;
        [SerializeField] private CarController car;
        [SerializeField] private float nearDistance = 2.2f;
        [SerializeField] private float minimumSpeedKph = 75f;
        [SerializeField] private CameraShake cameraShake;
        public int Combo { get; private set; }

        private void Awake()
        {
            if (!car) car = GetComponent<CarController>();
            if (!cameraShake) cameraShake = FindFirstObjectByType<CameraShake>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!car || car.SpeedKph < minimumSpeedKph) return;
            if (!other.GetComponentInParent<TrafficVehicleAI>()) return;
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance <= nearDistance) { Combo++; cameraShake?.Impact(0.08f); NearMiss?.Invoke(); }
        }
    }
}