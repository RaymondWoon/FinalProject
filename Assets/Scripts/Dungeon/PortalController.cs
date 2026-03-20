using UnityEngine;

public class PortalController : MonoBehaviour
{

    [SerializeField] private GameObject _door;

    private PlayerController _playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerController = other.GetComponent<PlayerController>();

            if (GameManager.Instance != null)
            {
                int remainingkeys = GameManager.Instance.KeyContainer[_playerController.PlayerFloor - 1].transform.childCount;

                if (remainingkeys == 0)
                {
                    _door.SetActive(false);
                    _playerController.IsFirstPerson = true;
                }
                    
            }
        }
    }

}
