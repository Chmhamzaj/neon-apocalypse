using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class SpeedLineFXController : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private ParticleSystem speedLines;
        [SerializeField] private float activationSpeedKph = 180f;
        private void Awake() { if (!car) car = FindFirstObjectByType<CarController>(); }
        private void Update()
        {
            if (!car || !speedLines) return;
            bool active = car.SpeedKph >= activationSpeedKph || car.IsBoosting;
            if (active && !speedLines.isPlaying) speedLines.Play();
            else if (!active && speedLines.isPlaying) speedLines.Stop();
        }
    }
}