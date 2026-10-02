using UnityEngine;
using NeonApocalypse.Player;
using NeonApocalypse.World;

namespace NeonApocalypse.Vehicles
{
    public sealed class VehicleEnterExit : MonoBehaviour
    {
        public float interactionRadius = 3.2f;
        public KeyCode interactKey = KeyCode.E;
        public static VehicleController ActiveVehicle { get; private set; }

        private VehicleController vehicle;
        private SimpleThirdPersonCamera cameraRig;
        private PlayerController playerController;
        private CharacterController playerCharacter;
        private Transform player;
        private Transform originalParent;
        private Vector3 originalScale;

        private static readonly Collider[] Nearby = new Collider[24];

        private void Awake() => vehicle = GetComponent<VehicleController>();

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player)
            {
                playerController = player.GetComponent<PlayerController>();
                playerCharacter = player.GetComponent<CharacterController>();
            }
            cameraRig = Camera.main ? Camera.main.GetComponent<SimpleThirdPersonCamera>() : null;
        }

        private void Update()
        {
            if (!player || !vehicle) return;
            if (Input.GetKeyDown(interactKey))
            {
                if (ActiveVehicle == vehicle) ExitVehicle();
                else if (!ActiveVehicle && Vector3.SqrMagnitude(player.position - transform.position) <= interactionRadius * interactionRadius)
                    EnterVehicle();
            }
        }

        public void EnterVehicle()
        {
            if (!player || !vehicle || ActiveVehicle) return;
            if (Vector3.SqrMagnitude(player.position - transform.position) > interactionRadius * interactionRadius) return;

            originalParent = player.parent;
            originalScale = player.localScale;
            player.SetParent(vehicle.driverSeat ? vehicle.driverSeat : transform, false);
            player.localPosition = Vector3.zero;
            player.localRotation = Quaternion.identity;
            player.localScale = originalScale;
            if (playerCharacter) playerCharacter.enabled = false;
            if (playerController) playerController.enabled = false;
            vehicle.IsOccupied = true;
            vehicle.SetPlayerControlled(true);
            ActiveVehicle = vehicle;

            if (cameraRig)
            {
                cameraRig.SetTarget(vehicle.cameraTarget ? vehicle.cameraTarget : transform, new Vector3(0f, 5.3f, -10.5f));
            }
        }

        public void ExitVehicle()
        {
            if (ActiveVehicle != vehicle || !player) return;
            Vector3 exit = vehicle.exitPoint ? vehicle.exitPoint.position : transform.position + transform.right * 2.5f;
            Quaternion rot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            player.SetParent(originalParent, true);
            player.position = exit + Vector3.up * 0.1f;
            player.rotation = rot;
            player.localScale = originalScale;
            if (playerCharacter) playerCharacter.enabled = true;
            if (playerController) playerController.enabled = true;
            vehicle.IsOccupied = false;
            vehicle.SetPlayerControlled(false);
            ActiveVehicle = null;

            if (cameraRig)
            {
                cameraRig.SetTarget(player, new Vector3(0f, 4.5f, -7.5f));
            }
        }

        public static void EnterNearestVehicle(float radius = 4.5f)
        {
            Transform p = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (!p || ActiveVehicle) return;
            if (TryFindNearestVehicle(p.position, radius, out var candidate)) candidate.EnterVehicle();
        }

        public static void ExitActiveVehicle()
        {
            var active = ActiveVehicle;
            if (active) active.GetComponent<VehicleEnterExit>()?.ExitVehicle();
        }

        public static bool TryFindNearestVehicle(Vector3 origin, float radius, out VehicleEnterExit result)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, radius, Nearby, ~0, QueryTriggerInteraction.Ignore);
            float best = radius * radius;
            result = null;
            for (int i = 0; i < count; i++)
            {
                var candidate = Nearby[i] ? Nearby[i].GetComponentInParent<VehicleEnterExit>() : null;
                if (!candidate || !candidate.vehicle || candidate.vehicle.IsOccupied) continue;
                float d = Vector3.SqrMagnitude(candidate.transform.position - origin);
                if (d < best) { best = d; result = candidate; }
            }
            return result != null;
        }
    }
}
