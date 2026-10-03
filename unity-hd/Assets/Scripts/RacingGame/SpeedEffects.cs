using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class SpeedEffects : MonoBehaviour {
  [SerializeField] CarController car; [SerializeField] ParticleSystem speedLines; [SerializeField] CanvasGroup vignette;
  void Update(){if(!car)return;float t=Mathf.InverseLerp(140f,300f,car.SpeedKph);if(speedLines){if(t>.55f&&!speedLines.isPlaying)speedLines.Play();else if(t<=.55f&&speedLines.isPlaying)speedLines.Stop();}if(vignette)vignette.alpha=Mathf.Lerp(0f,.35f,t);}
 }
}