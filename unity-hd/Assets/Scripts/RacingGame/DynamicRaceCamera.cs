using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class DynamicRaceCamera : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private CarController car;
        [SerializeField] private TouchInput touchInput;
        [SerializeField] private float baseFov = 62f;
        [SerializeField] private float maxFov = 78f;
        [SerializeField] private float lateralLook = 0.08f;
        [SerializeField] private float smoothing = 6f;

        private void Awake()
        {
            if (!cam) cam = GetComponent<Camera>();
            if (!car) car = FindFirstObjectByType<CarController>();
            if (!touchInput) touchInput = FindFirstObjectByType<TouchInput>();
        }

        private void Update()
        {
            if (!cam || !car) return;
            float speed01 = Mathf.Clamp01(car.SpeedKph / 320f);
            float desired = Mathf.Lerp(baseFov, maxFov, speed01 * speed01);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desired, smoothing * Time.deltaTime);

            float steer = touchInput ? touchInput.Steer : 0f;
            float yaw = steer * lateralLook;
            Quaternion target = Mathf.Abs(yaw) > 0.001f ? Quaternion.Euler(0f, yaw * 15f, 0f) : Quaternion.identity;
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, smoothing * Time.deltaTime);
        }
    }
}