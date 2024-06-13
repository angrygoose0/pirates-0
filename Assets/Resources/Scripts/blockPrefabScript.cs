using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockPrefabScript : MonoBehaviour
{
    public BlockObject blockObject; // Variable to store block objects
    public Vector2 blockDirection = Vector2.up; // Variable to store the direction as a Vector2

    private List<BlockValue> GetBlockValues()
    {
        if (blockObject != null)
        {
            return blockObject.blockValues;
        }
        else
        {
            Debug.LogWarning("BlockObject is not assigned.");
            return null;
        }
    }

    void Start()
    {
        // Ensure the sprite renderer is set up with the correct sprite
        if (blockObject != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = blockObject.blockSprite;
            }
        }
        else
        {
            Debug.LogWarning("BlockObject is not assigned.");
        }
    }

    public float GetBlockValueByName(string name)
    {
        if (blockObject != null)
        {
            foreach (BlockValue blockValue in blockObject.blockValues)
            {
                if (blockValue.name == name)
                {
                    return blockValue.value;
                }
            }
            Debug.LogWarning($"BlockValue with name {name} not found.");
            return float.NaN;
        }
        else
        {
            Debug.LogWarning("BlockObject is not assigned.");
            return float.NaN;
        }

    }


    //blockDirection.normalized * GetBlockValueByName(mastForce);

}
