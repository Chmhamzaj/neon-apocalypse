using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class VehicleEventFX : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private RaceFXController fx;
        private bool lastBoost;
        private bool lastBrake;
        private void Awake() { if (!car) car = GetComponentInParent<CarController>(); if (!fx) fx = GetComponentInChildren<RaceFXController>(); }
        private void Update()
        {
            if (!car || !fx) return;
            bool boost = car.IsBoosting;
            bool brake = Input.GetKey(KeyCode.Space);
            if (boost != lastBoost) { fx.SetNitro(boost); lastBoost = boost; }
            if (brake != lastBrake) { fx.SetBraking(brake); lastBrake = brake; }
            fx.SetDrift(Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.65f && car.SpeedKph > 55f);
        }
    }
}
