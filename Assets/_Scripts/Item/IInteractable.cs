public interface IInteractable
{
    string GetInteractName();  // Tên hiển thị trên UI (Chest, Gold, Stone...)
    void Interact();       // Hành động khi nhấn F
}
