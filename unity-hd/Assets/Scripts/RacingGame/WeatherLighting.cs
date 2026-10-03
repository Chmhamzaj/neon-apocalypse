using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class WeatherLighting : MonoBehaviour {
  [SerializeField] Light sun; [SerializeField] Light[] streetLights; [SerializeField] float nightThreshold=.35f;
  [SerializeField] float cycleSeconds=180f; float t;
  void Update(){t=(t+Time.deltaTime/cycleSeconds)%1f;float daylight=(Mathf.Sin(t*Mathf.PI*2f-Mathf.PI*.5f)+1f)*.5f;
   if(sun){sun.intensity=Mathf.Lerp(.15f,1.2f,daylight);sun.transform.rotation=Quaternion.Euler(Mathf.Lerp(165f,35f,daylight),-35f,0);}
   foreach(var l in streetLights)if(l)l.intensity=Mathf.Lerp(5f,0f,daylight);
  }
 }
}