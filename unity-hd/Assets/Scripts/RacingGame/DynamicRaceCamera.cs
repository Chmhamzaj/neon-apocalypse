using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class DynamicRaceCamera : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private CarController car;
        [SerializeField] private float baseFov = 62f;
        [SerializeField] private float maxFov = 78f;
        [SerializeField] private float lateralLook = 0.08f;
        [SerializeField] private float smoothing = 6f;
        private void Awake() { if (!cam) cam = GetComponent<Camera>(); if (!car) car = FindFirstObjectByType<CarController>(); }
        private void Update()
        {
            if (!cam || !car) return;
            float speed01 = Mathf.Clamp01(car.SpeedKph / 320f);
            float desired = Mathf.Lerp(baseFov, maxFov, speed01 * speed01);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desired, smoothing * Time.deltaTime);
            float yaw = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f) * lateralLook;
            if (Mathf.Abs(yaw) > 0.001f) transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0f, yaw * 15f, 0f), smoothing * Time.deltaTime);
            else transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, smoothing * Time.deltaTime);
        }
    }
}
