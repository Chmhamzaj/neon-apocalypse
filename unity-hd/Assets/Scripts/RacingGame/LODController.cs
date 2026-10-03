using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class LODController : MonoBehaviour {
  [SerializeField] LODGroup[] groups;
  [SerializeField] float quality=1f;
  void Awake(){foreach(var g in groups)if(g){var ls=g.GetLODs();for(int i=0;i<ls.Length;i++){float s=ls[i].screenRelativeTransitionHeight;ls[i].screenRelativeTransitionHeight=Mathf.Clamp(s*quality,.01f,.9f);}g.SetLODs(ls);g.RecalculateBounds();}}
 }
}