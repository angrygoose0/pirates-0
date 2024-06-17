using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileObject", menuName = "Tile", order = 1)]
public class TileObject : ScriptableObject
{
    public TileBase tileBase; // The tile's visual representation
    public Vector2Int temperatureRange; // Range of acceptable temperatures (inclusive)
    public Vector2Int depthRange; // Range of acceptable depths (inclusive)
    public Vector2Int hostilityRange; // Range of acceptable hostilities (inclusive)

}
