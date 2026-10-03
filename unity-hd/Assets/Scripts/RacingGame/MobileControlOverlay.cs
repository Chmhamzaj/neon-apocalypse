using UnityEngine;
using UnityEngine.EventSystems;

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
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}