using UnityEngine;

public class PortalExitController : MonoBehaviour
{
    [SerializeField] private GameObject _door;

    private PlayerController _playerController;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerController = other.GetComponent<PlayerController>();
            _playerController.TurnActiveMapOff();

            // Prevent player from activating the map
            _playerController.IsInStairway = true;

            _door.SetActive(true);

            _playerController.IsFirstPerson = false;
        }
    }
}
