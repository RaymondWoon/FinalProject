using UnityEngine;

public class FloorEntryController : MonoBehaviour
{

    [SerializeField] private GameObject _door;

    private PlayerController _playerController;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // close the portal
            _door.SetActive(true);

            // get the PlayerController component
            _playerController = other.GetComponent<PlayerController>();

            // Player is not in the stairwell
            _playerController.IsInStairway = false;

            // Update the current player floor
            _playerController.PlayerFloor -= 1;
        }
    }

}
