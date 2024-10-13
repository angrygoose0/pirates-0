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
    // Extension method to convert Direction to Vector3Int
    public static Vector3Int ToVector3Int(this Direction direction)
    {
        switch (direction)
        {
            case Direction.S:
                return new Vector3Int(0, -1, 0); // South (0, -1)
            case Direction.SE:
                return new Vector3Int(1, -1, 0); // South-East (1, -1)
            case Direction.E:
                return new Vector3Int(1, 0, 0);  // East (1, 0)
            case Direction.NE:
                return new Vector3Int(1, 1, 0);  // North-East (1, 1)
            case Direction.N:
                return new Vector3Int(0, 1, 0);  // North (0, 1)
            case Direction.NW:
                return new Vector3Int(-1, 1, 0); // North-West (-1, 1)
            case Direction.W:
                return new Vector3Int(-1, 0, 0); // West (-1, 0)
            case Direction.SW:
                return new Vector3Int(-1, -1, 0); // South-West (-1, -1)
            default:
                return Vector3Int.zero;         // Default to (0, 0, 0) if no direction is matched
        }
    }

    // positive=clockwise | negative=anti-clockwise
    public static Direction Rotate(this Direction direction, int steps)
    {
        int totalDirections = System.Enum.GetValues(typeof(Direction)).Length; // Get the number of enum values (8)
        // Use modulo to wrap around in both positive and negative directions
        int next = ((int)direction + steps % totalDirections + totalDirections) % totalDirections;
        return (Direction)next;
    }
}
