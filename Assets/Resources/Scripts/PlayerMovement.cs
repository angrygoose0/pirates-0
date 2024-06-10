using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float bounceBackFactor = 0.2f; // Factor to determine how much the player bounces back
    public Rigidbody2D rb;
    public ShipGenerator shipGenerator; // Reference to the ShipGenerator script
    public Vector2 tileOffset; // Offset to adjust for isometric alignment
    public TileBase interactableTileSprite; // New sprite for interactable tiles
    public TileBase defaultTileSprite; // Default sprite for tiles

    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private Vector3Int facingTilePosition; // Variable to store the tile position the player is facing
    private Vector3Int? previousInteractableTilePosition; // Store the previous interactable tile position

    public InteractionManager interactionManager; // Reference to the InteractionManager script
    // Use ShipGenerator's Direction enum
    public ShipGenerator.Direction currentDirection;

    void Update()
    {
        // Get input from the player
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.y = Input.GetAxis("Vertical") / 2f;

        // Normalize the input to prevent faster diagonal movement
        movementInput = movementInput.normalized;

        // Update the direction based on movement input
        UpdateDirection();

        // Check if the player presses the "E" key to interact with a block
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithBlock(0);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InteractWithBlock(1);
        }
    }

    void FixedUpdate()
    {
        // Attempt to move the player
        MovePlayer();

        // Calculate and log the tile the player is facing
        CalculateFacingTile();
    }

    void MovePlayer()
    {
        // Apply acceleration or deceleration
        if (movementInput != Vector2.zero)
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, movementInput * moveSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Calculate the potential new position
        Vector2 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;

        // Convert the new position to a tilemap position
        Vector3Int tilePosition = shipGenerator.tilemap.WorldToCell(new Vector3(newPosition.x, newPosition.y, 0)) + new Vector3Int((int)tileOffset.x, (int)tileOffset.y, 0);

        // Check if the new tile position is walkable
        if (shipGenerator.IsTileWalkable(tilePosition))
        {
            // Move the player using the Rigidbody2D component
            rb.MovePosition(newPosition);
        }
        else
        {
            // Apply bounce back effect
            currentVelocity = -currentVelocity * bounceBackFactor;
            newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    void CalculateFacingTile()
    {
        Vector3Int playerTilePosition = shipGenerator.tilemap.WorldToCell(rb.position) + new Vector3Int((int)tileOffset.x, (int)tileOffset.y, 0);

        // Get the list of interactable tiles around the player
        List<(Vector3Int position, ShipGenerator.Direction direction)> interactableTiles = shipGenerator.GetInteractableNeighbors(playerTilePosition);

        // Reset the previous interactable tile if it is not in the list of interactable tiles
        if (previousInteractableTilePosition.HasValue && !interactableTiles.Exists(t => t.position == previousInteractableTilePosition.Value))
        {
            ResetTile(previousInteractableTilePosition.Value);
            previousInteractableTilePosition = null;
        }

        // Find the interactable tile that matches the current direction or is the closest
        var bestMatch = FindClosestDirectionTile(interactableTiles, currentDirection);

        if (bestMatch != null)
        {
            HighlightTile(bestMatch.Value.position);
        }
    }

    (Vector3Int position, ShipGenerator.Direction direction)? FindClosestDirectionTile(List<(Vector3Int position, ShipGenerator.Direction direction)> tiles, ShipGenerator.Direction targetDirection)
    {
        if (tiles == null || tiles.Count == 0) return null;

        // Create a sorted list of directions in the order of preference (exact match first, then closest)
        ShipGenerator.Direction[] directionOrder = new ShipGenerator.Direction[]
        {
            targetDirection,
            GetClockwiseDirection(targetDirection),
            GetCounterClockwiseDirection(targetDirection),
            GetClockwiseDirection(GetClockwiseDirection(targetDirection)),
            GetCounterClockwiseDirection(GetCounterClockwiseDirection(targetDirection))
        };

        foreach (var direction in directionOrder)
        {
            foreach (var tile in tiles)
            {
                if (tile.direction == direction)
                {
                    return tile;
                }
            }
        }

        // Return the first tile if no close match found (fallback)
        return tiles[0];
    }

    ShipGenerator.Direction GetClockwiseDirection(ShipGenerator.Direction direction)
    {
        switch (direction)
        {
            case ShipGenerator.Direction.N: return ShipGenerator.Direction.NE;
            case ShipGenerator.Direction.NE: return ShipGenerator.Direction.E;
            case ShipGenerator.Direction.E: return ShipGenerator.Direction.SE;
            case ShipGenerator.Direction.SE: return ShipGenerator.Direction.S;
            case ShipGenerator.Direction.S: return ShipGenerator.Direction.SW;
            case ShipGenerator.Direction.SW: return ShipGenerator.Direction.W;
            case ShipGenerator.Direction.W: return ShipGenerator.Direction.NW;
            case ShipGenerator.Direction.NW: return ShipGenerator.Direction.N;
            default: return direction;
        }
    }

    ShipGenerator.Direction GetCounterClockwiseDirection(ShipGenerator.Direction direction)
    {
        switch (direction)
        {
            case ShipGenerator.Direction.N: return ShipGenerator.Direction.NW;
            case ShipGenerator.Direction.NW: return ShipGenerator.Direction.W;
            case ShipGenerator.Direction.W: return ShipGenerator.Direction.SW;
            case ShipGenerator.Direction.SW: return ShipGenerator.Direction.S;
            case ShipGenerator.Direction.S: return ShipGenerator.Direction.SE;
            case ShipGenerator.Direction.SE: return ShipGenerator.Direction.E;
            case ShipGenerator.Direction.E: return ShipGenerator.Direction.NE;
            case ShipGenerator.Direction.NE: return ShipGenerator.Direction.N;
            default: return direction;
        }
    }

    void HighlightTile(Vector3Int tilePosition)
    {
        // Reset the previous interactable tile to the default sprite
        if (previousInteractableTilePosition.HasValue && previousInteractableTilePosition.Value != tilePosition)
        {
            ResetTile(previousInteractableTilePosition.Value);
        }

        // Set the new tile to the interactable sprite
        shipGenerator.tilemap.SetTile(tilePosition, interactableTileSprite);

        // Check if the new tile has a blockPrefab and set its sprite to selectedSprite
        GameObject blockPrefab = shipGenerator.GetBlockPrefabAtTile(tilePosition);
        if (blockPrefab != null)
        {
            SpriteRenderer sr = blockPrefab.GetComponent<SpriteRenderer>();
            BlockObject blockObject = sr.GetComponent<blockPrefabScript>().blockObject;
            sr.sprite = blockObject.selectedSprite;
        }

        // Update the previous interactable tile position
        previousInteractableTilePosition = tilePosition;
    }

    void ResetTile(Vector3Int tilePosition)
    {
        // Reset the tile to the default sprite
        shipGenerator.tilemap.SetTile(tilePosition, defaultTileSprite);

        // Check if the tile has a blockPrefab and reset its sprite
        GameObject blockPrefab = shipGenerator.GetBlockPrefabAtTile(tilePosition);
        if (blockPrefab != null)
        {
            SpriteRenderer sr = blockPrefab.GetComponent<SpriteRenderer>();
            BlockObject blockObject = sr.GetComponent<blockPrefabScript>().blockObject;
            sr.sprite = blockObject.blockSprite;
        }
    }

    void UpdateDirection()
    {
        if (movementInput == Vector2.zero) return;

        if (movementInput.x > 0 && movementInput.y > 0)
            currentDirection = ShipGenerator.Direction.NE;
        else if (movementInput.x > 0 && movementInput.y < 0)
            currentDirection = ShipGenerator.Direction.SE;
        else if (movementInput.x < 0 && movementInput.y > 0)
            currentDirection = ShipGenerator.Direction.NW;
        else if (movementInput.x < 0 && movementInput.y < 0)
            currentDirection = ShipGenerator.Direction.SW;
        else if (movementInput.x > 0)
            currentDirection = ShipGenerator.Direction.E;
        else if (movementInput.x < 0)
            currentDirection = ShipGenerator.Direction.W;
        else if (movementInput.y > 0)
            currentDirection = ShipGenerator.Direction.N;
        else if (movementInput.y < 0)
            currentDirection = ShipGenerator.Direction.S;

    }

    void InteractWithBlock(int interaction)
    {
        if (previousInteractableTilePosition.HasValue)
        {
            Vector3Int tilePosition = previousInteractableTilePosition.Value;
            GameObject blockPrefab = shipGenerator.GetBlockPrefabAtTile(tilePosition);

            if (blockPrefab != null)
            {
                interactionManager.InteractWithBlock(blockPrefab, interaction);

            }
        }
    }
}
