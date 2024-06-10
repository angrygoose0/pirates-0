using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockPrefabScript : MonoBehaviour
{
    public BlockObject blockObject; // Variable to store block objects
    public Vector2 blockDirection = Vector2.up; // Variable to store the direction as a Vector2

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

    // Method to get the force vector based on the block direction
    public Vector2 GetForceVector(float force)
    {
        return blockDirection.normalized * force;
    }
}
