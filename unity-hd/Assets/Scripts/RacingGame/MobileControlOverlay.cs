using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#else
using UnityEngine.EventSystems;
#endif

namespace NitroStreetRush.Racing
{
    public sealed class MobileControlOverlay : MonoBehaviour
    {
        public void Configure(Canvas canvas)
        {
            if (!canvas) return;
            EnsureEventSystem();
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>()) return;
            var go = new GameObject("EventSystem");
            var eventSystem = go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
