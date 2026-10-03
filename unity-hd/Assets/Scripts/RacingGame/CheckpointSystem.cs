using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class CheckpointSystem : MonoBehaviour {
  [SerializeField] Transform[] checkpoints; [SerializeField] int current;
  public int Current=>current;
  void OnTriggerEnter(Collider other){ if(current>=checkpoints.Length)return; if(other.transform.root==transform.root)return; if(other.CompareTag("Player")||other.GetComponentInParent<CarController>()){ if(other.transform.position.z>=checkpoints[current].position.z) current++; } }
 }
}