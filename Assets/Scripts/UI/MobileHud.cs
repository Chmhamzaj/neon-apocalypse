using UnityEngine;
using UnityEngine.UI;
using NeonApocalypse.Player;
using NeonApocalypse.Combat;
using NeonApocalypse.Core;

namespace NeonApocalypse.UI
{
    public class MobileHud : MonoBehaviour
    {
        public static MobileHud Instance { get; private set; }
        public Text healthText;
        public Text xpText;
        public Text ammoText;
        public Text missionText;
        public Text messageText;
        public MobileJoystick joystick;
        public Button fireButton;
        public Button reloadButton;
        public Button dashButton;

        private PlayerController player;
        private Health playerHealth;
        private float messageUntil;

        private void Awake() { Instance = this; }

        public void BindToPlayer()
        {
            player = FindFirstObjectByType<PlayerController>();
            if (!player) return;
            playerHealth = player.GetComponent<Health>();
            if (joystick) joystick.ValueChanged += v => player.SetMoveInput(v);
            if (fireButton) fireButton.onClick.AddListener(player.FirePressed);
            if (reloadButton) reloadButton.onClick.AddListener(player.ReloadPressed);
            if (dashButton) dashButton.onClick.AddListener(player.DashPressed);
            if (player.equippedWeapon)
            {
                player.equippedWeapon.AmmoChanged += (current, max) => SetAmmo(current, max);
                SetAmmo(player.equippedWeapon.CurrentAmmo, player.equippedWeapon.magazineSize);
            }
        }

        private void Update()
        {
            if (playerHealth) healthText.text = $"HP {Mathf.CeilToInt(playerHealth.Current)} / {Mathf.CeilToInt(playerHealth.maxHealth)}";
            if (GameManager.Instance)
                xpText.text = $"LV {GameManager.Instance.PlayerLevel}  XP {GameManager.Instance.PlayerXP}/{GameManager.Instance.XPForNextLevel}";
            if (messageText && Time.time > messageUntil) messageText.text = string.Empty;
        }

        public void SetAmmo(int current, int max)
        {
            if (ammoText) ammoText.text = $"AMMO {current:00}/{max:00}";
        }

        public void SetMission(string text) { if (missionText) missionText.text = text; }

        public void ShowMessage(string text, float seconds)
        {
            if (!messageText) return;
            messageText.text = text;
            messageUntil = Time.time + seconds;
        }
    }

    public class MobileJoystick : MonoBehaviour, UnityEngine.EventSystems.IDragHandler, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
    {
        public RectTransform handle;
        public float radius = 90f;
        public Vector2 Value { get; private set; }
        public System.Action<Vector2> ValueChanged;
        private RectTransform rect;
        private void Awake() => rect = transform as RectTransform;
        public void OnPointerDown(UnityEngine.EventSystems.PointerEventData e) => OnDrag(e);
        public void OnDrag(UnityEngine.EventSystems.PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var p)) return;
            Value = Vector2.ClampMagnitude(p / radius, 1f);
            if (handle) handle.anchoredPosition = Value * radius;
            ValueChanged?.Invoke(Value);
        }
        public void OnPointerUp(UnityEngine.EventSystems.PointerEventData e)
        {
            Value = Vector2.zero;
            if (handle) handle.anchoredPosition = Vector2.zero;
            ValueChanged?.Invoke(Value);
        }
    }
}
