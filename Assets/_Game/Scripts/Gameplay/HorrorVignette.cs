using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class HorrorVignette : MonoBehaviour
    {
        private Texture2D texture;
        private SurveyorAgent surveyor;
        private float nextSearch;
        private void Awake()
        {
            const int size = 128; texture = new Texture2D(size,size,TextureFormat.RGBA32,false) { name = "RuntimeHorrorVignette" };
            var pixels = new Color[size*size];
            for (var y=0;y<size;y++) for (var x=0;x<size;x++)
            {
                var nx=(x/(float)(size-1)-.5f)*2f; var ny=(y/(float)(size-1)-.5f)*2f;
                var edge=Mathf.SmoothStep(.42f,1.15f,Mathf.Sqrt(nx*nx+ny*ny)); pixels[y*size+x]=new Color(.015f,.025f,.03f,edge);
            }
            texture.SetPixels(pixels); texture.Apply();
        }
        private void OnGUI()
        {
            var oldDepth=GUI.depth; GUI.depth=1000;
            if (Time.time > nextSearch) { nextSearch=Time.time+1f; surveyor=FindAnyObjectByType<SurveyorAgent>(); }
            var threat=surveyor == null ? 0f : 1f-Mathf.Clamp01(Vector3.Distance(transform.position,surveyor.transform.position)/18f);
            var old=GUI.color; GUI.color=new Color(1f,.82f,.76f,.3f+threat*.48f+Mathf.Sin(Time.time*5f)*threat*.05f);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),texture,ScaleMode.StretchToFill,true); GUI.color=old; GUI.depth=oldDepth;
        }
    }
}
