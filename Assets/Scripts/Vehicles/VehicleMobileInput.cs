using UnityEngine;
using NeonApocalypse.UI;

namespace NeonApocalypse.Vehicles
{
    public sealed class VehicleMobileInput : MonoBehaviour
    {
        private VehicleController vehicle;
        private MobileJoystick joystick;
        private Vector2 input;

        private void Awake() => vehicle = GetComponent<VehicleController>();

        private void Start()
        {
            joystick = FindFirstObjectByType<MobileJoystick>();
            if (joystick) joystick.ValueChanged += OnJoystick;
        }

        private void OnDestroy()
        {
            if (joystick) joystick.ValueChanged -= OnJoystick;
        }

        private void OnJoystick(Vector2 value) => input = value;

        private void Update()
        {
            if (!vehicle) return;
            if (vehicle.playerControlled)
                vehicle.SetInputs(input.y, input.x, false);
            else
                vehicle.SetInputs(0f, 0f, false);
        }
    }
}
