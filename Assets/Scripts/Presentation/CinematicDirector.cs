using UnityEngine;
using System.Collections;
namespace NeonApocalypse.Presentation
{
    public class CinematicDirector : MonoBehaviour
    {
        public Camera gameplayCamera; public Transform player; public Canvas overlay; public float introDuration=9f; private bool played;
        private void Start(){ if(!played&&gameplayCamera&&player) StartCoroutine(PlayIntro()); }
        private IEnumerator PlayIntro(){ played=true; if(overlay) overlay.gameObject.SetActive(false); Vector3 gameplayPos=gameplayCamera.transform.position; Quaternion gameplayRot=gameplayCamera.transform.rotation; Vector3 start=player.position+new Vector3(-15f,11f,-17f); Vector3 end=player.position+new Vector3(8f,7f,-10f); float t=0f; while(t<introDuration){t+=Time.deltaTime; float u=Mathf.Clamp01(t/introDuration); u=u*u*(3f-2f*u); gameplayCamera.transform.position=Vector3.Lerp(start,end,u); Vector3 look=Vector3.Lerp(player.position+Vector3.up*2f,player.position+player.forward*8f+Vector3.up*1.8f,u); gameplayCamera.transform.rotation=Quaternion.Slerp(gameplayCamera.transform.rotation,Quaternion.LookRotation(look-gameplayCamera.transform.position),0.18f); yield return null;} gameplayCamera.transform.position=gameplayPos; gameplayCamera.transform.rotation=gameplayRot; if(overlay) overlay.gameObject.SetActive(true); }
    }
}
