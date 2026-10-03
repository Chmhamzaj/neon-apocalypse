using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class VehicleEventFX : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private RaceFXController fx;
        private DrivingTrickSystem tricks;
        private bool lastBoost;
        private bool lastBrake;

        private void Awake()
        {
            if (!car) car = GetComponentInParent<CarController>();
            if (!fx) fx = GetComponentInChildren<RaceFXController>();
            tricks = GetComponentInParent<DrivingTrickSystem>();
        }

        private void Update()
        {
            if (!car || !fx) return;
            bool boost = car.IsBoosting;
            bool brake = car.IsBraking;
            bool drift = tricks && tricks.Drifting;

            if (boost != lastBoost) { fx.SetNitro(boost); lastBoost = boost; }
            if (brake != lastBrake) { fx.SetBraking(brake); lastBrake = brake; }
            fx.SetDrift(drift);
        }
    }
}
