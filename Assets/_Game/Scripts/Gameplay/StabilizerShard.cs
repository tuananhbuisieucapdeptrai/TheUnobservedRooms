using UnityEngine;

namespace UnobservedRooms.Gameplay
{
    public sealed class StabilizerShard : MonoBehaviour, IInteractable
    {
        [SerializeField] private int amount = 12;
        public string Prompt => $"Press E to recover +{amount} coherence";
        public float HoldDuration => 0f;
        public bool CanInteract(PlayerInteractor interactor) => interactor.Coherence != null;
        public void Interact(PlayerInteractor interactor)
        {
            interactor.Coherence.Restore(amount, "stabilizer_shard");
            GameAudio.Play(AudioCue.Shard,transform.position);
            Destroy(gameObject);
        }
        private void Update() { transform.Rotate(0f, 55f * Time.deltaTime, 0f); transform.position += Vector3.up * (Mathf.Sin(Time.time * 2f) * .0015f); }
    }
}
