using UnityEngine;
using UnityEngine.InputSystem;

namespace UnobservedRooms.Gameplay
{
    public sealed class PlayerFlashlight : MonoBehaviour
    {
        private Light beam;
        private void Awake()
        {
            beam = gameObject.AddComponent<Light>(); beam.type = LightType.Spot; beam.range = 16f; beam.spotAngle = 54f;
            beam.innerSpotAngle = 30f; beam.intensity = 2.6f; beam.color = new Color(.72f,.9f,1f); beam.shadows = LightShadows.Soft;
        }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            { beam.enabled = !beam.enabled; GameAudio.Play(AudioCue.Flashlight,transform.position,.75f); }
            if (beam.enabled) beam.intensity = 2.45f + Mathf.PerlinNoise(Time.time*8f,.37f)*.3f;
        }
    }
}
