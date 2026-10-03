using UnityEngine; using UnityEngine.SceneManagement;
namespace NitroStreetRush.Racing {
 public sealed class GameState : MonoBehaviour {
  public static GameState I{get;private set;} public enum State{Countdown,Racing,Finished,Paused}
  public State Current{get;private set;}=State.Countdown; float timer;
  void Awake(){if(I&&I!=this){Destroy(gameObject);return;}I=this;DontDestroyOnLoad(gameObject);}
  void Update(){if(Current==State.Countdown){timer+=Time.deltaTime;if(timer>=3f)Current=State.Racing;} if(Input.GetKeyDown(KeyCode.Escape)) TogglePause();}
  public void Finish(){Current=State.Finished;} public void TogglePause(){Current=Current==State.Paused?State.Racing:State.Paused;Time.timeScale=Current==State.Paused?0:1;}
 }
}