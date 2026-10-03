using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 chaseOffset = new(0f, 3.8f, -8.5f);
        [SerializeField] private float positionLerp = 8f;
        [SerializeField] private float rotationLerp = 10f;
        [SerializeField] private float baseFov = 62f;
        [SerializeField] private float boostFov = 75f;
        private Camera cam;
        public void SetTarget(Transform value) => target = value;
        private void Awake() => cam = GetComponent<Camera>();
        private void LateUpdate()
        {
            if (!target) return;
            Vector3 desired = target.TransformPoint(chaseOffset);
            transform.position = Vector3.Lerp(transform.position, desired, positionLerp * Time.deltaTime);
            Quaternion look = Quaternion.LookRotation((target.position + target.forward * 9f + Vector3.up * 1.1f) - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationLerp * Time.deltaTime);
            if (target.TryGetComponent<CarController>(out var car) && cam)
            {
                float desiredFov = Mathf.Lerp(baseFov, boostFov, car.IsBoosting ? 1f : 0f);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, desiredFov, 6f * Time.deltaTime);
            }
        }
    }
}
