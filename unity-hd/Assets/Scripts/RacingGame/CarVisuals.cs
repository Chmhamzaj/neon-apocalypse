using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class CarVisuals : MonoBehaviour {
  [SerializeField] CarController car; [SerializeField] Transform[] wheels;
  [SerializeField] ParticleSystem nitroFx; [SerializeField] Light[] headlights;
  [SerializeField] float wheelRadius=.34f;
  void Update(){
   if(!car) return; float speed=car.SpeedKph;
   foreach(var w in wheels) if(w) w.Rotate(Vector3.right,speed*8f*Time.deltaTime,Space.Self);
   bool boost=car.IsBoosting;
   if(nitroFx){if(boost&&!nitroFx.isPlaying)nitroFx.Play();else if(!boost&&nitroFx.isPlaying)nitroFx.Stop();}
   foreach(var l in headlights) if(l) l.intensity=Mathf.Lerp(l.intensity,boost?5f:2f,Time.deltaTime*8f);
  }
 }
}