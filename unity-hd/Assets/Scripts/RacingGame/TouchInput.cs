using UnityEngine;

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
            Steer = 0f;
            NitroHeld = false;
            BrakeHeld = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch t = Input.GetTouch(i);
                if (t.phase == TouchPhase.Began && t.position.x < Screen.width * 0.7f)
                {
                    steeringFinger = t.fingerId;
                    steeringStart = t.position;
                }

                if (t.fingerId == steeringFinger &&
                    (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary))
                {
                    Steer = Mathf.Clamp((t.position.x - steeringStart.x) / (Screen.width * 0.22f), -1f, 1f);
                }

                if (t.fingerId == steeringFinger && t.phase == TouchPhase.Ended)
                    steeringFinger = -1;

                if (t.position.x > Screen.width * 0.72f)
                {
                    if (t.position.y > Screen.height * 0.52f) NitroHeld = true;
                    else BrakeHeld = true;
                }
            }

            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f)
                Steer = Input.GetAxisRaw("Horizontal");
            NitroHeld |= Input.GetKey(KeyCode.LeftShift);
            BrakeHeld |= Input.GetKey(KeyCode.Space);
        }
    }
}
