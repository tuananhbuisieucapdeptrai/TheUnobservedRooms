using UnobservedRooms.Core;
using UnobservedRooms.UI;
using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class ExitAperture : MonoBehaviour, IInteractable
    {
        private EntanglementSystem entanglement;
        private GameStateMachine stateMachine;
        private Renderer surface;
        public string Prompt => entanglement != null && entanglement.Solved ? "Press E to commit route and escape" : "Exit locked  Match the entangled controls";
        public float HoldDuration => .25f;
        public void Configure(EntanglementSystem system, GameStateMachine state)
        {
            entanglement = system; stateMachine = state; surface = GetComponentInChildren<Renderer>();
            entanglement.Changed += Refresh; Refresh();
        }
        public bool CanInteract(PlayerInteractor interactor) => entanglement != null && entanglement.Solved;
        public void Interact(PlayerInteractor interactor)
        {
            if (!CanInteract(interactor)) return;
            stateMachine.TryEnter(GameFlowState.Exit);
            GameAudio.Play(AudioCue.Exit,transform.position);
            GameHud.ShowResult("ROUTE ACCEPTED\nThe facility will preserve the path you created.");
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        private void OnDestroy() { if (entanglement != null) entanglement.Changed -= Refresh; }
        private void Refresh()
        {
            if (surface == null || entanglement == null) return;
            var ready = entanglement.Solved;
            var color = ready ? new Color(.05f, .85f, 1f) : new Color(.28f, .03f, .38f);
            surface.material.color = color;
            surface.material.EnableKeyword("_EMISSION");
            surface.material.SetColor("_EmissionColor", ready ? color * 3.5f : color * .7f);
        }
    }
}
