
namespace DungeonEscape.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public InventoryItemData _itemData;
        public int _qty;

        // Constructor
        public InventorySlot(InventoryItemData itemData, int qty)
        {
            _itemData = itemData;
            _qty = qty;
        }

        // Check if a slot is empty
        public bool IsEmpty()
        {
            return _itemData == null || _qty == 0;
        }

        // Remove all items from a slot
        public void ClearSlot()
        {
            _itemData = null;
            _qty = 0;
        }

        // Add new item to a slot
        public void SetItem(InventoryItemData newItemData, int newQty)
        {
            _itemData = newItemData;
            _qty = newQty;
        }

    }
}