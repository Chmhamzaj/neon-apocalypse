using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class EnvironmentSpawner : MonoBehaviour {
  [SerializeField] Transform player; [SerializeField] GameObject[] buildingPrefabs; [SerializeField] GameObject[] propPrefabs;
  [SerializeField] int count=40; [SerializeField] float spacing=18f, side=28f;
  void Start(){ if(!player)return; for(int i=0;i<count;i++){float z=i*spacing+25f; Spawn(buildingPrefabs, new Vector3(-side-Random.Range(0,14),0,z)); Spawn(buildingPrefabs,new Vector3(side+Random.Range(0,14),0,z)); if(i%2==0)Spawn(propPrefabs,new Vector3(Random.Range(-9f,9f),0,z+8f));}}
  void Spawn(GameObject[] a,Vector3 p){if(a==null||a.Length==0)return; var g=Instantiate(a[Random.Range(0,a.Length)],p,Quaternion.Euler(0,Random.Range(0,360),0)); g.transform.localScale*=Random.Range(.8f,1.25f);}
 }
}