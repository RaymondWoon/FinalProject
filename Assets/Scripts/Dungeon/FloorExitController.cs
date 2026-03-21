using UnityEngine;

public class FloorExitController : MonoBehaviour
{
    private PlayerController _playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerController = other.GetComponent<PlayerController>();

            if (_playerController.PlayerFloor == 1)
            {
                _playerController.PlayerFloor -= 1;

                GameManager.Instance.UpdateGameState(GameManager.GameState.GameWon);
            }
        }
    }
}
