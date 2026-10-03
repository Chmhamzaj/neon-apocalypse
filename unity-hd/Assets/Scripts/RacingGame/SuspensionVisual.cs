using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class SuspensionVisual : MonoBehaviour {
  [SerializeField] Transform[] wheelHubs; [SerializeField] float travel=.08f, response=12f;
  Vector3[] basePos;
  void Awake(){basePos=new Vector3[wheelHubs.Length];for(int i=0;i<wheelHubs.Length;i++)if(wheelHubs[i])basePos[i]=wheelHubs[i].localPosition;}
  void Update(){for(int i=0;i<wheelHubs.Length;i++){var w=wheelHubs[i];if(!w)continue;float target=Mathf.Sin(Time.time*18f+i)*travel*.15f;w.localPosition=Vector3.Lerp(w.localPosition,basePos[i]+Vector3.up*target,1-Mathf.Exp(-response*Time.deltaTime));}}
 }
}