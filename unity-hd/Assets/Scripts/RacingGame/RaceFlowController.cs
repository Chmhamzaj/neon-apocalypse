using System.Collections;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceFlowController : MonoBehaviour
    {
        public enum State { Countdown, Racing, Finished }
        [SerializeField] private RaceStateEvents events;
        [SerializeField] private CarController player;
        [SerializeField] private RaceProgress progress;
        [SerializeField] private float countdownSeconds = 3f;
        public State CurrentState { get; private set; } = State.Countdown;
        public float CountdownRemaining { get; private set; }

        private void Awake()
        {
            if (!events) events = FindFirstObjectByType<RaceStateEvents>();
            if (!player) player = FindFirstObjectByType<CarController>();
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
        }

        private void Start()
        {
            StartCoroutine(BeginRace());
        }

        private IEnumerator BeginRace()
        {
            CurrentState = State.Countdown;
            CountdownRemaining = countdownSeconds;
            if (player) player.enabled = false;
            while (CountdownRemaining > 0f)
            {
                CountdownRemaining -= Time.deltaTime;
                yield return null;
            }
            CurrentState = State.Racing;
            if (player) player.enabled = true;
            events?.StartRace();
        }

        private void Update()
        {
            if (CurrentState == State.Racing && progress && progress.Finished)
            {
                CurrentState = State.Finished;
                if (player) player.enabled = false;
                events?.FinishRace();
            }
        }
    }
}