using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class VisualPulse : MonoBehaviour
    {
        private float speed = 2f;
        private float strength = .3f;
        private Renderer target;
        private Color baseEmission;

        public void Configure(float pulseSpeed, float pulseStrength) { speed = pulseSpeed; strength = pulseStrength; }
        private void Start()
        {
            target = GetComponent<Renderer>();
            if (target != null && target.material.HasProperty("_EmissionColor")) baseEmission = target.material.GetColor("_EmissionColor");
        }
        private void Update()
        {
            if (target == null || !target.material.HasProperty("_EmissionColor")) return;
            var wave = 1f + Mathf.Sin(Time.time * speed) * strength;
            target.material.SetColor("_EmissionColor", baseEmission * wave);
        }
    }
}
