using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceAudioEvents : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private AudioSource engine;
        [SerializeField] private AudioSource nitro;
        [SerializeField] private AudioSource brake;
        [SerializeField] private float minPitch = 0.82f;
        [SerializeField] private float maxPitch = 1.55f;
        private bool boostLast;
        private bool brakeLast;
        private void Awake() { if (!car) car = GetComponentInParent<CarController>(); }
        private void Update()
        {
            if (!car) return;
            float speed01 = Mathf.Clamp01(car.SpeedKph / 320f);
            if (engine) { engine.pitch = Mathf.Lerp(minPitch, maxPitch, speed01); engine.volume = Mathf.Lerp(0.25f, 0.85f, speed01); if (!engine.isPlaying) engine.Play(); }
            bool boosting = car.IsBoosting;
            if (nitro && boosting && !boostLast) nitro.Play();
            bool braking = Input.GetKey(KeyCode.Space);
            if (brake && braking && !brakeLast) brake.Play();
            boostLast = boosting; brakeLast = braking;
        }
    }
}
