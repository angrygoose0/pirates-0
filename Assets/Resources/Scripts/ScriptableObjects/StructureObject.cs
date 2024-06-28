using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "StructureObject", menuName = "Structure", order = 1)]
public class StructureObject : ScriptableObject
{
    public int spawnWeight;
    public int height;
    public int width;

}
