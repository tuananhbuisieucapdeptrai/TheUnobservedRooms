using System;
using UnobservedRooms.Data;
using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class EntanglementSystem : MonoBehaviour
    {
        public int A { get; private set; }
        public int B { get; private set; }
        public int TargetA { get; private set; }
        public int TargetB { get; private set; }
        public bool CalibratedA { get; private set; }
        public bool CalibratedB { get; private set; }
        public bool BothCalibrated => CalibratedA && CalibratedB;
        public bool Solved => BothCalibrated && A == TargetA && B == TargetB;
        public event Action Changed;

        public void Configure(EntanglementDefinition definition)
        {
            if (definition?.Pairs == null || definition.Pairs.Count == 0) return;
            var pair = definition.Pairs[0]; A = pair.InitialA; B = pair.InitialB; CalibratedA = false; CalibratedB = false;
            if (definition.TargetPattern != null && definition.TargetPattern.Count > 0)
            { TargetA = definition.TargetPattern[0].A; TargetB = definition.TargetPattern[0].B; }
            Changed?.Invoke();
        }

        public void Toggle(char channel)
        {
            if (channel == 'A') { A = 1 - A; B = 1 - A; CalibratedA = true; }
            else { B = 1 - B; A = 1 - B; CalibratedB = true; }
            Changed?.Invoke();
        }
    }
}
