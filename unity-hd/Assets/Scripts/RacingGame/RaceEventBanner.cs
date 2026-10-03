using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class RaceEventBanner : MonoBehaviour
    {
        [SerializeField] private Text bannerText;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private float holdSeconds = 1.2f;
        private float until;

        private void Awake()
        {
            if (!bannerText) bannerText = GetComponentInChildren<Text>();
            if (!group) group = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            RaceStateEvents.RaceStarted += OnRaceStarted;
            RaceStateEvents.RaceFinished += OnRaceFinished;
            RaceStateEvents.CheckpointReached += OnCheckpoint;
        }

        private void OnDisable()
        {
            RaceStateEvents.RaceStarted -= OnRaceStarted;
            RaceStateEvents.RaceFinished -= OnRaceFinished;
            RaceStateEvents.CheckpointReached -= OnCheckpoint;
        }

        private void Update()
        {
            if (!group) return;
            float remaining = until - Time.unscaledTime;
            group.alpha = Mathf.Clamp01(remaining <= 0f ? 0f : Mathf.Min(1f, remaining / 0.25f));
        }

        private void OnRaceStarted() => Show("GO!");
        private void OnRaceFinished() => Show("FINISH");
        private void OnCheckpoint(int number) => Show($"CHECKPOINT  {number}");

        public void Show(string message)
        {
            if (bannerText) bannerText.text = message;
            until = Time.unscaledTime + Mathf.Max(0.25f, holdSeconds);
            SetVisible(true);
        }

        private void SetVisible(bool visible)
        {
            if (group) group.alpha = visible ? 1f : 0f;
        }
    }
}
