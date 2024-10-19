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

[System.Serializable]
public class DirectionToSprite
{
    public Direction direction;
    public Sprite sprite;
    public Sprite highlightSprite;
}

[CreateAssetMenu(fileName = "Block", menuName = "ScriptableObjects/Block", order = 1)]
public class BlockObject : ScriptableObject
{
    public float id;
    public BlockType blockType;
    public List<BlockValue> blockValues;
    public List<DirectionToSprite> directionToSpriteList = new List<DirectionToSprite>();
    // Read-only dictionaries
    private Dictionary<Direction, Sprite> directionToSpriteDict = new Dictionary<Direction, Sprite>();
    private Dictionary<Direction, Sprite> highlightSpriteDict = new Dictionary<Direction, Sprite>();

    // Public accessors for the read-only dictionaries
    public IReadOnlyDictionary<Direction, Sprite> DirectionToSpriteDict => directionToSpriteDict;
    public IReadOnlyDictionary<Direction, Sprite> HighlightSpriteDict => highlightSpriteDict;


    private void OnEnable()
    {
        // Add items from the list to the dictionary, ensuring unique keys
        foreach (DirectionToSprite directionToSprite in directionToSpriteList)
        {
            // Check if the key already exists
            if (!directionToSpriteDict.ContainsKey(directionToSprite.direction))
            {
                directionToSpriteDict.Add(directionToSprite.direction, directionToSprite.sprite);
            }
            if (!highlightSpriteDict.ContainsKey(directionToSprite.direction))
            {
                highlightSpriteDict.Add(directionToSprite.direction, directionToSprite.highlightSprite);
            }
        }
    }
}
