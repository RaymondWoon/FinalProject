using UnityEngine;


namespace DungeonEscape.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemData", menuName = "Inventory/ItemData")]
    public class InventoryItemData : ScriptableObject
    {
        public string itemName;
        public string itemId;
        public Sprite icon;
        public InventoryItemType itemType;
        public bool isStackable;
        public int maxStackSize;
        public int maxQty;
        public GameObject prefab;
    }
}