using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class QuantumSwitch : MonoBehaviour, IInteractable
    {
        [SerializeField] private char channel = 'A';
        private EntanglementSystem system;
        private Renderer indicator;
        private TextMesh readout;
        public string Prompt
        {
            get
            {
                var calibrated = system != null && (channel == 'A' ? system.CalibratedA : system.CalibratedB);
                return calibrated ? $"Press E to retune control {channel}" : $"Press E to calibrate control {channel}";
            }
        }
        public float HoldDuration => 0f;

        public void Configure(char value, EntanglementSystem entanglement)
        {
            channel = value; system = entanglement; indicator = GetComponentInChildren<Renderer>();
            var label = new GameObject("StateReadout"); label.transform.SetParent(transform, false);
            label.transform.localPosition = new Vector3(0, 1.15f, 0); label.transform.localRotation = Quaternion.Euler(0, 180, 0);
            readout = label.AddComponent<TextMesh>(); readout.anchor = TextAnchor.MiddleCenter; readout.alignment = TextAlignment.Center;
            readout.fontSize = 72; readout.characterSize = .08f; readout.color = Color.white;
            system.Changed += Refresh; Refresh();
        }
        public bool CanInteract(PlayerInteractor interactor) => system != null;
        public void Interact(PlayerInteractor interactor)
        {
            system.Toggle(channel);
            GameAudio.Play(channel == 'A' ? AudioCue.SwitchA : AudioCue.SwitchB,transform.position);
        }
        private void OnDestroy() { if (system != null) system.Changed -= Refresh; }
        private void Refresh()
        {
            if (indicator == null || system == null) return;
            var on = channel == 'A' ? system.A : system.B;
            indicator.material.color = on == 1 ? (channel == 'A' ? Color.cyan : Color.magenta) : Color.gray;
            var calibrated = channel == 'A' ? system.CalibratedA : system.CalibratedB;
            if (readout != null) readout.text = $"{channel}\n{on}\n{(calibrated ? "LOCKED" : "UNCALIBRATED")}";
        }
    }
}
