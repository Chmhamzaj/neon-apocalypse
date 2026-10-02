using UnityEngine;
using NeonApocalypse.Progression;

namespace NeonApocalypse.Campaign
{
    public class Region01Director : MonoBehaviour
    {
        public MissionSystem missionSystem;
        public Transform player;
        public float checkpointRadius = 4f;
        public Transform[] checkpoints;
        private int nextCheckpoint;
        private bool finaleStarted;

        private void Update()
        {
            if (player == null || checkpoints == null || nextCheckpoint >= checkpoints.Length) return;
            var p = checkpoints[nextCheckpoint];
            if (p != null && Vector3.SqrMagnitude(player.position - p.position) <= checkpointRadius * checkpointRadius)
            {
                nextCheckpoint++;
                if (missionSystem != null) missionSystem.SendMessage("ShowMessage", "CHECKPOINT REACHED", SendMessageOptions.DontRequireReceiver);
                if (nextCheckpoint >= checkpoints.Length && !finaleStarted)
                {
                    finaleStarted = true;
                    if (missionSystem != null) missionSystem.SendMessage("ShowMessage", "NEMESIS HAS AWAKENED", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
