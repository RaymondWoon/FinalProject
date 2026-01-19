using UnityEngine;

public class Room
{

    // X coordinate of the lower left corner
    public int StartX { get; set; }
    // Z coordinate of the lower left corner
    public int StartZ { get; set; }
    // Width of room
    public int Width { get; set; }
    // Depth of room
    public int Depth { get; set; }
    // Tag used to identify the type of room
    public string Tag { get; set; }

    // X coordinate of the centre
    public int CenterX => (int)(StartX + Width / 2);
    // Z Coordinate of the centre
    public int CenterZ => (int)(StartZ + Depth / 2);

    /// <summary>
    /// Default constructor
    /// </summary>
    public Room()
    {
        StartX = 0;
        StartZ = 0;
        Width = 0;
        Depth = 0;
        Tag = "NA";
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public Room(int x, int z, int width, int depth, string tag = "NA")
    {
        StartX = x;
        StartZ = z;
        Width = width;
        Depth = depth;
        Tag = tag;
    }

    /// <summary>
    /// Determines whether 2 rooms intersect or overlap
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Intersects(Room other)
    {
        // Check horizontal edges then vertical edges
        return !(StartX + Width < other.StartX || other.StartX + other.Width < StartX
            || StartZ + Depth < other.StartZ ||  other.StartZ + other.Depth < StartZ);
    }

    /// <summary>
    /// Override the default for debugging
    /// Print the properties of the room
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string template = "X: {0}, Z: {1}, Width: {2}, Depth: {3}, Tag: {4}";
        string roomData = string.Format(template, StartX, StartZ, Width, Depth, Tag);

        return roomData;
    }
}
