using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class RaceObjectiveHUD : MonoBehaviour
    {
        [SerializeField] private RaceObjectiveSystem objectives;
        [SerializeField] private RaceTimer timer;
        [SerializeField] private RaceComboSystem combo;
        [SerializeField] private RaceFlowController flow;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text statusText;
        [SerializeField] private Slider scoreBar;

        private void Awake()
        {
            if (!objectives) objectives = FindFirstObjectByType<RaceObjectiveSystem>();
            if (!timer) timer = FindFirstObjectByType<RaceTimer>();
            if (!combo) combo = FindFirstObjectByType<RaceComboSystem>();
            if (!flow) flow = FindFirstObjectByType<RaceFlowController>();
        }

        private void Update()
        {
            if (!objectives) return;

            if (objectiveText)
                objectiveText.text = objectives.ObjectiveText;

            if (scoreBar)
                scoreBar.value = objectives.ScoreProgress;

            if (statusText)
            {
                string time = timer ? timer.Formatted : "00:00.000";
                string score = combo ? combo.Score.ToString("N0") : "0";
                string state = flow && flow.CurrentState == RaceFlowController.State.Finished
                    ? (objectives.AllObjectivesComplete ? "OBJECTIVES CLEAR" : "RACE COMPLETE")
                    : $"TIME {time}   •   SCORE {score}";
                statusText.text = state;
            }
        }
    }
}
