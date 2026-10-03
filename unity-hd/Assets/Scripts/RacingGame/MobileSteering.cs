using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class MobileSteering : MonoBehaviour
    {
        [SerializeField] private float sensitivity = 1.5f;
        [SerializeField] private float smoothing = 8f;
        public float Value { get; private set; }
        private void Update()
        {
            float target = Mathf.Clamp(Input.acceleration.x * sensitivity, -1f, 1f);
            Value = Mathf.Lerp(Value, target, smoothing * Time.deltaTime);
        }
    }
}
