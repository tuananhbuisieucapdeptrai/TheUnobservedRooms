using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class ObservationGate : MonoBehaviour, IInteractable
    {
        [SerializeField] private int coherenceCost = 12;
        [SerializeField] private string edgeId;
        private bool open;
        public int Cost => coherenceCost;
        public string Prompt => open ? string.Empty : $"Hold E to observe threshold  -{coherenceCost} coherence";
        public float HoldDuration => .8f;

        public void Configure(string id, int cost)
        { edgeId = id; coherenceCost = cost <= 0 ? 0 : Mathf.CeilToInt(cost * 1.5f); }

        public bool CanInteract(PlayerInteractor interactor) => !open && interactor.Coherence != null;

        public void Interact(PlayerInteractor interactor)
        {
            if (open || !interactor.Coherence.TrySpend(coherenceCost, "observe:" + edgeId)) return;
            open = true;
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
            foreach (var renderer in GetComponentsInChildren<Renderer>()) renderer.enabled = false;
            GameAudio.Play(AudioCue.GateOpen,transform.position,.9f);
            Debug.Log($"Observed threshold '{edgeId}' for {coherenceCost} coherence.");
        }
    }
}
