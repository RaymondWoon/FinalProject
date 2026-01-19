using System;
using UnityEngine;

public class Edge
{
    public Room A, B;
    public double distance;

    /// <summary>
    /// Constructor connecting two rooms
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public Edge(Room a, Room b)
    {
        A = a;
        B = b;
        distance = Math.Sqrt(
            Math.Pow(a.CenterX - b.CenterX, 2)
            + Math.Pow(a.CenterZ - b.CenterZ, 2));
    }

    /// <summary>
    /// Override the default for debugging
    /// Print the properties of the edge
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string template = "(X: {0}, Y: {1}), (X: {2}, Y: {3}), distance: {4}";
        string edgeData = string.Format(template, A.CenterX, A.CenterZ, B.CenterX, B.CenterZ, string.Format("{0:F3}", distance));

        return edgeData;
    }
}
