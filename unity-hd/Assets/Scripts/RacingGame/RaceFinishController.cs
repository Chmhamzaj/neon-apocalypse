using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceFinishController : MonoBehaviour
    {
        [SerializeField] private RaceProgress progress;
        [SerializeField] private RaceStateEvents events;
        [SerializeField] private GameObject finishFx;
        private bool fired;

        private void Awake()
        {
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
            if (!events) events = FindFirstObjectByType<RaceStateEvents>();
        }

        private void Update()
        {
            if (fired || !progress || !progress.Finished) return;
            fired = true;
            events?.FinishRace();
            if (finishFx) finishFx.SetActive(true);
        }
    }
}