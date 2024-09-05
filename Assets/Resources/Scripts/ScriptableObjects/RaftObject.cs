using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RaftObject", menuName = "Raft", order = 1)]
public class RaftObject : ScriptableObject
{
    public float health;
    public float armor;
    public float mass;
    public TileBase tilemap;




}
