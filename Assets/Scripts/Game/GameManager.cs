using DungeonEscape.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text _floorText;
    [SerializeField] private Text _keyText;
    [SerializeField] private GameObject[] _keyContainer;
    [SerializeField] private InventorySystem _inventorySystem;

    [Header("Menu")]
    [SerializeField] private GameObject _pause_ui;

    public static GameManager Instance { get; private set; }

    private bool _gameOver;
    private int _totalKeys;
    private int _playerFloor;

    private GameObject _player;
    private PlayerController _playerController;

    private enum GameState
    {
        GamePlay,
        Pause,
        GameOver
    }

    private GameState _gameState;

    public GameObject[] KeyContainer
    { get { return _keyContainer; } }

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindWithTag("Player");

        _playerController = _player.GetComponent<PlayerController>();

        // Initialize variables
        _gameOver = false;

        // Default to GamePlay
        SwitchGameState(GameState.GamePlay);
    }

    // Update is called once per frame
    void Update()
    {
        // stop update if game is over
        if (_gameOver || Instance == null)
            return;

        if (_keyText.gameObject == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePauseUI();

        _playerFloor = _playerController.PlayerFloor;

        UpdateFloorText();

        if (_playerController.IsInStairway)
        {
            _keyText.gameObject.SetActive(false);
        }
        else
        {
            _keyText.gameObject.SetActive(true);
            UpdateKeyText();
        }
    }

    private void SwitchGameState(GameState state)
    {
        _pause_ui.SetActive(false);

        switch (state)
        {
            case GameState.GamePlay:
                _player.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.Pause:
                Time.timeScale = 0.0f;
                Cursor.lockState = CursorLockMode.None;
                _player.SetActive(false);
                _pause_ui.SetActive(true);
                break;

            case GameState.GameOver:
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _player.SetActive(false);
                break;
        }

        // In WebGL, keep the cursor visible and free
#if UNITY_WEBGL
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
#endif

        _gameState = state;
    }

    private void UpdateKeyText()
    {
        // Player has won
        if (_playerFloor == 0)
            return;

        int keysFound = 0;

        _totalKeys = _keyContainer[_playerFloor - 1].transform.childCount;

        foreach (InventorySlot slot in _inventorySystem.slots)
        {
            if (slot._itemData.itemType == InventoryItemType.Key)
            {
                keysFound = slot._qty;
            }
        }

        _keyText.text = "Keys: " + keysFound.ToString() + " / " + (_totalKeys + keysFound).ToString();
    }

    private void UpdateFloorText()
    {
        _floorText.text = "Floor: " + _playerFloor.ToString();
    }

    public void TogglePauseUI()
    {
        if (_gameState == GameState.GamePlay)
        {
            SwitchGameState(GameState.Pause);
        }
        else if (_gameState == GameState.Pause)
        {
            SwitchGameState(GameState.GamePlay);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        _gameState = GameState.GameOver;

        // unlock the cursor
        Cursor.lockState = CursorLockMode.None;

        SceneManager.LoadScene("MenuScene");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
