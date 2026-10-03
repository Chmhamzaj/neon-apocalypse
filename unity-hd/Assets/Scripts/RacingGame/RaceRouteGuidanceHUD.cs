using UnityEngine;
using UnityEngine.UI;

namespace NitroStreetRush.Racing
{
    public sealed class RaceRouteGuidanceHUD : MonoBehaviour
    {
        [SerializeField] private RaceSplinePath path;
        [SerializeField] private RaceProgress progress;
        [SerializeField] private Transform player;
        [SerializeField] private Text directionText;
        [SerializeField] private Text distanceText;

        private void Awake()
        {
            if (!path) path = FindFirstObjectByType<RaceSplinePath>();
            if (!progress) progress = FindFirstObjectByType<RaceProgress>();
            if (!player)
            {
                var car = FindFirstObjectByType<CarController>();
                if (car) player = car.transform;
            }
        }

        private void Update()
        {
            if (!path || !progress || !player || !directionText) return;

            float current = progress.DistanceTravelled;
            float lookDistance = Mathf.Min(path.TotalLength, current + 110f);
            Vector3 tangent = path.GetTangent(lookDistance);
            float angle = Vector3.SignedAngle(player.forward, tangent, Vector3.up);

            string direction;
            float magnitude = Mathf.Abs(angle);
            if (magnitude < 10f) direction = "STRAIGHT";
            else if (magnitude < 32f) direction = angle < 0f ? "SLIGHT LEFT" : "SLIGHT RIGHT";
            else direction = angle < 0f ? "LEFT" : "RIGHT";

            directionText.text = $"NEXT  {direction}";

            if (distanceText)
            {
                float remaining = Mathf.Max(0f, path.TotalLength - current);
                distanceText.text = remaining >= 1000f
                    ? $"{remaining / 1000f:0.0} KM"
                    : $"{Mathf.RoundToInt(remaining):N0} M";
            }
        }
    }
}
