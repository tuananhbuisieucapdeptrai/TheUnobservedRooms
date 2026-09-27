using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class RoomAtmosphere : MonoBehaviour
    {
        private string hazard;
        private int seed;
        private Light roomLight;
        private float baseIntensity;
        public void Configure(string hazardType, int variantSeed) { hazard = hazardType; seed = variantSeed; }
        private void Start()
        {
            roomLight = GetComponentInChildren<Light>();
            if (roomLight != null) baseIntensity = roomLight.intensity;
        }
        private void Update()
        {
            if (roomLight == null) return;
            var time = Time.time + seed * .37f;
            if (hazard == "Flicker" || hazard == "Static")
            {
                var noise = Mathf.PerlinNoise(time * 5.2f, seed * .13f);
                roomLight.intensity = baseIntensity * (noise > .3f ? Mathf.Lerp(.68f,1.18f,noise) : .12f);
            }
            else roomLight.intensity = baseIntensity * (1f + Mathf.Sin(time * .8f) * .035f);
        }
    }
}
