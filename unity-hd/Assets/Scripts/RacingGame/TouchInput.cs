using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NitroStreetRush.Racing
{
    public sealed class TouchInput : MonoBehaviour
    {
        public static TouchInput Instance { get; private set; }
        public float Steer { get; private set; }
        public bool NitroHeld { get; private set; }
        public bool BrakeHeld { get; private set; }

        private Vector2 steeringStart;
        private int steeringFinger = -1;

        private void Awake() => Instance = this;

        private void Update()
        {
            Steer = (MobileControlButton.RightHeld ? 1f : 0f) - (MobileControlButton.LeftHeld ? 1f : 0f);
            NitroHeld = MobileControlButton.NitroHeld;
            BrakeHeld = MobileControlButton.BrakeHeld;

#if ENABLE_INPUT_SYSTEM
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var control in touchscreen.touches)
                {
                    if (!control.press.isPressed) continue;
                    int finger = control.touchId.ReadValue();
                    Vector2 position = control.position.ReadValue();
                    if (control.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began && position.x < Screen.width * 0.7f)
                    {
                        steeringFinger = finger;
                        steeringStart = position;
                    }
                    if (finger == steeringFinger)
                        Steer = Mathf.Clamp((position.x - steeringStart.x) / (Screen.width * 0.22f), -1f, 1f);
                    if (position.x > Screen.width * 0.72f)
                    {
                        if (position.y > Screen.height * 0.52f) NitroHeld = true;
                        else BrakeHeld = true;
                    }
                }
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                float keyboardSteer = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
                if (Mathf.Abs(keyboardSteer) > 0.01f) Steer = keyboardSteer;
                NitroHeld |= keyboard.leftShiftKey.isPressed;
                BrakeHeld |= keyboard.spaceKey.isPressed;
            }
#else
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began && t.position.x < Screen.width * 0.7f) { steeringFinger = t.fingerId; steeringStart = t.position; }
                if (t.fingerId == steeringFinger && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
                    Steer = Mathf.Clamp((t.position.x - steeringStart.x) / (Screen.width * 0.22f), -1f, 1f);
                if (t.fingerId == steeringFinger && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)) steeringFinger = -1;
                if (t.position.x > Screen.width * 0.72f) { if (t.position.y > Screen.height * 0.52f) NitroHeld = true; else BrakeHeld = true; }
            }
            NitroHeld |= Input.GetKey(KeyCode.LeftShift);
            BrakeHeld |= Input.GetKey(KeyCode.Space);
#endif
        }

        public void SetTarget(CarController _) { }
    }
}