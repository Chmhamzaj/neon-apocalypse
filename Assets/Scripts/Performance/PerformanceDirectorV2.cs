using UnityEngine;
namespace NeonApocalypse.Performance
{
    public class PerformanceDirectorV2 : MonoBehaviour
    {
        public int targetFps=60; public float minRenderScale=.78f,maxRenderScale=1f,response=1.0f,frameBudgetMs=16.67f;
        public float adjustEverySeconds = 1.0f;
        public int slowWindowsBeforeDrop = 2;
        public int fastWindowsBeforeRaise = 3;
        private float scale=1f,emaMs=16.67f,windowTimer;
        private int slowFrames,fastFrames,slowWindows,fastWindows;
        private void Awake(){QualitySettings.vSyncCount=0;QualitySettings.maxQueuedFrames=1;Application.targetFrameRate=targetFps;Time.fixedDeltaTime=1f/60f;}
        private void Update(){
            float ms=Time.unscaledDeltaTime*1000f; emaMs=Mathf.Lerp(emaMs,ms,0.08f); windowTimer+=Time.unscaledDeltaTime;
            if(emaMs>frameBudgetMs*1.08f) {slowFrames++; fastFrames=0;} else if(emaMs<frameBudgetMs*.82f) {fastFrames++; slowFrames=0;} else {slowFrames=Mathf.Max(0,slowFrames-1);fastFrames=Mathf.Max(0,fastFrames-1);}
            if(windowTimer>=Mathf.Max(0.5f, adjustEverySeconds)){
                if(slowFrames>=10) { slowWindows++; fastWindows=0; }
                else if(fastFrames>=16) { fastWindows++; slowWindows=0; }
                else { slowWindows=Mathf.Max(0,slowWindows-1); fastWindows=Mathf.Max(0,fastWindows-1); }

                if(slowWindows>=Mathf.Max(1, slowWindowsBeforeDrop)) {
                    scale=Mathf.MoveTowards(scale,minRenderScale,.02f);
                    slowWindows=0;
                }
                else if(fastWindows>=Mathf.Max(1, fastWindowsBeforeRaise)) {
                    scale=Mathf.MoveTowards(scale,maxRenderScale,.01f);
                    fastWindows=0;
                }
                slowFrames=0;fastFrames=0;windowTimer=0f;
                ScalableBufferManager.ResizeBuffers(scale,scale);
            }
        }
    }
}
