using UnityEngine;

namespace NitroStreetRush.Racing
{
    public class RaceFXController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem nitroFx;
        [SerializeField] private ParticleSystem[] tireSmoke;
        [SerializeField] private Light[] brakeLights;
        [SerializeField] private float brakeLightIntensity = 7f;

        public void SetNitro(bool active)
        {
            if (!nitroFx) return;
            if (active && !nitroFx.isPlaying) nitroFx.Play();
            else if (!active && nitroFx.isPlaying) nitroFx.Stop();
        }

        public void SetDrift(bool active)
        {
            if (tireSmoke == null) return;
            foreach (var fx in tireSmoke)
            {
                if (!fx) continue;
                if (active && !fx.isPlaying) fx.Play();
                else if (!active && fx.isPlaying) fx.Stop();
            }
        }

        public void SetBraking(bool active)
        {
            if (brakeLights == null) return;
            foreach (var light in brakeLights)
                if (light) light.intensity = active ? brakeLightIntensity : 0f;
        }
    }
}
