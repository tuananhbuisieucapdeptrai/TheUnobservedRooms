namespace UnobservedRooms.Gameplay
{
    public interface IInteractable
    {
        string Prompt { get; }
        float HoldDuration { get; }
        bool CanInteract(PlayerInteractor interactor);
        void Interact(PlayerInteractor interactor);
    }
}
