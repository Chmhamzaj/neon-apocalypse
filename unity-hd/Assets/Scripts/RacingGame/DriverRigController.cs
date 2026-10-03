using UnityEngine;

namespace NitroStreetRush.Racing
{
    public class DriverRigController : MonoBehaviour
    {
        [SerializeField] private Transform steeringWheel;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        [SerializeField] private float steeringVisualAngle = 22f;
        private Quaternion wheelBase;

        private void Awake()
        {
            if (steeringWheel) wheelBase = steeringWheel.localRotation;
        }

        public void SetSteeringVisual(float input)
        {
            if (steeringWheel)
                steeringWheel.localRotation = wheelBase * Quaternion.Euler(0f, 0f, -input * steeringVisualAngle);
        }
    }
}
