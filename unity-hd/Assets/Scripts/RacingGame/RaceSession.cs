using UnityEngine;
namespace NitroStreetRush.Racing {
 public sealed class RaceSession : MonoBehaviour {
  [SerializeField] CarController player;
  public float Elapsed {get;private set;}
  public int CreditsEarned {get;private set;}
  void Update(){if(!player||GameState.I==null||GameState.I.Current!=GameState.State.Racing)return;Elapsed+=Time.deltaTime;}
  public void Complete(int baseReward=100){CreditsEarned=Mathf.RoundToInt(baseReward+Mathf.Max(0f,300f-Elapsed));PlayerPrefs.SetInt("NSR_Credits",PlayerPrefs.GetInt("NSR_Credits",0)+CreditsEarned);PlayerPrefs.SetFloat("NSR_BestTime",Mathf.Min(PlayerPrefs.GetFloat("NSR_BestTime",99999f),Elapsed));PlayerPrefs.Save();}
 }
}