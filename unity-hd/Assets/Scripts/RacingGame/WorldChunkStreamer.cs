using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class WorldChunkStreamer : MonoBehaviour {
  [SerializeField] Transform player; [SerializeField] GameObject[] chunks; [SerializeField] float chunkLength=120f, ahead=3, behind=1;
  int lastBase=int.MinValue;
  void Update(){if(!player||chunks==null||chunks.Length==0)return;int b=Mathf.FloorToInt(player.position.z/chunkLength);if(b==lastBase)return;lastBase=b;
   for(int i=0;i<chunks.Length;i++)if(chunks[i]){float d=Mathf.Abs(i-b);chunks[i].SetActive(d<=ahead||i>=b-behind&&i<=b+ahead);}}
 }
}