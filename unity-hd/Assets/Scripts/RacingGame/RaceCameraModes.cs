using UnityEngine;

namespace NitroStreetRush.Racing
{
    public sealed class RaceCameraModes : MonoBehaviour
    {
        public enum Mode { Chase, Cockpit, Cinematic }
        [SerializeField] private RaceCamera chaseCamera;
        [SerializeField] private Camera cockpitCamera;
        [SerializeField] private Camera cinematicCamera;
        [SerializeField] private Transform cockpitTarget;
        [SerializeField] private float cinematicFov = 70f;
        public Mode CurrentMode { get; private set; } = Mode.Chase;

        private void Awake()
        {
            ApplyMode(CurrentMode);
        }

        public void CycleMode()
        {
            int next = ((int)CurrentMode + 1) % 3;
            ApplyMode((Mode)next);
        }

        public void ApplyMode(Mode mode)
        {
            CurrentMode = mode;
            if (cockpitCamera) cockpitCamera.enabled = mode == Mode.Cockpit;
            if (cinematicCamera) { cinematicCamera.enabled = mode == Mode.Cinematic; cinematicCamera.fieldOfView = cinematicFov; }
            if (chaseCamera) chaseCamera.enabled = mode == Mode.Chase;
        }
    }
}