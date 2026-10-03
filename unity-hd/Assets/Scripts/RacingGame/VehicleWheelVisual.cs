using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class VehicleWheelVisual : MonoBehaviour
    {
        [SerializeField] private Transform[] wheels;
        [SerializeField] private float wheelRadius = 0.34f;
        [SerializeField] private float steerAngle = 28f;
        private Quaternion[] bases;
        private float distanceTravelled;
        private float lastZ;

        private void Awake()
        {
            bases = new Quaternion[wheels != null ? wheels.Length : 0];
            for (int i = 0; i < bases.Length; i++) if (wheels[i]) bases[i] = wheels[i].localRotation;
            lastZ = transform.position.z;
        }

        public void SetSteering(float input)
        {
            if (wheels == null) return;
            float angle = input * steerAngle;
            for (int i = 0; i < wheels.Length; i++)
            {
                if (!wheels[i]) continue;
                bool front = i < wheels.Length * 0.5f;
                wheels[i].localRotation = bases[i] * (front ? Quaternion.Euler(0f, angle, 0f) : Quaternion.identity);
            }
        }

        private void Update()
        {
            float dz = transform.position.z - lastZ;
            lastZ = transform.position.z;
            distanceTravelled += Mathf.Abs(dz);
            float degrees = (distanceTravelled / Mathf.Max(0.05f, wheelRadius)) * Mathf.Rad2Deg;
            if (wheels != null)
                for (int i = 0; i < wheels.Length; i++) if (wheels[i]) wheels[i].localRotation *= Quaternion.Euler(degrees * Time.deltaTime, 0f, 0f);
        }
    }
}
