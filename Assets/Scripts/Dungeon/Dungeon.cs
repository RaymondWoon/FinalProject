using System;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon
{

    private Tile[,] tiles;

    private List<Room> rooms;

    private List<Edge> corridors;

    public Tile[,] Tiles { get { return tiles; } }

    public List<Room> Rooms { get { return rooms; } }

    public List<Edge> Corridors
    {
        get { return corridors; }
        set { corridors = value; }
    }

    public void SetTile(int x, int z, Tile tile)
    {
        tiles[x, z] = tile;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public Dungeon() { }

    /// <summary>
    /// Constructor to initialize the Tile array
    /// </summary>
    /// <param name="width"></param>
    /// <param name="depth"></param>
    public Dungeon(int width, int depth)
    {
        // Initialize dungeon array
        tiles = new Tile[width, depth];

        // Initialize the dungeon rooms
        rooms = new List<Room>();

        // Initialize the dungeon edges (connectors)
        corridors = new List<Edge>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < depth; y++)
            {
                Tile tile = new();

                SetTile(x, y, tile);
            }
        }
    }

}
