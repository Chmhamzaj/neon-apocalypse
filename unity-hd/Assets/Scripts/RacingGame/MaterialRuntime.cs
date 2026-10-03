using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class MaterialRuntime : MonoBehaviour {
  [SerializeField] Renderer[] renderers;
  [SerializeField] Color bodyColor = new Color(.08f,.12f,.16f,1);
  [SerializeField] float metallic=.82f, smoothness=.72f;
  [SerializeField] Color emissionColor = new Color(.02f,.25f,1f,1);
  [SerializeField] float emission=.0f;
  void Awake(){
   foreach(var r in renderers){ if(!r) continue; foreach(var m in r.materials){ if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",bodyColor); if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic); if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",smoothness); if(m.HasProperty("_EmissionColor")&&emission>0)m.SetColor("_EmissionColor",emissionColor*emission); } }
  }
 }
}