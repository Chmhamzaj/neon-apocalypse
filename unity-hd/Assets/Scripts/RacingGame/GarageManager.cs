using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class GarageManager : MonoBehaviour {
  [SerializeField] VehicleProfile[] vehicles;
  const string SelectedKey="NSR_SelectedVehicle";
  public VehicleProfile Selected { get; private set; }
  void Awake(){ Select(PlayerPrefs.GetString(SelectedKey, vehicles!=null&&vehicles.Length>0?vehicles[0].vehicleId:"")); }
  public void Select(string id){ if(vehicles==null)return; foreach(var v in vehicles) if(v&&v.vehicleId==id){Selected=v;PlayerPrefs.SetString(SelectedKey,id);PlayerPrefs.Save();return;} if(Selected==null&&vehicles.Length>0)Selected=vehicles[0]; }
  public bool IsUnlocked(VehicleProfile v){return v&&PlayerPrefs.GetInt("NSR_UNLOCK_"+v.vehicleId,v.unlockCost==0?1:0)==1;}
  public bool Unlock(VehicleProfile v,int credits){if(!v||IsUnlocked(v)||credits<v.unlockCost)return false;PlayerPrefs.SetInt("NSR_UNLOCK_"+v.vehicleId,1);PlayerPrefs.Save();return true;}
 }
}