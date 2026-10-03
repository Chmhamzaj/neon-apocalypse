using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class GameplayFeedback : MonoBehaviour
    {
        [SerializeField] private RaceComboSystem combo;
        [SerializeField] private DrivingTrickSystem tricks;
        private float lastDrift;
        private void Awake() { combo = combo ? combo : FindFirstObjectByType<RaceComboSystem>(); tricks = tricks ? tricks : FindFirstObjectByType<DrivingTrickSystem>(); }
        private void Update()
        {
            if (!combo || !tricks) return;
            if (tricks.Drifting) { lastDrift += Time.deltaTime; combo.AddDrift(Time.deltaTime); }
            else lastDrift = 0f;
        }
    }
}
