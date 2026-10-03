using System;
using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceStateEvents : MonoBehaviour
    {
        public static event Action RaceStarted;
        public static event Action RaceFinished;
        public static event Action<int> CheckpointReached;
        public void StartRace() => RaceStarted?.Invoke();
        public void FinishRace() => RaceFinished?.Invoke();
        public void Checkpoint(int index) => CheckpointReached?.Invoke(index);
    }
}
