using UnityEngine;

public class FloorEntryController : MonoBehaviour
{

    [SerializeField] private GameObject _door;

    private PlayerController _playerController;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _door.SetActive(true);

            _playerController = other.GetComponent<PlayerController>();

            _playerController.IsInStairway = false;
            _playerController.PlayerFloor -= 1;
        }
    }

}
