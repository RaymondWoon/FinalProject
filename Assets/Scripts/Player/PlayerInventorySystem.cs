using DungeonEscape.Inventory;
using UnityEngine;

public class PlayerInventorySystem : MonoBehaviour
{
    [SerializeField] private InventorySystem _inventorySystem;

    // Handler for player to pick item up and add to his inventory
    public int PickUpItem(InventoryItemData itemData, int qty)
    {
        if (!_inventorySystem.IsFull() || itemData.isStackable)
            return _inventorySystem.AddItem(itemData, qty);

        return qty;
    }
}
