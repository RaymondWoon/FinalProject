using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.VectorGraphics;
using UnityEngine;
//using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
//using UnityEngine.UI;
//using static UnityEngine.LightAnchor;
using Unity.AI.Navigation;

public class DungeonGenerator : MonoBehaviour
{

    #region SERIALIZE FIELDS

    //[Header("** Dungeon Parameters **")]
    //[SerializeField] private int _dungeonWidth = 29;
    //[SerializeField] private int _dungeonDepth = 29;
    //[SerializeField] private int _dungeonFloor = 1;
    //[SerializeField] private int _minRoomSize = 3;
    //[SerializeField] private int _maxRoomSize = 5;

    [Header("** AI Navigation **")]
    [SerializeField] private NavMeshSurface _navMeshSurface;

    [Header("** Dungeon Containers **")]
    [SerializeField] private GameObject _corridorContainer;
    [SerializeField] private GameObject _doorContainer;
    [SerializeField] private GameObject _roomCornerContainer;
    [SerializeField] private GameObject _roomWallContainer;
    [SerializeField] private GameObject _roomSectionContainer;
    [SerializeField] private GameObject _columnContainer;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private GameObject _treasureContainer;
    [SerializeField] private GameObject _keyContainer;

    [System.Serializable]
    public struct DungeonModule
    {
        public GameObject prefab;
        public Vector3 rotation;
    }

    [Header("** Dungeon Modules: Corridor **")]
    public DungeonModule NS_Straight;
    public DungeonModule EW_Straight;
    public DungeonModule NS_T_Junction_E;
    public DungeonModule NS_T_Junction_W;
    public DungeonModule EW_T_Junction_N;
    public DungeonModule EW_T_Junction_S;
    public DungeonModule Cross_Junction;
    public DungeonModule SW_Corner;
    public DungeonModule NW_Corner;
    public DungeonModule NE_Corner;
    public DungeonModule SE_Corner;

    [Header("** Dungeon Modules: Room **")]
    public DungeonModule Room_Section;
    public DungeonModule Room_SW_Corner;
    public DungeonModule Room_NW_Corner;
    public DungeonModule Room_NE_Corner;
    public DungeonModule Room_SE_Corner;
    public DungeonModule Room_SW_Corner_Door_SW;
    public DungeonModule Room_NW_Corner_Door_NW;
    public DungeonModule Room_NE_Corner_Door_NE;
    public DungeonModule Room_SE_Corner_Door_SE;
    public DungeonModule Room_SW_Corner_Door_S;
    public DungeonModule Room_SW_Corner_Door_W;
    public DungeonModule Room_NW_Corner_Door_W;
    public DungeonModule Room_NW_Corner_Door_N;
    public DungeonModule Room_NE_Corner_Door_N;
    public DungeonModule Room_NE_Corner_Door_E;
    public DungeonModule Room_SE_Corner_Door_E;
    public DungeonModule Room_SE_Corner_Door_S;
    public DungeonModule Room_N_Wall;
    public DungeonModule Room_E_Wall;
    public DungeonModule Room_S_Wall;
    public DungeonModule Room_W_Wall;
    public DungeonModule Room_N_Wall_Door_N;
    public DungeonModule Room_E_Wall_Door_E;
    public DungeonModule Room_S_Wall_Door_S;
    public DungeonModule Room_W_Wall_Door_W;
    public DungeonModule Room_Corner_Pillar_SW;
    public DungeonModule Room_Corner_Pillar_NW;
    public DungeonModule Room_Corner_Pillar_NE;
    public DungeonModule Room_Corner_Pillar_SE;
    public DungeonModule Floor_Exit_Portal;
    public DungeonModule Floor_Entry_Portal;

    [Header("** Dungeon Modules: Stairwell **")]
    public DungeonModule Stairwell;

    [Header("** Prototype Prefabs **")]
    [SerializeField] private GameObject _prototypeCorridor;
    [SerializeField] private GameObject _prototypeRoom;
    [SerializeField] private GameObject _prototypeWall;
    [SerializeField] private bool _isPrototype = false;

    [Header("** Game Elements **")]
    [SerializeField] private GameObject _enemy;
    [SerializeField] private GameObject _treasure;
    [SerializeField] private GameObject _key;
    [SerializeField] private GameObject[] _enemies;
    [SerializeField] private GameObject _bossEnemy;

    [Header("** Map **")]
    [SerializeField] private UnityEngine.UI.Image _mapImage;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private bool _showMap = false;

    [Header("** Debug **")]
    [SerializeField] private bool _debugOutput = false;

    #endregion

    #region DUNGEON PROPERTIES

    private int _dungeonWidth;
    private int _dungeonDepth;
    private int _dungeonFloor;
    private int _dungeonFloorHeight;
    private int _dungeonScale;
    private int _minRoomSize;
    private int _maxRoomSize;
    private int _xOffset = 0;
    private int _zOffset = 0;
    private bool _addFloorEntrance = false;

    public int DungeonWidth
    {
        //get { return _dungeonWidth; } 
        set { _dungeonWidth = value; }
    }

    public int DungeonDepth
    {
        set { _dungeonDepth = value; }
    }

    public int DungeonFloor
    {
        set { _dungeonFloor = value; }
    }

    public int DungeonFloorHeight
    {
        set { _dungeonFloorHeight = value; }
    }

    public Room DungeonRoom(int index)
    {
        return _dungeon.Rooms[index];
    }

    public int DungeonScale
    {
        get { return _dungeonScale; }
        set { _dungeonScale = value; }
    }

    public int MinRoomSize
    {
        set { _minRoomSize = value; }
    }

    public int MaxRoomSize
    {
        set { _maxRoomSize = value; }
    }

    public bool ShowMap
    {
        set { _showMap = value; }
    }

    public int XOffset
    {
        get { return _xOffset; }
        set { _xOffset = value; }
    }

    public int ZOffset
    {
        get { return _zOffset; }
        set { _zOffset = value; }
    }

    public bool AddFloorEntrance
    {
        set { _addFloorEntrance = value; }
    }

    public void ToggleMap()
    {
        _showMap = !_showMap;

        _canvas.gameObject.SetActive(_showMap);
    }
    #endregion

    #region LOCAL VARIABLES

    private GameObject _player;

    private Dungeon _dungeon;

    private List<Edge> edges;

    private List<Edge> MST;

    private Scene activeScene;

    private int numOfTries = 800;
    private System.Random rng = new();

    private Texture2D mapTexture;

    private int _floorDepth;

    private Tile.TileType _wallTile = Tile.TileType.Wall;
    private Tile.TileType _corridorTile = Tile.TileType.Corridor;
    private Tile.TileType _roomTile = Tile.TileType.Room;
    private Tile.TileType _doorExitTile = Tile.TileType.DoorExit;
    private Tile.TileType _doorEnterTile = Tile.TileType.DoorEnter;
    private Tile.TileType _floorExitTile = Tile.TileType.FloorExit;
    private Tile.TileType _floorEnterTile = Tile.TileType.FloorEnter;

    #endregion

    private void Awake()
    {

        _player = GameObject.FindGameObjectWithTag("Player");

        // Get the active scene
        activeScene = SceneManager.GetActiveScene();

        // Create DungeonMap instance
        //if (activeScene.name != "PrototypeScene")
        //    CreateMap();
    }

    // Update is called once per frame
    void Update()
    {
        //if (activeScene.name == "GameScene" && _showMap)
        //{
            // Update player position on map if visible
            //UpdateMap();
        //}

        if (activeScene.name != "PrototypeScene" && _showMap)
        {
            // Update player position on map if visible
            UpdateMap();
        }
        else
        {
            _canvas.gameObject.SetActive(_showMap);
        }
    }

    #region DUNGEON

    public void BuildDungeon()
    {
        // Override the DungeonFloorHeight for the PrototypeScene
        if (activeScene.name == "PrototypeScene")
            _dungeonFloorHeight = 0;

        // Dungeon dimensions must be odd
        if (_dungeonWidth % 2 == 0)
            _dungeonWidth++;

        if (_dungeonDepth % 2 == 0)
            _dungeonDepth++;

        // Make the width >= depth
        UpdateDungeonDimension();

        // Depth of the dungeon floor below the surface
        _floorDepth = _dungeonFloor * _dungeonFloorHeight * -1;

        // Create DungeonMap instance
        if (activeScene.name != "PrototypeScene")
            CreateMap();

        // Initialize room connectors
        edges = new List<Edge>();

        // Initialize the MST
        MST = new List<Edge>();

        GenerateDungeon();
    }

    /// <summary>
    /// Main method to access the methods to generate the dungeon
    /// </summary>
    private void GenerateDungeon()
    {
        if (_debugOutput)
            Debug.Log("**GenerateDungeon**");

        // Initialize the dungeon
        _dungeon = new Dungeon(_dungeonWidth, _dungeonDepth);

        // Step 1: Add entrance
        AddDungeonEntrance();

        // Step 2: Add dungeon rooms
        AddDungeonRooms();

        // Step 3a: Connect the rooms
        ConnectDungeonRooms();

        // Step 3b: Get the MST from the room connectors
        MST = GetMinimumSpanningTree();

        // Step 3c: Add dungeon corridors
        _dungeon.Corridors = AddDungeonCorridors();

        // Update the dungeon map for the corridors
        CarveCorridorTiles();

        // Generate Prototype/Game Level as required
        if (_isPrototype)
        {
            DrawPrototype();
        }
        else if (activeScene.name == "GameScene")
        {
            GenerateDungeonLevel();
        }

        if (activeScene.name == "GameScene")
        {
            //StartCoroutine(BuildNavMesh());
        }
        else if (activeScene.name == "PrototypeScene")
        {
            // Spawn the enemies
            SpawnEnemy();

            // Spawn treasure
            SpawnTreasure();

            // Update the player start position to the entrance room
            UpdatePlayerInitialPosition();

            // Update the map to include the player position
            if (_showMap)
                UpdateMap();
        }

        // Spawn GameObjects
        //if (activeScene.name != "TestScene")
        //{
        //if (activeScene.name == "PrototypeScene")
        //{
        // Spawn the enemies
        //SpawnEnemy();

        // Spawn treasure
        //    SpawnTreasure();
        //}

        // Spawn keys
        //if (activeScene.name == "GameScene")
        //    SpawnKey();

        // Update the player start position to the entrance room
        //UpdatePlayerInitialPosition();

        // Update the map to include the player position
        //if (_showMap)
        //    UpdateMap();
        //}

        // Print Debug Output if required
        if (_debugOutput)
            DebugOutput();
    }

    public void GenerateNavMesh()
    {
        StartCoroutine(BuildNavMesh());
    }

    /// <summary>
    /// Force Dungeon width to be greater than or equal to the depth
    /// for correct player/map orientation
    /// </summary>
    private void UpdateDungeonDimension()
    {
        int width = Mathf.Max(_dungeonWidth, _dungeonDepth);
        int depth = Mathf.Min(_dungeonWidth, _dungeonDepth);

        _dungeonWidth = width;
        _dungeonDepth = depth;
    }

    /// <summary>
    /// Add the entrance to the dungeons along the middle of the longer
    /// of the left or the bottom edges
    /// </summary>
    private void AddDungeonEntrance()
    {
        // Entrance is a 2 x 2 room located at the middle left/bottom of the longest edge

        // Initialize start point
        int startX = 0;
        int startZ = 0;

        // Define entrtance along longer axis
        if (_dungeonWidth >= _dungeonDepth)
        {
            startX = _dungeonWidth / 2 - 1;
        }
        else
        {
            startZ = _dungeonDepth / 2 - 1;
        }

        // Create the entrance room
        Room entrance = new Room(startX, startZ, 2, 2, "Entrance");

        // Add the entrance room to the collection
        _dungeon.Rooms.Add(entrance);

        // Update the dungeon grid
        for (int x = startX; x < startX + 2; x++)
        {
            for (int z = startZ; z < startZ + 2; z++)
            {
                if (_addFloorEntrance && x == startX && z == startZ)
                {
                    CarveTile(x, z, Tile.TileType.FloorEnter, true);
                }
                else
                {
                    CarveTile(x, z, Tile.TileType.Room, true);
                }
            }
        }

        // Debug output
        if (_debugOutput)
            Debug.Log("New Room added: " + entrance.ToString());
    }

    /// <summary>
    /// Randomly add rectangular rooms of varying sizes
    /// within the range of _minRoomSize and _maxRoomSize
    /// </summary>
    private void AddDungeonRooms()
    {
        for (var i = 0; i < numOfTries; i++)
        {
            // Select a random width for the room
            int width = rng.Next(_minRoomSize, _maxRoomSize + 1);
            // Select a random depth for the room
            int depth = rng.Next(_minRoomSize, _maxRoomSize + 1);

            // Select a start point such that the room is within the bounds of the dungeon
            int startX = rng.Next(1, _dungeonWidth - width - 1);
            int startZ = rng.Next(1, _dungeonDepth - depth - 1);

            if (startX + width > _dungeonWidth || startZ + depth > _dungeonDepth)
            {
                Debug.Log("Error: " + "NX: " + startX + ", NZ: " + startZ + ", width: " + width + ", " + depth);
                continue;
            }

            // Create a new room with the random generated parameters
            Room newRoom = new Room(startX, startZ, width, depth);

            // Check if the newly created room overlaps an existing room
            bool overlaps = false;

            foreach (var other in _dungeon.Rooms)
            {
                if (newRoom.Intersects(other))
                {
                    overlaps = true;
                    break;
                }
            }

            // If it does, do not add room and retry
            if (overlaps) continue;

            // Add new non-overlapping room
            _dungeon.Rooms.Add(newRoom);

            if (_debugOutput)
                Debug.Log("New Room added: " + newRoom.ToString());

            // Update the dungeon grid
            for (int x = startX; x < startX + width; x++)
            {
                for (int z = startZ; z < startZ + depth; z++)
                {
                    CarveTile(x, z, Tile.TileType.Room, true);

                }
            }
        }
    }

    /// <summary>
    /// Add an edge between each room
    /// </summary>
    private void ConnectDungeonRooms()
    {
        List<Edge> nEdges = new List<Edge>();

        for (int i = 0; i < _dungeon.Rooms.Count; i++)
        {
            for (int j = i + 1; j < _dungeon.Rooms.Count; j++)
            {
                nEdges.Add(new Edge(_dungeon.Rooms[i], _dungeon.Rooms[j]));
            }

            // Find the furthest room from the first room: the 'Entrance'
            if (i == 0)
            {
                Edge longestEdge = nEdges.OrderByDescending(e => e.distance).First();

                longestEdge.B.Tag = "Exit Room";

                // Update the "Exit Floor Tile"
                CarveTile(longestEdge.B.StartX + 1,
                    longestEdge.B.StartZ + longestEdge.B.Depth - 1,
                    _floorExitTile, true);

                // Update Dungeon Floor Offsets
                XOffset = longestEdge.B.StartX + 1 - _dungeon.Rooms[0].StartX;
                // Z location for the stairwell and SW corner of next floor entrance
                ZOffset = longestEdge.B.StartZ + longestEdge.B.Depth + 1;
            }
        }

        // Sorted in ascending order to be implied 'weight' for MST
        edges = nEdges.OrderBy(e => e.distance).ToList();
    }

    /// <summary>
    /// Determine the minimum spanning tree using Kruskal's algorithm
    /// </summary>
    /// <returns></returns>
    public List<Edge> GetMinimumSpanningTree()
    {
        var mst = new List<Edge>();

        var parent = _dungeon.Rooms.ToDictionary(r => r, r => r);

        // Root node
        Room Find(Room r)
        {
            if (parent[r] != r)
                parent[r] = Find(parent[r]);

            return parent[r];
        }

        // Update node
        void Union(Room a, Room b)
        {
            parent[Find(a)] = Find(b);
        }

        // edges collection is already sorted in increasing order
        foreach (var edge in edges)
        {
            if (Find(edge.A) != Find(edge.B))
            {
                mst.Add(edge);
                Union(edge.A, edge.B);
            }
        }

        return mst;
    }

    /// <summary>
    /// Starting with the MST edges, 
    /// Apply chance to non-MST edges and to form the dungeon corridors
    /// </summary>
    /// <returns></returns>
    public List<Edge> AddDungeonCorridors()
    {
        // Initiate new list by copying the MST
        var _dungeonCorridors = new List<Edge>(MST);

        /// Iterate over all edges
        //foreach (var edge in edges)
        //{
        //    // If it's not included in the MST, apply the chance to become a corridor
        //    if (!_dungeonCorridors.Contains(edge) && rng.NextDouble() <chanceXtraCorridor)
        //        _dungeonCorridors.Add(edge);
        //}

        return _dungeonCorridors;
    }

    /// <summary>
    /// Update the player’s initial position to the entrance of the last floor
    /// </summary>
    public void UpdatePlayerInitialPosition()
    {
        Vector3 pos = new Vector3(_dungeon.Rooms[0].CenterX * _dungeonScale, _floorDepth + 1f, _dungeon.Rooms[0].CenterZ * _dungeonScale);

        _player.transform.position = pos;
    }

    private void PlaceActors()
    {
        // Spawn the enemies
        SpawnEnemy();

        // Spawn keys
        SpawnKey();

        // Update the player start position to the entrance room
        //UpdatePlayerInitialPosition();

        // Update the map to include the player position
        if (_showMap)
            UpdateMap();

        // Print Debug Output if required
        if (_debugOutput)
            DebugOutput();
    }

    public IEnumerator BuildNavMesh()
    {
        // Wait to ensure that is instantiated tiles are in place
        yield return new WaitForEndOfFrame();

        _navMeshSurface.BuildNavMesh();

        PlaceActors();
    }

    #endregion

    #region TILE

    /// <summary>
    /// Update the tiletype from the default 'Wall' to Room, Corridon
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    /// <param name="_tileType"></param>
    /// <param name="_isVisible"></param>
    private void CarveTile(int _x, int _z, Tile.TileType _tileType, bool _isVisible = true)
    {
        _dungeon.Tiles[_x, _z].Type = _tileType;
        _dungeon.Tiles[_x, _z].IsVisible = _isVisible;
    }

    /// <summary>
    /// Update the dungeon map for the dungeon corridors
    /// </summary>
    private void CarveCorridorTiles()
    {
        foreach (var edge in _dungeon.Corridors)
        {
            // Room A Centre
            int cxa = edge.A.CenterX;
            int cza = edge.A.CenterZ;

            // Room B centre
            int cxb = edge.B.CenterX;
            int czb = edge.B.CenterZ;

            // Start from Room A
            int cx = cxa;
            int cz = cza;

            // Track previous tile properties
            Tile.TileType pTile = Tile.TileType.Wall;
            int pX = 0;
            int pZ = 0;

            if (_dungeonWidth >= _dungeonDepth)
            {
                // Vertical first
                while (cz != czb)
                {
                    if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Wall)
                    {
                        // Update previous tile to DoorExit
                        if (pTile == Tile.TileType.Room)
                            CarveTile(pX, pZ, Tile.TileType.DoorExit);

                        // Update the current tile to corridor
                        CarveTile(cx, cz, Tile.TileType.Corridor);
                    }
                    // Vertical section entering room
                    else if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Room
                        && pTile == Tile.TileType.Corridor)
                    {
                        // Update the current tile to DoorEnter
                        CarveTile(cx, cz, Tile.TileType.DoorEnter);
                    }

                    // Update the previous tile parameters
                    pX = cx;
                    pZ = cz;
                    pTile = _dungeon.Tiles[cx, cz].Type;

                    cz += Math.Sign(czb - cz);
                }

                while (cx != cxb)
                {
                    if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Wall)
                    {
                        // Update previous tile to DoorExit
                        if (pTile == Tile.TileType.Room)
                            CarveTile(pX, pZ, Tile.TileType.DoorExit);

                        // Update the current tile to corridor
                        CarveTile(cx, cz, Tile.TileType.Corridor);
                    }
                    // Horizontal section entering room
                    else if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Room
                        && pTile == Tile.TileType.Corridor)
                    {
                        // Update the current tile to DoorEnter
                        CarveTile(cx, cz, Tile.TileType.DoorEnter);
                    }

                    // Update the previous tile parameters
                    pX = cx;
                    pZ = cz;
                    pTile = _dungeon.Tiles[cx, cz].Type;

                    cx += Math.Sign(cxb - cx);
                }
            }
            // Horizontal first
            else
            {
                while (cx != cxb)
                {
                    if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Wall)
                    {
                        // Update previous tile to DoorExit
                        if (pTile == Tile.TileType.Room)
                            CarveTile(pX, pZ, Tile.TileType.DoorExit);

                        // Update the current tile to corridor
                        CarveTile(cx, cz, Tile.TileType.Corridor);
                    }
                    // Horizontal section entering room
                    else if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Room
                        && pTile == Tile.TileType.Corridor)
                    {
                        // Update the current tile to DoorEnter
                        CarveTile(cx, cz, Tile.TileType.DoorEnter);
                    }

                    // Update the previous tile parameters
                    pX = cx;
                    pZ = cz;
                    pTile = _dungeon.Tiles[cx, cz].Type;

                    cx += Math.Sign(cxb - cx);
                }

                while (cz != czb)
                {
                    if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Wall)
                    {
                        // Update previous tile to DoorExit
                        if (pTile == Tile.TileType.Room)
                            CarveTile(pX, pZ, Tile.TileType.DoorExit);

                        // Update the current tile to corridor
                        CarveTile(cx, cz, Tile.TileType.Corridor);
                    }
                    // Vertical section entering room
                    else if (_dungeon.Tiles[cx, cz].Type == Tile.TileType.Room
                        && pTile == Tile.TileType.Corridor)
                    {
                        // Update the current tile to DoorEnter
                        CarveTile(cx, cz, Tile.TileType.DoorEnter);
                    }

                    // Update the previous tile parameters
                    pX = cx;
                    pZ = cz;
                    pTile = _dungeon.Tiles[cx, cz].Type;

                    cz += Math.Sign(czb - cz);
                }
            }
        }
    }

    #endregion

    #region GAME OBJECTS

    private void GenerateDungeonLevel()
    {
        // x, y - 2D map coordinates
        for (int y = 0; y < _dungeonDepth; y++)
        {
            for (int x = 0; x < _dungeonWidth; x++)
            {
                Tile.TileType tile_W = x == 0 ? _wallTile : _dungeon.Tiles[x - 1, y].Type;
                Tile.TileType tile_E = x == _dungeonWidth - 1 ? _wallTile : _dungeon.Tiles[x + 1, y].Type;
                Tile.TileType tile_S = y == 0 ? _wallTile : _dungeon.Tiles[x, y - 1].Type;
                Tile.TileType tile_N = y == _dungeonDepth - 1 ? _wallTile : _dungeon.Tiles[x, y + 1].Type;

                switch (_dungeon.Tiles[x, y].Type)
                {
                    case Tile.TileType.Corridor:
                        // Cross-Junc
                        if ((tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Cross_Junction.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Cross_Junction.rotation);
                            go.name = "CCJ - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // NS_T-Junc_E
                        else if ((tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile))
                        {
                            GameObject go = Instantiate(NS_T_Junction_E.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(NS_T_Junction_E.rotation);
                            go.name = "CNST-E - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // NS_T-Junc_W
                        else if ((tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(NS_T_Junction_W.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(NS_T_Junction_W.rotation);
                            go.name = "CNST-W - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // EW_T-Junc_N
                        else if ((tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile)
                            && (tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile))
                        {
                            GameObject go = Instantiate(EW_T_Junction_N.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(EW_T_Junction_N.rotation);
                            go.name = "CEWT-N - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // EW_T-Junc_S
                        else if ((tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile)
                            && (tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile))
                        {
                            GameObject go = Instantiate(EW_T_Junction_S.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(EW_T_Junction_S.rotation);
                            go.name = "CEWT-S - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // SW_Corner
                        else if ((tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile))
                        {
                            GameObject go = Instantiate(SW_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(SW_Corner.rotation);
                            go.name = "CSWC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // NW_Corner
                        else if ((tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile))
                        {
                            GameObject go = Instantiate(NW_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(NW_Corner.rotation);
                            go.name = "CNWC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // NE_Corner
                        else if ((tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(NE_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(NE_Corner.rotation);
                            go.name = "CNEC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // SE_Corner
                        else if ((tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(SE_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(SE_Corner.rotation);
                            go.name = "CSEC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // NS
                        else if ((tile_N == _corridorTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_S == _corridorTile || tile_S == _doorEnterTile || tile_S == _doorExitTile))
                        {
                            GameObject go = Instantiate(NS_Straight.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(NS_Straight.rotation);
                            go.name = "CNS - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        // EW
                        else if ((tile_E == _corridorTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_W == _corridorTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(EW_Straight.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(EW_Straight.rotation);
                            go.name = "CEW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _corridorContainer.transform;
                        }
                        break;

                    case Tile.TileType.Room:
                        // SW_Corner
                        if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _wallTile || tile_S == _corridorTile)
                            && (tile_W == _wallTile || tile_W == _corridorTile))
                        {
                            GameObject go = Instantiate(Room_SW_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SW_Corner.rotation);
                            go.name = "RSWC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomCornerContainer.transform;
                        }
                        // NW_Corner
                        else if ((tile_N == _wallTile || tile_N == _corridorTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile || tile_E == _floorExitTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile || tile_S == _floorEnterTile)
                            && (tile_W == _wallTile || tile_W == _corridorTile))
                        {
                            GameObject go = Instantiate(Room_NW_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NW_Corner.rotation);
                            go.name = "RNWC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomCornerContainer.transform;
                        }
                        // NE_Corner
                        else if ((tile_N == _wallTile || tile_N == _corridorTile)
                            && (tile_E == _wallTile || tile_E == _corridorTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile || tile_W == _floorExitTile))
                        {
                            GameObject go = Instantiate(Room_NE_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NE_Corner.rotation);
                            go.name = "RNEC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomCornerContainer.transform;
                        }
                        // SE_Corner
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _wallTile || tile_E == _corridorTile)
                            && (tile_S == _wallTile || tile_S == _corridorTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile || tile_W == _floorEnterTile))
                        {
                            GameObject go = Instantiate(Room_SE_Corner.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SE_Corner.rotation);
                            go.name = "RSEC - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomCornerContainer.transform;
                        }
                        // W_Wall
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && tile_E == _roomTile
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _wallTile || tile_W == _corridorTile))
                        {
                            GameObject go = Instantiate(Room_W_Wall.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_W_Wall.rotation);
                            go.name = "RWW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomWallContainer.transform;
                        }
                        // N_Wall
                        else if ((tile_N == _wallTile || tile_N == _corridorTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile || tile_E == _floorExitTile)
                            && tile_S == _roomTile
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile || tile_W == _floorExitTile))
                        {
                            GameObject go = Instantiate(Room_N_Wall.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_N_Wall.rotation);
                            go.name = "RNW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomWallContainer.transform;
                        }
                        // E_Wall
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _wallTile || tile_E == _corridorTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && tile_W == _roomTile)
                        {
                            GameObject go = Instantiate(Room_E_Wall.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_E_Wall.rotation);
                            go.name = "REW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomWallContainer.transform;
                        }
                        // S_Wall
                        else if (tile_N == _roomTile
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _wallTile || tile_S == _corridorTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_S_Wall.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_S_Wall.rotation);
                            go.name = "RSW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomWallContainer.transform;
                        }
                        // Standard room section
                        else
                        {
                            GameObject go = Instantiate(Room_Section.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_Section.rotation);
                            go.name = "RM - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _roomSectionContainer.transform;
                        }
                        break;

                    case Tile.TileType.DoorEnter: case Tile.TileType.DoorExit:
                        //SW_Corner_Door_SW
                        if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && tile_S == _corridorTile
                            && tile_W == _corridorTile)
                        {
                            GameObject go = Instantiate(Room_SW_Corner_Door_SW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SW_Corner_Door_SW.rotation);
                            go.name = "RSWC-DSW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add SE pillar
                            go = Instantiate(Room_Corner_Pillar_SE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SE.rotation);
                            go.name = "COL-SE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;

                            // Add NW pillar
                            go = Instantiate(Room_Corner_Pillar_NW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NW.rotation);
                            go.name = "COL-NW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        //NW_Corner_Door_NW
                        else if (tile_N == _corridorTile 
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && tile_W == _corridorTile)
                        {
                            GameObject go = Instantiate(Room_NW_Corner_Door_NW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NW_Corner_Door_NW.rotation);
                            go.name = "RNWC-DNW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NE pillar
                            go = Instantiate(Room_Corner_Pillar_NE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NE.rotation);
                            go.name = "COL-NE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;

                            // Add SW pillar
                            go = Instantiate(Room_Corner_Pillar_SW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SW.rotation);
                            go.name = "COL-SW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        //NE_Corner_Door_NE
                        else if (tile_N == _corridorTile && tile_E == _corridorTile
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_NE_Corner_Door_NE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NE_Corner_Door_NE.rotation);
                            go.name = "RNEC-DNE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NW pillar
                            go = Instantiate(Room_Corner_Pillar_NW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NW.rotation);
                            go.name = "COL-NW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;

                            // Add SE pillar
                            go = Instantiate(Room_Corner_Pillar_SE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SE.rotation);
                            go.name = "COL-SE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        //SE_Corner_Door_SE
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && tile_E == _corridorTile
                            && tile_S == _corridorTile
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_SE_Corner_Door_SE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SE_Corner_Door_SE.rotation);
                            go.name = "RSEC-DSE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NE pillar
                            go = Instantiate(Room_Corner_Pillar_NE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NE.rotation);
                            go.name = "COL-NE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;

                            // Add SW pillar
                            go = Instantiate(Room_Corner_Pillar_SW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SW.rotation);
                            go.name = "COL-SW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // SW_Corner_Door_S
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && tile_S == _corridorTile
                            && (tile_W == _wallTile || tile_W == _corridorTile))
                        {
                            GameObject go = Instantiate(Room_SW_Corner_Door_S.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SW_Corner_Door_S.rotation);
                            go.name = "RSWC-DS - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add SE pillar
                            go = Instantiate(Room_Corner_Pillar_SE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SE.rotation);
                            go.name = "COL-SE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // SW_Corner_Door_W
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _wallTile || tile_S == _corridorTile)
                            && tile_W == _corridorTile)
                        {
                            GameObject go = Instantiate(Room_SW_Corner_Door_W.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SW_Corner_Door_W.rotation);
                            go.name = "RSWC-DW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NW pillar
                            go = Instantiate(Room_Corner_Pillar_NW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NW.rotation);
                            go.name = "COL-NW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // NW_Corner_Door_W
                        else if ((tile_N == _wallTile || tile_N == _corridorTile)
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile || tile_S == _floorEnterTile)
                            && tile_W == _corridorTile)
                        {
                            GameObject go = Instantiate(Room_NW_Corner_Door_W.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NW_Corner_Door_W.rotation);
                            go.name = "RNWC-DW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add SW pillar
                            go = Instantiate(Room_Corner_Pillar_SW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SW.rotation);
                            go.name = "COL-SW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // NW_Corner_Door_N
                        else if (tile_N == _corridorTile
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile || tile_S == _floorEnterTile)
                            && (tile_W == _wallTile || tile_W == _corridorTile))
                        {
                            GameObject go = Instantiate(Room_NW_Corner_Door_N.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NW_Corner_Door_N.rotation);
                            go.name = "RNWC-DN - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NE pillar
                            go = Instantiate(Room_Corner_Pillar_NE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NE.rotation);
                            go.name = "COL-NE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // NE_Corner_Door_N
                        else if (tile_N == _corridorTile
                            && (tile_E == _wallTile || tile_E == _corridorTile)
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_NE_Corner_Door_N.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NE_Corner_Door_N.rotation);
                            go.name = "RNEC-DN - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NW pillar
                            go = Instantiate(Room_Corner_Pillar_NW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NW.rotation);
                            go.name = "COL-NW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // NE_Corner_Door_E
                        else if ((tile_N == _wallTile || tile_N == _corridorTile)
                            && tile_E == _corridorTile
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_NE_Corner_Door_E.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_NE_Corner_Door_E.rotation);
                            go.name = "RNEC-DE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add SE pillar
                            go = Instantiate(Room_Corner_Pillar_SE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SE.rotation);
                            go.name = "COL-SE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // SE_Corner_Door_E
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && tile_E == _corridorTile
                            && (tile_S == _wallTile || tile_S == _corridorTile)
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_SE_Corner_Door_E.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SE_Corner_Door_E.rotation);
                            go.name = "RSEC-DE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add NE pillar
                            go = Instantiate(Room_Corner_Pillar_NE.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_NE.rotation);
                            go.name = "COL-NE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // SE_Corner_Door_S
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && (tile_E == _wallTile || tile_E == _corridorTile)
                            && tile_S == _corridorTile
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_SE_Corner_Door_S.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_SE_Corner_Door_S.rotation);
                            go.name = "RSEC-DS - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;

                            // Add SW pillar
                            go = Instantiate(Room_Corner_Pillar_SW.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale + 0.01f, _floorDepth, y * _dungeonScale + 0.01f);
                            go.transform.Rotate(Room_Corner_Pillar_SW.rotation);
                            go.name = "COL-SW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _columnContainer.transform;
                        }
                        // N_Wall_Door_N
                        else if (tile_N == _corridorTile
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && tile_S == _roomTile
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_N_Wall_Door_N.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_N_Wall_Door_N.rotation);
                            go.name = "RNW-DN - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;
                        }
                        // E_Wall_Door_E
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && tile_E == _corridorTile
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && tile_W == _roomTile)
                        {
                            GameObject go = Instantiate(Room_E_Wall_Door_E.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_E_Wall_Door_E.rotation);
                            go.name = "REW-DE - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;
                        }
                        // S_Wall_Door_S
                        else if (tile_N == _roomTile
                            && (tile_E == _roomTile || tile_E == _doorEnterTile || tile_E == _doorExitTile)
                            && tile_S == _corridorTile
                            && (tile_W == _roomTile || tile_W == _doorEnterTile || tile_W == _doorExitTile))
                        {
                            GameObject go = Instantiate(Room_S_Wall_Door_S.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_S_Wall_Door_S.rotation);
                            go.name = "RSW-DS - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;
                        }
                        // W_Wall_Door_W
                        else if ((tile_N == _roomTile || tile_N == _doorEnterTile || tile_N == _doorExitTile)
                            && tile_E == _roomTile
                            && (tile_S == _roomTile || tile_S == _doorEnterTile || tile_S == _doorExitTile)
                            && tile_W == _corridorTile)
                        {
                            GameObject go = Instantiate(Room_W_Wall_Door_W.prefab);
                            go.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                            go.transform.Rotate(Room_W_Wall_Door_W.rotation);
                            go.name = "RWW-DW - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                            go.transform.parent = _doorContainer.transform;
                        }
                        break;

                    case Tile.TileType.FloorExit:
                        // Exit door
                        GameObject floorExit = Instantiate(Floor_Exit_Portal.prefab);
                        floorExit.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                        floorExit.transform.Rotate(Floor_Exit_Portal.rotation);
                        floorExit.name = "EXIT - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                        floorExit.transform.parent = _doorContainer.transform;

                        // Stairwell
                        GameObject stairs = Instantiate(Stairwell.prefab);
                        stairs.transform.position = new Vector3(x * _dungeonScale, _floorDepth, (y + 1) * _dungeonScale);
                        stairs.transform.Rotate(Stairwell.rotation);
                        stairs.name = "STAIRS - " + (x * _dungeonScale).ToString() + " - " + ((y + 1) * _dungeonScale).ToString();
                        stairs.transform.parent = _doorContainer.transform;
                        break;

                    case Tile.TileType.FloorEnter:
                        // Enter door
                        GameObject floorEnter = Instantiate(Floor_Entry_Portal.prefab);
                        floorEnter.transform.position = new Vector3(x * _dungeonScale, _floorDepth, y * _dungeonScale);
                        floorEnter.transform.Rotate(Floor_Entry_Portal.rotation);
                        floorEnter.name = "ENTRY - " + (x * _dungeonScale).ToString() + " - " + (y * _dungeonScale).ToString();
                        floorEnter.transform.parent = _doorContainer.transform;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Spawn enemy in each room
    /// </summary>
    private void SpawnEnemy()
    {
        int roomCount = 0;

        foreach (Room room in _dungeon.Rooms)
        {
            roomCount += 1;

            if (room.Tag == "Entrance") continue;

            float enemyX, enemyZ;
            Vector3 upDirection = transform.up;
            //GameObject go = _enemies[rng.Next(_enemies.Length)];
            
            // Define the limits of the room to randomly place the key
            Vector3 pos1 = new Vector3((room.StartX + 0.75f) * _dungeonScale, 1.0f, (room.StartZ + 0.75f) * _dungeonScale);
            Vector3 pos2 = new Vector3((room.StartX + room.Width - 0.75f) * _dungeonScale, 1.0f, (room.StartZ + 0.75f) * _dungeonScale);
            Vector3 pos3 = new Vector3((room.StartX + room.Width - 0.75f) * _dungeonScale, 1.0f, (room.StartZ + room.Depth - 0.75f) * _dungeonScale);
            Vector3 pos4 = new Vector3((room.StartX + 0.75f) * _dungeonScale, 1.0f, (room.StartZ + room.Depth - 0.75f) * _dungeonScale);

            // Randomly select X & Z as a float
            enemyX = UnityEngine.Random.Range(pos1.x, pos3.x) + XOffset * _dungeonScale;
            enemyZ = UnityEngine.Random.Range(pos1.z, pos3.z) + ZOffset * _dungeonScale;

            if (room.Tag == "Exit Room")
            {
                GameObject boss = Instantiate(_bossEnemy, new Vector3(enemyX, _floorDepth, enemyZ), Quaternion.identity);
                boss.name = "BOSS " + roomCount.ToString();
                boss.transform.parent = _enemyContainer.transform;
            }
            else
            {
                GameObject go = _enemies[UnityEngine.Random.Range(0, _enemies.Length)];
                GameObject enemy = Instantiate(go, new Vector3(enemyX, _floorDepth, enemyZ), Quaternion.identity);
                enemy.name = "ENEMY " + roomCount.ToString();
                enemy.transform.parent = _enemyContainer.transform;
            }
        }
    }

    /// <summary>
    /// Spawn treasure in each room
    /// </summary>
    private void SpawnTreasure()
    {
        foreach (Room room in _dungeon.Rooms)
        {
            if (room.Tag == "Entrance") continue;

            float treasureX, treasureZ;
            Vector3 upDirection = transform.up;

            Vector3 pos1 = new Vector3((room.StartX + 0.75f) * _dungeonScale, 1.0f, (room.StartZ + 0.75f) * _dungeonScale);
            Vector3 pos2 = new Vector3((room.StartX + room.Width - 0.75f) * _dungeonScale, 1.0f, (room.StartZ + 0.75f) * _dungeonScale);
            Vector3 pos3 = new Vector3((room.StartX + room.Width - 0.75f) * _dungeonScale, 1.0f, (room.StartZ + room.Depth - 0.75f) * _dungeonScale);
            Vector3 pos4 = new Vector3((room.StartX + 0.75f) * _dungeonScale, 1.0f, (room.StartZ + room.Depth - 0.75f) * _dungeonScale);

            // Randomly select X & Z as a float
            treasureX = UnityEngine.Random.Range(pos1.x, pos3.x);
            treasureZ = UnityEngine.Random.Range(pos1.z, pos3.z);

            GameObject treasure = Instantiate(_treasure, new Vector3(treasureX, _dungeonFloorHeight, treasureZ) + upDirection * 1.0f, Quaternion.identity);
            treasure.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            treasure.transform.parent = _treasureContainer.transform;
        }
    }

    /// <summary>
    /// Spawn a key in each room
    /// </summary>
    private void SpawnKey()
    {
        int roomCount = 0;

        foreach (Room room in _dungeon.Rooms)
        {
            roomCount += 1;

            float keyX, keyZ;
            Vector3 upDirection = transform.up;

            // Define the limits of the room to randomly place the key
            Vector3 pos1 = new Vector3((room.StartX + 0.75f) * _dungeonScale, 1.0f, (room.StartZ + 0.75f) * _dungeonScale);
            Vector3 pos2 = new Vector3((room.StartX + room.Width - 0.75f) * _dungeonScale, 1.0f, (room.StartZ + 0.75f) * _dungeonScale);
            Vector3 pos3 = new Vector3((room.StartX + room.Width - 0.75f) * _dungeonScale, 1.0f, (room.StartZ + room.Depth - 0.75f) * _dungeonScale);
            Vector3 pos4 = new Vector3((room.StartX + 0.75f) * _dungeonScale, 1.0f, (room.StartZ + room.Depth - 0.75f) * _dungeonScale);

            //Vector3[] pos = { pos1, pos2, pos3, pos4 };

            // Randomly select X & Z as a float
            keyX = UnityEngine.Random.Range(pos1.x, pos3.x) + XOffset * _dungeonScale;
            keyZ = UnityEngine.Random.Range(pos1.z, pos3.z) + ZOffset * _dungeonScale;

            GameObject key = Instantiate(_key, new Vector3(keyX, _floorDepth, keyZ) + upDirection * 1.0f, Quaternion.identity);
            key.name = "KEY " + roomCount.ToString();
            key.transform.parent = _keyContainer.transform;
        }
    }

    #endregion

    #region PROTOTYPE

    /// <summary>
    /// Generate the prototype dungeon
    /// </summary>
    private void DrawPrototype()
    {
        for (int z = 0; z < _dungeonDepth; z++)
        {
            for (int x = 0; x < _dungeonWidth; x++)
            {
                Vector3 pos = new Vector3(x * _dungeonScale, 0, z * _dungeonScale);
                
                if (_dungeon.Tiles[x, z].Type == Tile.TileType.Corridor)
                {
                    GameObject corridor = Instantiate(_prototypeCorridor, pos, Quaternion.identity);
                    corridor.transform.localScale = new Vector3(_dungeonScale, 1, _dungeonScale);
                    corridor.transform.parent = _corridorContainer.transform;
                }
                else if (_dungeon.Tiles[x, z].Type == Tile.TileType.DoorEnter
                    || _dungeon.Tiles[x, z].Type == Tile.TileType.DoorExit
                    || _dungeon.Tiles[x, z].Type == Tile.TileType.FloorEnter
                    || _dungeon.Tiles[x, z].Type == Tile.TileType.FloorExit)
                {
                    GameObject room = Instantiate(_prototypeRoom, pos, Quaternion.identity);
                    room.transform.localScale = new Vector3(_dungeonScale, 1, _dungeonScale);
                    room.transform.parent = _doorContainer.transform;
                }
                else if (_dungeon.Tiles[x, z].Type == Tile.TileType.Room)
                {
                    GameObject room = Instantiate(_prototypeRoom, pos, Quaternion.identity);
                    room.transform.localScale = new Vector3(_dungeonScale, 1, _dungeonScale);
                    room.transform.parent = _roomSectionContainer.transform;
                }
                else if (_dungeon.Tiles[x, z].Type == Tile.TileType.Wall)
                {
                    GameObject wall = Instantiate(_prototypeWall, pos, Quaternion.identity);
                    wall.transform.localScale = new Vector3(_dungeonScale, 1, _dungeonScale);
                    wall.transform.parent = _roomWallContainer.transform;
                }
            }
        }
    }

    #endregion

    #region MAP

    /// <summary>
    /// Create the Map GameObject
    /// </summary>
    private void CreateMap()
    {
        GameObject mapGO = new GameObject();
        mapGO.name = "MapObject" + _dungeonFloor.ToString();

        // add a rect Transform to replace the normal Transform
        RectTransform sRect = mapGO.AddComponent<RectTransform>();

        sRect.SetParent(_canvas.gameObject.transform, false);
        // Add a sprite Renderer to the new Object
        _mapImage = mapGO.AddComponent<UnityEngine.UI.Image>();

        mapTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        mapTexture.filterMode = FilterMode.Point;
    }

    /// <summary>
    /// Update the map based on the TileType
    /// </summary>
    public void UpdateMap()
    {
        mapTexture.Reinitialize(_dungeonWidth, _dungeonDepth, TextureFormat.RGBA32, false);

        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonDepth; y++)
            {
                switch (_dungeon.Tiles[x, y].Type)
                {
                    case Tile.TileType.Corridor:
                        mapTexture.SetPixel(x, y, Color.yellow);
                        break;

                    case Tile.TileType.DoorEnter:
                        mapTexture.SetPixel(x, y, Color.green);
                        break;

                    case Tile.TileType.DoorExit:
                        mapTexture.SetPixel(x, y, Color.magenta);
                        break;

                    case Tile.TileType.FloorEnter:
                        mapTexture.SetPixel(x, y, Color.darkTurquoise);
                        break;

                    case Tile.TileType.FloorExit:
                        mapTexture.SetPixel(x, y, Color.maroon);
                        break;

                    case Tile.TileType.Room:
                        mapTexture.SetPixel(x, y, Color.white);
                        break;

                    case Tile.TileType.Wall:
                        mapTexture.SetPixel(x, y, Color.black);
                        break;

                    default:
                        mapTexture.SetPixel(x, y, Color.clear);
                        break;
                }
            }
        }

        if (activeScene.name == "GameScene")
        {
            // Convert player position to map coordinates
            // considering the offsets of the dungeon in multiple floors
            int playerTileX = (int)(_player.transform.position.x / _dungeonScale) - _xOffset;
            int playerTileY = (int)(_player.transform.position.z / _dungeonScale) - _zOffset;

            if (_dungeon.Tiles[playerTileX, playerTileY].Type != Tile.TileType.Wall)
            {
                mapTexture.SetPixel(playerTileX, playerTileY, Color.gray);
            }
            else if (_dungeonWidth >= _dungeonDepth)
            {
                // Map is generated 'Vertical-first'
                // 'Horizontal-First' better aligns the player with the map
                if (_dungeon.Tiles[playerTileX + 1, playerTileY].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX + 1, playerTileY, Color.gray);
                }
                else if (_dungeon.Tiles[playerTileX - 1, playerTileY].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX - 1, playerTileY, Color.gray);
                }
                else if (_dungeon.Tiles[playerTileX, playerTileY + 1].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX, playerTileY + 1, Color.gray);
                }
                else if (_dungeon.Tiles[playerTileX, playerTileY - 1].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX, playerTileY - 1, Color.gray);
                }
            }
            else
            {
                // Map is generated 'Horizontal-first'
                // 'Vertical-First' better aligns the player with the map
                if (_dungeon.Tiles[playerTileX, playerTileY + 1].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX, playerTileY + 1, Color.gray);
                }
                else if (_dungeon.Tiles[playerTileX, playerTileY - 1].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX, playerTileY - 1, Color.gray);
                }
                else if (_dungeon.Tiles[playerTileX + 1, playerTileY].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX + 1, playerTileY, Color.gray);
                }
                else if (_dungeon.Tiles[playerTileX - 1, playerTileY].Type != Tile.TileType.Wall)
                {
                    mapTexture.SetPixel(playerTileX - 1, playerTileY, Color.gray);
                }
            }
        }

        mapTexture.Apply();
        RefreshMap(new Vector2(_dungeonWidth, _dungeonDepth));
    }

    /// <summary>
    /// Refresh the dungeon map
    /// </summary>
    /// <param name="sizePx"></param>
    private void RefreshMap(Vector2 sizePx)
    {
        //keep order of this changes:		
        _mapImage.rectTransform.anchorMin = new Vector2(1F, 0F);
        _mapImage.rectTransform.anchorMax = new Vector2(1F, 0F);
        _mapImage.rectTransform.pivot = new Vector2(1F, 0F);
        _mapImage.rectTransform.offsetMin = Vector2.zero;
        _mapImage.rectTransform.offsetMax = sizePx * 1F;   //1 tile = 2x2 px
        _mapImage.rectTransform.anchoredPosition = new Vector2(-3, +3);//small dist from corner	

        Sprite sprite = Sprite.Create(mapTexture, new Rect(0, 0, mapTexture.width, mapTexture.height), new Vector2(0.5F, 0.5F));
        _mapImage.sprite = sprite;
        _mapImage.enabled = true;
    }

    #endregion

    #region DEBUG

    /// <summary>
    /// Main method to print Debug notes
    /// </summary>
    private void DebugOutput()
    {
        // Print Dungeon tiles
        PrintDungeonTiles();

        // Print Rooms
        PrintDungeonRooms();

        // All room connections
        PrintEdges();

        // MST
        PrintMST();

        // Dundeon corridors
        PrintDungeonCorridors();
    }

    /// <summary>
    /// Print the accepted corridors in the dungeon
    /// </summary>
    private void PrintDungeonCorridors()
    {
        foreach (var edge in _dungeon.Corridors)
        {
            Debug.Log("Corridor: " + edge.ToString());
        }
    }

    /// <summary>
    /// Print all room connections – Delaunay Triangulation
    /// </summary>
    private void PrintEdges()
    {
        foreach (var edge in edges)
        {
            Debug.Log("Edge: " + edge.ToString());
        }
    }

    /// <summary>
    /// Print the accepted corridors in the dungeon
    /// </summary>
    private void PrintDungeonTiles()
    {
        for (int i = 0; i < _dungeonWidth; i++)
        {
            for (int j = 0; j < _dungeonDepth; j++)
            {
                Debug.Log("X: " + i.ToString() + ", Y: " + j.ToString() + ", " + _dungeon.Tiles[i, j].ToString());
            }
        }
    }

    
    /// <summary>
    /// Print the Edges resulting from Kruskal’s algorithm
    /// </summary>
    private void PrintMST()
    {
        foreach (var edge in MST)
        {
            Debug.Log("MST: " + edge.ToString());
        }
    }

    /// <summary>
    /// Print the properties of each room
    /// </summary>
    private void PrintDungeonRooms()
    {
        foreach (var room in _dungeon.Rooms)
        {
            Debug.Log("Room: " + room.ToString());
        }
    }

    #endregion
}
