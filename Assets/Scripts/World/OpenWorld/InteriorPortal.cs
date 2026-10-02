using UnityEngine;
using NeonApocalypse.Core;

namespace NeonApocalypse.World.OpenWorld
{
    public sealed class InteriorPortal : MonoBehaviour
    {
        public string interiorName = "SAFEHOUSE";
        public Vector3 interiorOffset = new Vector3(0f, 0f, 500f);
        public float enterRadius = 18f;
        public float exitRadius = 7f;

        private bool inside;
        private Transform player;
        private Transform interiorRoot;
        private Vector3 interiorDoorWorld;

        private void Start() => player = GameManager.Instance?.PlayerTransform;

        private void Update()
        {
            if (!player) player = GameManager.Instance?.PlayerTransform;
            if (!player) return;

            if (!inside && Vector3.SqrMagnitude(player.position - transform.position) < enterRadius * enterRadius)
            {
                Enter();
            }
            else if (inside && Vector3.SqrMagnitude(player.position - interiorDoorWorld) < exitRadius * exitRadius)
            {
                Exit();
            }
        }

        public void Enter()
        {
            if (inside || !player) return;
            if (!InteriorPoolService.Instance)
            {
                var serviceGo = new GameObject("InteriorPoolService");
                serviceGo.AddComponent<InteriorPoolService>();
            }
            Vector3 rootPos = transform.position + interiorOffset;
            interiorRoot = InteriorPoolService.Instance.Acquire(interiorName, rootPos);
            inside = true;
            interiorDoorWorld = rootPos + new Vector3(0f, 1.2f, -9f);
            player.position = rootPos + Vector3.up * 1.2f;
            NeonApocalypse.UI.MobileHud.Instance?.ShowMessage("ENTERED • " + interiorName, 2.5f);
        }

        public void Exit()
        {
            if (!inside || !player) return;
            player.position = transform.position + transform.forward * 3f + Vector3.up * 0.1f;
            if (InteriorPoolService.Instance) InteriorPoolService.Instance.Release(interiorName);
            interiorRoot = null;
            inside = false;
        }
    }
}
