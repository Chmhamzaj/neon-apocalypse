using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class VehicleDamage : MonoBehaviour {
  [SerializeField] float maxHealth=100f, impactScale=2.2f, repairRate=3f;
  [SerializeField] ParticleSystem impactFx; [SerializeField] GameObject damagedVisual;
  public float Health {get;private set;}
  void Awake(){Health=maxHealth;}
  void Update(){if(Health<maxHealth)Health=Mathf.Min(maxHealth,Health+repairRate*Time.deltaTime); if(damagedVisual)damagedVisual.SetActive(Health<maxHealth*.35f);}
  void OnCollisionEnter(Collision c){float impact=c.relativeVelocity.magnitude; if(impact<5f)return; float damage=Mathf.Clamp((impact-5f)*impactScale,0f,45f); Health=Mathf.Max(0f,Health-damage); if(impactFx)impactFx.Play();}
 }
}