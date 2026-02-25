using UnityEngine;

namespace DungeonEscape.Inventory
{
    public class InventoryItemPickup : MonoBehaviour
    {
        [SerializeField] private InventoryItemData itemData;
        [SerializeField] private int quantity = 1;
        [SerializeField] private AudioClip _pickupSFX;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //Debug.Log("Player collided");

                // Get the player's InventorySystem component
                PlayerInventorySystem playerInventory = other.GetComponent<PlayerInventorySystem>();

                // PlayerInventory found
                if (playerInventory != null)
                {
                    // Add as many items possible, returning the balance
                    quantity = playerInventory.PickUpItem(itemData, quantity);

                    // play soundeffect
                    other.GetComponent<AudioSource>().PlayOneShot(_pickupSFX);

                    // All items added. Remove from GameScene
                    if (quantity <= 0)
                        Destroy(gameObject);
                }
            }
        }
    }
}