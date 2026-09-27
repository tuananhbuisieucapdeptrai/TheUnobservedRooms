using UnityEngine;
using UnityEngine.InputSystem;

namespace UnobservedRooms.Gameplay
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float range = 4.25f;
        [SerializeField] private float aimAssistRadius = .16f;
        private Camera playerCamera;
        private IInteractable target;
        private float held;
        public CoherenceSystem Coherence { get; private set; }
        public string Prompt => target is ObservationGate gate && Coherence != null && Coherence.Current < gate.Cost
            ? $"INSUFFICIENT COHERENCE  Need {gate.Cost}, have {Coherence.Current}"
            : target?.Prompt ?? string.Empty;
        public float Progress => target == null || target.HoldDuration <= 0f ? 0f : Mathf.Clamp01(held / target.HoldDuration);

        public void Configure(Camera camera, CoherenceSystem coherence)
        { playerCamera = camera; Coherence = coherence; }

        private void Update()
        {
            target = FindTarget();
            if (target == null || Keyboard.current == null || !target.CanInteract(this)) { held = 0f; return; }
            if (target is ObservationGate gate && Coherence != null && Coherence.Current < gate.Cost) { held = 0f; return; }
            if (!Keyboard.current.eKey.isPressed) { held = 0f; return; }
            held += Time.deltaTime;
            if (held < target.HoldDuration) return;
            target.Interact(this); held = 0f;
        }

        private IInteractable FindTarget()
        {
            if (playerCamera == null) return null;
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (!Physics.SphereCast(ray, aimAssistRadius, out var hit, range, ~0, QueryTriggerInteraction.Collide)) return null;
            return hit.collider.GetComponentInParent<IInteractable>();
        }
    }
}
