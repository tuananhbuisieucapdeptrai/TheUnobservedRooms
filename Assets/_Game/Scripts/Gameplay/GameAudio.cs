using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public enum AudioCue { Footstep, GateOpen, Shard, SwitchA, SwitchB, Exit, Collapse, Flashlight, Threat }

    public sealed class GameAudio : MonoBehaviour
    {
        private const int Rate = 22050;
        private static GameAudio instance;
        private readonly Dictionary<AudioCue,AudioClip> clips = new();
        private AudioSource ambience;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(this); return; }
            instance=this; BuildLibrary();
            ambience=gameObject.AddComponent<AudioSource>(); ambience.clip=BuildAmbience(); ambience.loop=true; ambience.volume=.085f; ambience.spatialBlend=0f;
            var filter=gameObject.AddComponent<AudioLowPassFilter>(); filter.cutoffFrequency=920f; filter.lowpassResonanceQ=.8f; ambience.Play();
        }

        public static void Play(AudioCue cue, Vector3 position, float volume=1f)
        {
            if (instance == null || !instance.clips.TryGetValue(cue,out var clip)) return;
            var source=new GameObject("SFX_"+cue).AddComponent<AudioSource>(); source.transform.position=position; source.clip=clip;
            source.volume=volume * .42f; source.spatialBlend=cue == AudioCue.Collapse || cue == AudioCue.Exit ? 0f : .72f;
            source.minDistance=2f; source.maxDistance=22f; source.pitch=UnityEngine.Random.Range(.96f,1.04f); source.Play(); Destroy(source.gameObject,clip.length+.2f);
        }

        private void BuildLibrary()
        {
            clips[AudioCue.Footstep]=Clip("ConcreteFootstep",.24f,(t,rng)=> Noise(rng)*Mathf.Exp(-t*18f)*.32f + Mathf.Sin(t*90f)*Mathf.Exp(-t*25f)*.18f);
            clips[AudioCue.GateOpen]=Clip("QuantumObservation",1.05f,(t,rng)=>
                Mathf.Sin(t*Mathf.PI*2f*Mathf.Lerp(85f,620f,t))*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/1.05f))*.24f + Noise(rng)*Mathf.Exp(-Mathf.Pow((t-.72f)*8f,2f))*.13f);
            clips[AudioCue.Shard]=Clip("CoherenceChime",.85f,(t,rng)=> Envelope(t,.85f)*(.2f*Mathf.Sin(t*Mathf.PI*2f*523.25f)+.14f*Mathf.Sin(t*Mathf.PI*2f*783.99f)+.08f*Mathf.Sin(t*Mathf.PI*2f*1046.5f)));
            clips[AudioCue.SwitchA]=Clip("EntangleA",.5f,(t,rng)=>Envelope(t,.5f)*(.24f*Mathf.Sin(t*Mathf.PI*2f*220f)+.12f*Mathf.Sin(t*Mathf.PI*2f*330f)));
            clips[AudioCue.SwitchB]=Clip("EntangleB",.5f,(t,rng)=>Envelope(t,.5f)*(.24f*Mathf.Sin(t*Mathf.PI*2f*277.18f)+.12f*Mathf.Sin(t*Mathf.PI*2f*415.3f)));
            clips[AudioCue.Exit]=Clip("RouteAccepted",1.8f,(t,rng)=>Envelope(t,1.8f)*(.16f*Mathf.Sin(t*Mathf.PI*2f*130.81f)+.14f*Mathf.Sin(t*Mathf.PI*2f*196f)+.12f*Mathf.Sin(t*Mathf.PI*2f*261.63f)));
            clips[AudioCue.Collapse]=Clip("CoherenceCollapse",1.4f,(t,rng)=>Mathf.Exp(-t*2.6f)*(Mathf.Sin(t*Mathf.PI*2f*Mathf.Lerp(72f,24f,t/1.4f))*.3f+Noise(rng)*.14f));
            clips[AudioCue.Flashlight]=Clip("FlashlightRelay",.12f,(t,rng)=>Noise(rng)*Mathf.Exp(-t*42f)*.2f+Mathf.Sin(t*Mathf.PI*2f*160f)*Mathf.Exp(-t*30f)*.13f);
            clips[AudioCue.Threat]=Clip("SurveyorAwakens",2.2f,(t,rng)=>Mathf.Sin(t*Mathf.PI*2f*(48f+Mathf.Sin(t*7f)*5f))*Mathf.Clamp01(t/.3f)*Mathf.Clamp01((2.2f-t)/.7f)*.28f+Noise(rng)*.035f);
        }

        private static AudioClip BuildAmbience()
        {
            const float seconds=8f; var rng=new System.Random(7919); return Clip("FacilityAmbience",seconds,(t,_)=>
                Mathf.Sin(t*Mathf.PI*2f*43f)*.025f + Mathf.Sin(t*Mathf.PI*2f*59f)*.018f + Noise(rng)*.012f +
                Mathf.Sin(t*Mathf.PI*2f*.13f)*Mathf.Sin(t*Mathf.PI*2f*91f)*.012f);
        }

        private static AudioClip Clip(string name,float seconds,Func<float,System.Random,float> generator)
        {
            var count=Mathf.CeilToInt(seconds*Rate); var data=new float[count]; var rng=new System.Random(name.GetHashCode());
            for (var i=0;i<count;i++) data[i]=Mathf.Clamp(generator((float)i/Rate,rng),-.95f,.95f);
            var clip=AudioClip.Create(name,count,1,Rate,false); clip.SetData(data,0); return clip;
        }
        private static float Noise(System.Random rng)=>(float)(rng.NextDouble()*2.0-1.0);
        private static float Envelope(float t,float length)=>Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/length))*Mathf.Exp(-t*.8f);
    }
}
