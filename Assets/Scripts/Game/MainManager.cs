using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;

    // global values
    public int DungeonWidth;
    public int DungeonDepth;
    public int DungeonFloor;
    public int DungeonScale;

    // contraints
    public int MinDungeonWidth;
    public int MaxDungeonWidth;
    public int MinDungeonDepth;
    public int MaxDungeonDepth;
    public int MinDungeonFloor;
    public int MaxDungeonFloor;

    private void Awake()
    {
        // Destroy any existing MainManager
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize the user defined dungeon dimensions
        InitDungeonWidth(DungeonWidth);
        InitDungeonDepth(DungeonDepth);
        InitDungeonFloor(DungeonFloor);
    }

    public void InitDungeonWidth(int width)
    {
        // Update the DungeonWidth
        Instance.DungeonWidth = width;
    }

    public void InitDungeonDepth(int depth)
    {
        // Update the DungeonDepth
        Instance.DungeonDepth = depth;
    }

    public void InitDungeonFloor(int floor)
    {
        // Update the DungeonFloor
        Instance.DungeonFloor = floor;
    }
}
