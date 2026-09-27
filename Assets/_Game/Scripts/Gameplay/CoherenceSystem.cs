using System;
using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class CoherenceSystem : MonoBehaviour
    {
        [SerializeField, Range(1, 200)] private int maximum = 100;
        [SerializeField] private float decayDelay = 20f;
        [SerializeField] private float decayInterval = 7f;
        private float nextDecay;
        private bool decayEnabled;
        public int Maximum => maximum;
        public int Current { get; private set; }
        public event Action<int, int, string> Changed;
        public event Action Depleted;

        private void Awake() => ResetResource();

        public void ConfigureChallenge(int newMaximum)
        {
            maximum = Mathf.Clamp(newMaximum, 1, 200); Current = maximum;
            nextDecay = Time.time + decayDelay; decayEnabled = true;
        }

        private void Update()
        {
            if (!decayEnabled || Time.time < nextDecay || Current <= 0) return;
            nextDecay = Time.time + decayInterval;
            Set(Current - 1, "ambient_instability");
        }

        public void ResetResource()
        {
            var old = Current; Current = maximum;
            Changed?.Invoke(old, Current, "reset");
        }

        public bool TrySpend(int amount, string reason)
        {
            if (amount < 0 || Current < amount) return false;
            Set(Current - amount, reason);
            return true;
        }

        public void Restore(int amount, string reason)
        {
            if (amount <= 0) return;
            Set(Mathf.Min(maximum, Current + amount), reason);
        }

        private void Set(int value, string reason)
        {
            var old = Current; Current = Mathf.Clamp(value, 0, maximum);
            if (old == Current) return;
            Changed?.Invoke(old, Current, reason);
            if (Current == 0) Depleted?.Invoke();
        }
    }
}
