using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonManager : MonoBehaviour
{
    [Header("** Dungeon Parameters **")]
    [SerializeField] private int _dungeonWidth = 29;
    [SerializeField] private int _dungeonDepth = 29;
    [SerializeField] private int _dungeonFloor = 1;
    [SerializeField] private int _dungeonFloorHeight = 9;
    [SerializeField] private int _minRoomSize = 3;
    [SerializeField] private int _maxRoomSize = 5;
    [SerializeField] private int _scale = 6;
    [SerializeField] private DungeonGenerator[] _dGens;

    [Header("*** GameObjects ***")]
    [SerializeField] private PlayerController _playerController;

    private Scene activeScene;

    private void Awake()
    {
        // Get the active scene
        activeScene = SceneManager.GetActiveScene();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Update dungeon parameters from the player specified values
        if (MainManager.Instance != null)
        {
            _dungeonWidth = MainManager.Instance.DungeonWidth;
            _dungeonDepth = MainManager.Instance.DungeonDepth;
            _dungeonFloor = MainManager.Instance.DungeonFloor;
            _scale = MainManager.Instance.DungeonScale;
        }
        
        for (int i = 0; i < _dungeonFloor; i++)
        {
            _dGens[i].gameObject.SetActive(true);

            _dGens[i].DungeonWidth = _dungeonWidth;
            _dGens[i].DungeonDepth = _dungeonDepth;
            // Floors begin at 1
            _dGens[i].DungeonFloor = i + 1;
            _dGens[i].DungeonFloorHeight = _dungeonFloorHeight;
            _dGens[i].MinRoomSize = _minRoomSize;
            _dGens[i].MaxRoomSize = _maxRoomSize;
            _dGens[i].DungeonScale = _scale;
            _dGens[i].AddFloorEntrance = i != _dungeonFloor - 1;

            _dGens[i].BuildDungeon();
        }

        // If not GameScene, end process
        if (activeScene.name != "GameScene")
            return;

        // Set the Player starting floor - the last floor
        _playerController.PlayerFloor = _dungeonFloor;

        // Store each dungeon floor offsets
        Vector2Int[] offsets = new Vector2Int[_dungeonFloor];

        for (int i = 0; i < _dungeonFloor; i++)
        {
            offsets[i] = new Vector2Int(_dGens[i].XOffset, _dGens[i].ZOffset);
        }

        Vector2Int totalOffset = new Vector2Int(0, 0);

        for (int i = _dungeonFloor - 1; i >= 0; i--)
        {
            if (i == _dungeonFloor - 1)
            {
                // update the total
                totalOffset += offsets[i];

                // Zero the offset and do not move the last floor
                _dGens[i].XOffset = 0;
                _dGens[i].ZOffset = 0;
                continue;
            }
            else
            {
                // Update _dGen[i] offsets
                // required to calculate the player's position on the map
                _dGens[i].XOffset = totalOffset.x;
                _dGens[i].ZOffset = totalOffset.y;

                // Move by the accumulative offset
                _dGens[i].gameObject.transform.Translate(
                    totalOffset.x * _scale,
                    0f,
                    totalOffset.y * _scale
                );

                // Update the total offset
                totalOffset += offsets[i];
            }
        }
    }
}
