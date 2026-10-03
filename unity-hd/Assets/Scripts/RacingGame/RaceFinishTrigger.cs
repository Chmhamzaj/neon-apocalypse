using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class RaceFinishTrigger : MonoBehaviour {
  [SerializeField] RaceProgress progress;
  [SerializeField] GameObject finishPanel;
  void OnTriggerEnter(Collider other){
   if(!other.GetComponentInParent<CarController>())return;
   if(progress) progress.enabled=false;
   if(GameState.I)GameState.I.Finish();
   if(finishPanel)finishPanel.SetActive(true);
  }
 }
}