using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonApocalypse.Vehicles
{
    public enum VehicleClass { Compact, Muscle, Super, SUV, Truck, Motorcycle, Police, Van, OffRoad, Boat, Helicopter, Armored }

    [Serializable]
    public struct VehicleSpec
    {
        public string id;
        public VehicleClass vehicleClass;
        public float maxSpeedKph;
        public float acceleration;
        public float steering;
        public float armor;
        public float grip;
    }

    [CreateAssetMenu(menuName = "Neon Apocalypse/Vehicle Catalog")]
    public sealed class VehicleArchetypeCatalog : ScriptableObject
    {
        public List<VehicleSpec> vehicles = new List<VehicleSpec>
        {
            new VehicleSpec { id="neon_compact", vehicleClass=VehicleClass.Compact, maxSpeedKph=175, acceleration=0.9f, steering=1.1f, armor=1f, grip=7.5f },
            new VehicleSpec { id="neon_muscle", vehicleClass=VehicleClass.Muscle, maxSpeedKph=220, acceleration=1.2f, steering=0.9f, armor=1.4f, grip=6.8f },
            new VehicleSpec { id="neon_super", vehicleClass=VehicleClass.Super, maxSpeedKph=310, acceleration=1.5f, steering=1.25f, armor=0.8f, grip=9.2f },
            new VehicleSpec { id="neon_suv", vehicleClass=VehicleClass.SUV, maxSpeedKph=195, acceleration=1.0f, steering=0.85f, armor=1.8f, grip=7.2f },
            new VehicleSpec { id="neon_truck", vehicleClass=VehicleClass.Truck, maxSpeedKph=145, acceleration=0.7f, steering=0.65f, armor=2.3f, grip=5.7f },
            new VehicleSpec { id="neon_bike", vehicleClass=VehicleClass.Motorcycle, maxSpeedKph=265, acceleration=1.7f, steering=1.5f, armor=0.35f, grip=8.8f },
            new VehicleSpec { id="neon_police", vehicleClass=VehicleClass.Police, maxSpeedKph=235, acceleration=1.35f, steering=1.2f, armor=1.7f, grip=8.5f },
            new VehicleSpec { id="neon_van", vehicleClass=VehicleClass.Van, maxSpeedKph=155, acceleration=0.82f, steering=0.72f, armor=1.5f, grip=6.2f },
            new VehicleSpec { id="neon_offroad", vehicleClass=VehicleClass.OffRoad, maxSpeedKph=185, acceleration=1.05f, steering=0.92f, armor=1.9f, grip=8.9f },
            new VehicleSpec { id="neon_armored", vehicleClass=VehicleClass.Armored, maxSpeedKph=135, acceleration=0.6f, steering=0.6f, armor=4.5f, grip=5.0f },
            new VehicleSpec { id="neon_boat", vehicleClass=VehicleClass.Boat, maxSpeedKph=120, acceleration=1.0f, steering=0.8f, armor=1.2f, grip=3.0f },
            new VehicleSpec { id="neon_helicopter", vehicleClass=VehicleClass.Helicopter, maxSpeedKph=280, acceleration=1.1f, steering=0.9f, armor=1.4f, grip=2.0f }
        };
    }

    [DisallowMultipleComponent]
    public sealed class VehicleSpecBinder : MonoBehaviour
    {
        public VehicleSpec spec;
        private VehicleController controller;
        private float baseForce;
        private float baseTorque;

        private void Awake()
        {
            controller = GetComponent<VehicleController>();
            if (controller)
            {
                baseForce = controller.engineForce;
                baseTorque = controller.steeringTorque;
                ApplySpec(spec);
            }
        }

        public void ApplySpec(VehicleSpec value)
        {
            spec = value;
            if (!controller) controller = GetComponent<VehicleController>();
            if (!controller) return;
            controller.maxSpeed = Mathf.Max(8f, value.maxSpeedKph / 3.6f);
            controller.engineForce = baseForce > 0f ? baseForce * Mathf.Max(0.5f, value.acceleration) : 10500f * Mathf.Max(0.5f, value.acceleration);
            controller.steeringTorque = baseTorque > 0f ? baseTorque * Mathf.Max(0.5f, value.steering) : 5.2f * Mathf.Max(0.5f, value.steering);
            controller.lateralGrip = Mathf.Max(2f, value.grip);
        }
    }
}
