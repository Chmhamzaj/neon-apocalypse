using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroStreetRush.Racing
{
    public sealed class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public enum ActionType { Left, Right, Nitro, Brake }
        [SerializeField] private ActionType action;
        public static bool LeftHeld { get; private set; }
        public static bool RightHeld { get; private set; }
        public static bool NitroHeld { get; private set; }
        public static bool BrakeHeld { get; private set; }

        public void Configure(ActionType type) => action = type;

        public void OnPointerDown(PointerEventData _) => Set(true);
        public void OnPointerUp(PointerEventData _) => Set(false);
        public void OnPointerExit(PointerEventData _) => Set(false);

        private void Set(bool value)
        {
            switch (action)
            {
                case ActionType.Left: LeftHeld = value; break;
                case ActionType.Right: RightHeld = value; break;
                case ActionType.Nitro: NitroHeld = value; break;
                case ActionType.Brake: BrakeHeld = value; break;
            }
        }

        private void OnDisable() => Set(false);
    }
}