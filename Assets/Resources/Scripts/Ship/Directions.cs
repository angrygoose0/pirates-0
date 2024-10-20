using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction
{
    S,  // South
    SE, // South-East
    E,  // East
    NE, // North-East
    N,  // North
    NW, // North-West
    W,  // West
    SW  // South-West
}

public static class DirectionExtensions
{
    // Extension method to convert Direction to Vector2
    public static Vector2 ToVector2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.S:
                return new Vector2(0, -1); // South (0, -1)
            case Direction.SE:
                return new Vector2(1, -1); // South-East (1, -1)
            case Direction.E:
                return new Vector2(1, 0);  // East (1, 0)
            case Direction.NE:
                return new Vector2(1, 1);  // North-East (1, 1)
            case Direction.N:
                return new Vector2(0, 1);  // North (0, 1)
            case Direction.NW:
                return new Vector2(-1, 1); // North-West (-1, 1)
            case Direction.W:
                return new Vector2(-1, 0); // West (-1, 0)
            case Direction.SW:
                return new Vector2(-1, -1); // South-West (-1, -1)
            default:
                return Vector2.zero;         // Default to (0, 0) if no direction is matched
        }
    }

    // positive=clockwise | negative=anti-clockwise
    public static Direction Rotate(this Direction direction, int steps) // 1 step is 45 degrees
    {
        int totalDirections = System.Enum.GetValues(typeof(Direction)).Length; // Get the number of enum values (8)
        // Use modulo to wrap around in both positive and negative directions
        int next = ((int)direction + steps % totalDirections + totalDirections) % totalDirections;
        return (Direction)next;
    }

    public static int GetZRotation(this Direction direction) // this is for the 2d isometric view
    {
        switch (direction)
        {
            case Direction.N:
                return 0;   // North
            case Direction.NE:
                return 60;  // North-East
            case Direction.E:
                return 90;  // East
            case Direction.SE:
                return 120; // South-East
            case Direction.S:
                return 180; // South
            case Direction.SW:
                return 240; // South-West
            case Direction.W:
                return 270; // West
            case Direction.NW:
                return 300; // North-West
            default:
                return 0;
        }
    }
}
