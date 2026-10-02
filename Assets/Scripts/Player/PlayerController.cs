using UnityEngine;
using NeonApocalypse.Combat;
using NeonApocalypse.UI;

namespace NeonApocalypse.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5.5f;
        public float sprintMultiplier = 1.7f;
        public float gravity = -22f;
        public float dashSpeed = 15f;
        public float dashDuration = 0.16f;
        public float dashCooldown = 1.4f;

        [Header("Camera / Aim")]
        public Transform cameraTransform;
        public float turnSpeed = 15f;
        public Weapon equippedWeapon;

        public Vector2 MoveInput { get; set; }
        public bool SprintHeld { get; set; }

        private CharacterController controller;
        private Health health;
        private Vector3 velocity;
        private float dashEnd;
        private float nextDash;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            health.Died += HandleDeath;
            GameManager.Instance?.RegisterPlayer(transform);
        }

        private void Update()
        {
            ReadDesktopInput();
            Move();
            AimAndFire();
            if (Input.GetKeyDown(KeyCode.R)) equippedWeapon?.BeginReload();
            if (Input.GetKeyDown(KeyCode.Space)) TryDash();
        }

        private void ReadDesktopInput()
        {
            Vector2 keyboard = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (keyboard.sqrMagnitude > 0.01f) MoveInput = Vector2.ClampMagnitude(keyboard, 1f);
            SprintHeld = Input.GetKey(KeyCode.LeftShift) || SprintHeld;
        }

        private void Move()
        {
            Vector3 flatForward = cameraTransform ? Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized : Vector3.forward;
            Vector3 flatRight = cameraTransform ? Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized : Vector3.right;
            Vector3 dir = flatForward * MoveInput.y + flatRight * MoveInput.x;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            if (Time.time < dashEnd)
                controller.Move(transform.forward * dashSpeed * Time.deltaTime);
            else
            {
                float speed = moveSpeed * (SprintHeld ? sprintMultiplier : 1f);
                controller.Move(dir * speed * Time.deltaTime);
                if (dir.sqrMagnitude > 0.01f)
                    transform.forward = Vector3.Slerp(transform.forward, dir, turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            MoveInput = Vector2.zero;
            SprintHeld = false;
        }

        private void AimAndFire()
        {
            if (!equippedWeapon) return;
            if (Input.GetMouseButton(0))
            {
                Vector3 direction = cameraTransform ? cameraTransform.forward : transform.forward;
                equippedWeapon.TryFire(direction);
                Vector3 flat = Vector3.Scale(direction, new Vector3(1, 0, 1));
                if (flat.sqrMagnitude > 0.01f)
                    transform.forward = Vector3.Slerp(transform.forward, flat.normalized, turnSpeed * 2f * Time.deltaTime);
            }
        }

        public void SetMoveInput(Vector2 value) => MoveInput = value;
        public void SetSprint(bool value) => SprintHeld = value;
        public void FirePressed()
        {
            if (equippedWeapon)
                equippedWeapon.TryFire(cameraTransform ? cameraTransform.forward : transform.forward);
        }
        public void ReloadPressed() => equippedWeapon?.BeginReload();
        public void DashPressed() => TryDash();

        private void TryDash()
        {
            if (Time.time < nextDash || health.IsDead) return;
            nextDash = Time.time + dashCooldown;
            dashEnd = Time.time + dashDuration;
        }

        private void HandleDeath()
        {
            enabled = false;
            MobileHud.Instance?.ShowMessage("SYSTEM FAILURE", 3f);
        }
    }
}
