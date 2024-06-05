using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockPrefabScript : MonoBehaviour
{
    public BlockObject blockObject; // Variable to store block objects

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
}
