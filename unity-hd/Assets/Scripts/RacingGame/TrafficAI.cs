using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class TrafficAI : MonoBehaviour {
  [SerializeField] float cruiseSpeed=22f, laneWidth=3.5f, laneChangeRate=1.5f;
  int lane; Transform player;
  void Start(){var p=GameObject.FindGameObjectWithTag("Player"); if(p)player=p.transform; lane=Random.Range(-1,2);}
  void Update(){
   transform.Translate(Vector3.forward*cruiseSpeed*Time.deltaTime);
   if(player && Vector3.Distance(transform.position,player.position)<22f){
    float targetLane=lane*laneWidth;
    Vector3 local=transform.parent?transform.parent.InverseTransformPoint(transform.position):transform.position;
    local.x=Mathf.Lerp(local.x,targetLane,Time.deltaTime*laneChangeRate);
    if(transform.parent) transform.position=transform.parent.TransformPoint(local); else transform.position=new Vector3(Mathf.Lerp(transform.position.x,targetLane,Time.deltaTime*laneChangeRate),transform.position.y,transform.position.z);
   }
  }
 }
}