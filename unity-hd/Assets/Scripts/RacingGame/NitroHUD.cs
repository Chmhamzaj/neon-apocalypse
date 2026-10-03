using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class NitroHUD : MonoBehaviour
    {
        [SerializeField] private CarController car;
        [SerializeField] private Slider nitroBar;
        [SerializeField] private Slider speedBar;

        private void Update()
        {
            if (!car) return;
            if (nitroBar) nitroBar.value = car.Nitro / 100f;
            if (speedBar) speedBar.value = Mathf.Clamp01(car.SpeedKph / 320f);
        }
    }
}
