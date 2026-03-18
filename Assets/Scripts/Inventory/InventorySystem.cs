using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonEscape.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        //[System.Serializable]
        //public struct InventoryModule
        //{
        //    public InventoryItemData itemData;
        //    public int qty;
        //}
        //public InventoryModule _initialArrows;

        [SerializeField] private int _maxSlots;

        public List<InventorySlot> slots = new List<InventorySlot>();

        private void Start()
        {
            //AddItem(_initialArrows.itemData, _initialArrows.qty);
        }

        public int AddItem(InventoryItemData itemData, int qty)
        {
            // if qty = 0, nothing to add
            if (qty == 0)
                return 0;

            int remItems = qty;

            // First, if item is stackable, check if there are partially full slots
            if (itemData.isStackable)
            {
                foreach (InventorySlot slot in slots)
                {
                    // check for matching slots
                    if (slot._itemData == itemData)
                    {
                        // check available space in slot
                        int spaceInSlot = itemData.maxStackSize - slot._qty;

                        if (spaceInSlot > 0)
                        {
                            // number of items that is possible to add
                            int itemsToAdd = Mathf.Min(remItems, spaceInSlot);

                            // update quantity of items in the slot
                            slot._qty += itemsToAdd;

                            // update the remaining items still to be added
                            remItems -= itemsToAdd;

                            // Nothing else to add
                            if (remItems <= 0)
                                return 0;
                        }
                    }
                }
            }

            // Items remain to be added and slots are available
            while (remItems > 0 && slots.Count < _maxSlots)
            {
                // item to add is eithere 1 or the maximum allowable stack size for the item
                int itemsToAdd = Mathf.Min(remItems, itemData.isStackable ? itemData.maxStackSize : 1);

                // Add the item(s) to the next available slot
                slots.Add(new InventorySlot(itemData, itemsToAdd));

                // Update the remaining items to be added
                remItems -= itemsToAdd;
            }

            // return the remaining items
            return remItems;
        }

        public int RemoveItem(InventoryItemData itemData, int qty, bool removePartial = true)
        {
            int remItems = qty;
            
            List<InventorySlot> slotsWithItem = slots.Where(s => s._itemData == itemData).ToList();

            int totalAvailableItems = slotsWithItem.Sum(s => s._qty);

            if (totalAvailableItems < remItems && !removePartial)
                return qty;

            foreach (var slot in slotsWithItem)
            {
                if (remItems <= 0)
                    break;

                if (slot._qty <= remItems)
                {
                    remItems -= slot._qty;
                    slot.ClearSlot();
                    slots.Remove(slot);
                }
                else
                {
                    slot._qty -= remItems;
                    remItems = 0;
                }
            }

            return remItems;
        }

        public int TotalItem(InventoryItemData itemData)
        {
            List<InventorySlot> slotsWithItem = slots.Where(s => s._itemData == itemData).ToList();

            int totalAvailableItems = slotsWithItem.Sum(s => s._qty);

            return totalAvailableItems;
        }

        public int TotalItemType(InventoryItemType type, string itemName)
        {
            List<InventorySlot> slotsWithItem = slots.Where(s => s._itemData.itemType == type 
                && s._itemData.itemName == itemName).ToList();

            if (slotsWithItem.Count == 0)
                return 0;

            int totalAvailableItems = slotsWithItem.Count(s => s._itemData.itemType == type);

            return totalAvailableItems;
        }



        public bool IsFull()
        {
            return slots.Count >= _maxSlots;
        }

        public void ClearInventory()
        {
            slots.Clear();
        }


    }
}