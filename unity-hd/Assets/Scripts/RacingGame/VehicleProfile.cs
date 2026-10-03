using UnityEngine;
namespace NitroStreetRush.Racing {
 [CreateAssetMenu(menuName="Nitro Street Rush/Vehicle Profile")]
 public sealed class VehicleProfile : ScriptableObject {
  public string vehicleId="hero_01";
  public string displayName="NSR Apex";
  public GameObject prefab;
  public float topSpeedKph=320f;
  public float acceleration=42f;
  public float handling=2.6f;
  public float nitroCapacity=100f;
  public int unlockCost=0;
 }
}