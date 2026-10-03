using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class DriftFX : MonoBehaviour {
  [SerializeField] CarController car; [SerializeField] ParticleSystem[] tireSmoke; [SerializeField] TrailRenderer[] skidTrails;
  void Update(){if(!car)return;bool drifting=car.SpeedKph>45f;foreach(var p in tireSmoke)if(p){if(drifting&&!p.isPlaying)p.Play();else if(!drifting&&p.isPlaying)p.Stop();}foreach(var t in skidTrails)if(t)t.emitting=drifting;}
 }
}