using UnityEngine;
using UnityEngine.UI;
using DungeonEscape.Inventory;

public class GameManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private Text _keyText;
    [SerializeField] private GameObject _keyContainer;
    [SerializeField] private InventorySystem _inventorySystem;

    public static GameManager instance;

    private bool _gameOver;
    private int _totalKeys;

    private GameObject _player;

    private enum GameState
    {
        GamePlay,
        Pause,
        GameOver
    }

    private GameState _gameState;

    private void Awake()
    {
        // Initialize variables
        instance = this;
        _gameOver = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // stop update if game is over
        if (_gameOver)
            return;

        UpdateKeyText();
    }

    private void SwitchGameState(GameState state)
    {
        switch (state)
        {
            case GameState.GamePlay:
                _player.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
        }

        _gameState = state;
    }

    private void UpdateKeyText()
    {
        int keysFound = 0;

        _totalKeys = _keyContainer.transform.childCount;

        Debug.Log(_totalKeys);

        foreach (InventorySlot slot in _inventorySystem.slots)
        {
            if (slot._itemData.itemType == InventoryItemType.Key)
            {
                keysFound = slot._qty;
            }
        }

        _keyText.text = "Keys: " + keysFound.ToString() + " / " + (_totalKeys + keysFound).ToString();
    }
}
