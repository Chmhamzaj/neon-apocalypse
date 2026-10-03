using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class VehiclePrefabAuthoring : MonoBehaviour
    {
        [SerializeField] private CarController controller;
        [SerializeField] private VehicleWheelVisual wheelVisuals;
        [SerializeField] private DriverRigController driver;
        [SerializeField] private RaceFXController fx;
        [SerializeField] private CollisionRecovery recovery;
        [SerializeField] private Rigidbody body;

        private void Reset()
        {
            body = GetComponent<Rigidbody>();
            controller = GetComponent<CarController>();
            if (!controller) controller = gameObject.AddComponent<CarController>();
            if (!recovery) recovery = gameObject.GetComponent<CollisionRecovery>();
            if (!recovery) recovery = gameObject.AddComponent<CollisionRecovery>();
            if (!wheelVisuals) wheelVisuals = GetComponentInChildren<VehicleWheelVisual>(true);
            if (!driver) driver = GetComponentInChildren<DriverRigController>(true);
            if (!fx) fx = GetComponentInChildren<RaceFXController>(true);
        }

        public void ConfigureProductionDefaults()
        {
            if (!body) body = GetComponent<Rigidbody>();
            if (body)
            {
                body.mass = 1420f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.centerOfMass = new Vector3(0f, -0.35f, 0.1f);
            }
        }
    }
}