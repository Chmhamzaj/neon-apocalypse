using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class CameraFX : MonoBehaviour {
  [SerializeField] Transform target; [SerializeField] CarController car;
  [SerializeField] float distance=7.2f, height=2.3f, follow=8f, lookAhead=5f, normalFov=62f, boostFov=74f;
  Camera cam;
  void Awake(){cam=GetComponent<Camera>();}
  void LateUpdate(){
   if(!target) return;
   float boost=car && car.IsBoosting ? 1f : 0f;
   Vector3 desired=target.position-target.forward*distance+Vector3.up*height;
   transform.position=Vector3.Lerp(transform.position,desired,1f-Mathf.Exp(-follow*Time.deltaTime));
   Vector3 look=target.position+target.forward*(lookAhead+boost*3f)+Vector3.up*1.0f;
   transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(look-transform.position),1f-Mathf.Exp(-10f*Time.deltaTime));
   if(cam) cam.fieldOfView=Mathf.Lerp(cam.fieldOfView, boostFov*boost+normalFov*(1f-boost),1f-Mathf.Exp(-5f*Time.deltaTime));
  }
 }
}