using UnityEngine;
using System.Collections.Generic;

public enum BlockType
{
    Cannon,
    Payload,
    Mast
}

[System.Serializable]
public class BlockValue
{
    public string name;
    public float value;

    public BlockValue(string name, float value)
    {
        this.name = name;
        this.value = value;
    }
}

[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Block", order = 1)]
public class BlockObject : ScriptableObject
{
    public float id;
    public Sprite blockSprite;
    public Sprite selectedSprite;
    public BlockType blockType;
    public List<BlockValue> blockValues;
}
