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
    [SerializeField] private Text _arrowText;
    [SerializeField] private Text _healthText;
    [SerializeField] private GameObject[] _keyContainer;
    [SerializeField] private InventorySystem _inventorySystem;

    [Header("Menu")]
    [SerializeField] private GameObject _pause_ui;
    [SerializeField] private GameObject _gameOver_ui;

    public static GameManager Instance { get; private set; }

    private bool _gameOver;
    private int _totalKeys;
    private int _playerFloor;

    private GameObject _player;
    private PlayerController _playerController;
    private PlayerHealth _playerHealth;

    public int PlayerFloor
    {
        get { return _playerFloor; }
    }

    public enum GameState
    {
        GamePlay,
        Pause,
        GameOver
    }

    private GameState _gameState;

    public GameObject[] KeyContainer
    { get { return _keyContainer; } }

    public GameState CurrentGameState
    {
        set { _gameState = value; }
    }

    public void UpdateGameState(GameState _state)
    {
        SwitchGameState(_state);
    }

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

        _playerHealth = _player.GetComponent<PlayerHealth>();

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

        UpdateArrowText();

        UpdateHealthText();

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

                foreach (AudioSource audio in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                    audio.Stop();

                _gameOver_ui.SetActive(true);
                break;
        }

        // In WebGL, keep the cursor visible and free
//#if UNITY_WEBGL
//        Cursor.lockState = CursorLockMode.None;
//        Cursor.visible = true;
//#endif

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

    private void UpdateArrowText()
    {
        int arrowQty = 0;

        foreach (InventorySlot slot in _inventorySystem.slots)
        {
            if (slot._itemData.itemName == "Arrow")
            {
                arrowQty = slot._qty;
            }
        }

        _arrowText.text = "Arrows: " + arrowQty.ToString();
    }

    private void UpdateHealthText()
    {
        _healthText.text = "Health: " + _playerHealth.PlayerCurrentHealth;
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
