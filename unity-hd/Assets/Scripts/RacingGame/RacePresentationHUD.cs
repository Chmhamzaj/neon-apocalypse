using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class RacePresentationHUD : MonoBehaviour
    {
        [SerializeField] private RaceFlowController flow;
        [SerializeField] private CarController car;
        [SerializeField] private RaceProgress progress;
        [SerializeField] private Text speedText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text stateText;
        [SerializeField] private Slider nitroBar;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Slider damageBar;

        private void Update()
        {
            if (car)
            {
                if (speedText) speedText.text = $"{Mathf.RoundToInt(car.SpeedKph)} KM/H";
                if (nitroBar) nitroBar.value = Mathf.Clamp01(car.Nitro / 100f);
            }
            if (progress)
            {
                if (progressBar) progressBar.value = progress.Progress;
                if (progressText) progressText.text = $"{Mathf.RoundToInt(progress.Progress * 100f)}%";
            }
            if (flow && stateText)
            {
                stateText.text = flow.CurrentState == RaceFlowController.State.Countdown ? $"{Mathf.CeilToInt(Mathf.Max(0f, flow.CountdownRemaining))}" : flow.CurrentState == RaceFlowController.State.Finished ? "FINISH" : "RACE";
            }
        }

        public void SetDamage(float value)
        {
            if (damageBar) damageBar.value = Mathf.Clamp01(value);
        }
    }
}