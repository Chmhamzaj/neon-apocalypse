using UnityEngine; using UnityEngine.UI;
namespace NeonApocalypse.Presentation
{
    public class CinematicSubtitle:MonoBehaviour
    { public Text subtitle; public float holdSeconds=4f; float expires; public void Show(string speaker,string line){if(subtitle)subtitle.text=string.IsNullOrEmpty(speaker)?line:speaker+"  //  "+line; expires=Time.time+holdSeconds;} void Update(){if(subtitle&&Time.time>expires)subtitle.text=string.Empty;} }
}
