using DungeonEscape.Inventory;
using UnityEngine;

public class PlayerInventorySystem : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryModule
    {
        public InventoryItemData itemData;
        public int qty;
    }

    public InventoryModule _bow;
    public InventoryModule _initialArrows;

    [SerializeField] private InventorySystem _inventorySystem;

    private int _currentArrows;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        PickUpItem(_bow.itemData, _bow.qty);
        PickUpItem(_initialArrows.itemData, _initialArrows.qty);

        _currentArrows = _initialArrows.qty;
    }

    // Handler for player to pick item up and add to his inventory
    public int PickUpItem(InventoryItemData itemData, int qty)
    {
        if (!_inventorySystem.IsFull() || itemData.isStackable)
            return _inventorySystem.AddItem(itemData, qty);

        return qty;
    }

    //public int RemoveItem(InventoryItemData itemData, int qty, bool removePartial = true)
    //{
    //    return _inventorySystem.RemoveItem(itemData, qty, removePartial);
    //}

    //public int TotalItem(InventoryItemData itemData)
    //{
    //    return _inventorySystem.TotalItem(itemData);
    //}

    public bool HasItem(InventoryItemData itemData)
    {
        return _inventorySystem.TotalItem(itemData) > 0;
    }

    public bool HasWeapon(InventoryItemData itemData)
    {
        return false;
    }

    public bool HasArrows()
    {
        return _inventorySystem.TotalItemType(InventoryItemType.Ammo, "Arrow") > 0;
    }

    public void RemoveArrow()
    {
        _currentArrows = _inventorySystem.RemoveItem(_initialArrows.itemData, 1);
    }

}
