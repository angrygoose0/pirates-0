using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "RaftObject", menuName = "Raft", order = 1)]
public class RaftObject : ScriptableObject
{
    public float health;
    public float armor;
    public float mass;
    public TileBase tilemap;

    public float healthRegenRate = 2f; // Health points regenerated per second
    public float regenDelay = 5f; // Time in seconds before health starts regenerating




}
