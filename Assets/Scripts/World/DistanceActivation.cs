using UnityEngine;

namespace NeonApocalypse.World
{
    public class DistanceActivation : MonoBehaviour
    {
        public Transform target;
        public float activeDistance = 90f;
        public bool disableRenderersOnly = true;
        private Renderer[] renderers;
        private bool state = true;

        private void Awake() { renderers = GetComponentsInChildren<Renderer>(true); }
        private void Update()
        {
            if (target == null) return;
            bool next = (target.position - transform.position).sqrMagnitude <= activeDistance * activeDistance;
            if (next == state) return;
            state = next;
            if (disableRenderersOnly)
                foreach (var r in renderers) if (r) r.enabled = state;
            else gameObject.SetActive(state);
        }
    }
}
