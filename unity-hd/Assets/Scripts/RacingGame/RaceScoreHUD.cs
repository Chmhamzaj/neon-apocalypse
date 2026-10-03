using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class RaceScoreHUD : MonoBehaviour
    {
        [SerializeField] private RaceComboSystem combo;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text comboText;

        private void Awake()
        {
            if (!combo) combo = FindFirstObjectByType<RaceComboSystem>();
        }

        private void Update()
        {
            if (!combo) return;
            if (scoreText) scoreText.text = $"SCORE  {combo.Score:N0}";
            if (comboText) comboText.text = combo.Combo > 1 ? $"COMBO  x{combo.Combo}" : "";
        }
    }
}
