using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NitroStreetRush.Racing
{
    public sealed class MobileSteering : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 1.5f;
        [SerializeField] private float smoothing = 8f;
        public float Value { get; private set; }
        private void Update()
        {
            float axis = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Accelerometer.current != null) axis = Accelerometer.current.acceleration.ReadValue().x;
#else
            axis = Input.acceleration.x;
#endif
            float target = Mathf.Clamp(axis * sensitivity, -1f, 1f);
            Value = Mathf.Lerp(Value, target, smoothing * Time.deltaTime);
        }
    }
}
