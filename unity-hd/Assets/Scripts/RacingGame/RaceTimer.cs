using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceTimer : MonoBehaviour
    {
        [SerializeField] private RaceFlowController flow;
        public float ElapsedSeconds { get; private set; }
        public bool Running => flow && flow.CurrentState == RaceFlowController.State.Racing;
        private void Awake() { if (!flow) flow = FindFirstObjectByType<RaceFlowController>(); }
        private void Update() { if (Running) ElapsedSeconds += Time.deltaTime; }
        public void ResetTimer() => ElapsedSeconds = 0f;
        public string Formatted => Format(ElapsedSeconds);
        public static string Format(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int whole = Mathf.FloorToInt(seconds) % 60;
            int milli = Mathf.FloorToInt((seconds - Mathf.Floor(seconds)) * 1000f);
            return $"{minutes:00}:{whole:00}.{milli:000}";
        }
    }
}
