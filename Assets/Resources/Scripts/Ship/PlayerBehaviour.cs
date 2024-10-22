using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

public class PlayerBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float bounceBackFactor = 0.2f; // Factor to determine how much the player bounces back
    public Rigidbody2D rb;
    public Vector2 tileOffset; // Offset to adjust for isometric alignment
    public TileBase interactableTileSprite; // New sprite for interactable tiles
    public TileBase defaultTileSprite; // Default sprite for tiles

    private Vector2 movementInput;
    private Vector2 currentVelocity;
    private Vector3Int facingTilePosition; // Variable to store the tile position the player is facing
    private Vector3Int? previousInteractableTilePosition; // Store the previous interactable tile position
    public GameObject selectedBlockPrefab = null;

    public SpriteRenderer spriteRenderer;
    public Sprite spriteN, spriteNE, spriteE, spriteSE, spriteS, spriteSW, spriteW, spriteNW;

    public float dashSpeed = 10f;      // How fast the dash is
    public float dashDuration = 0.2f;  // How long the dash lasts
    public float dashCooldown = 2f;    // Cooldown between dashes

    private bool isDashing = false;    // Flag to check if the player is dashing
    private float dashTime = 0f;       // Timer for dash duration
    private float lastDashTime = 0f;   // Timer to track cooldown



    public GameObject equippedItem;
    public GameObject closestItem;
    public GameObject equippedBlock;
    public float playerPickupRadius = 5f; // Radius within which to detect items

    public Direction currentDirection;

    public GameObject pickupRadius;

    private bool isFiring = false;
    private Coroutine fireCoroutine;
    private float lastFireTime = 0f;

    private float mouseDownTime = 0f;

    void Start()
    {


        if (equippedItem != null)
        {
            ItemScript itemScript = equippedItem.GetComponent<ItemScript>();
            itemScript.NewParent(gameObject);

            // Set the equipped item's position to the player's position
            equippedItem.transform.localPosition = Vector3.zero;
        }

        pickupRadius.transform.parent = this.transform;
    }
    void Update()
    {
        if (SingletonManager.Instance.gameStart.gameStarted || SingletonManager.Instance.gameStart.trailer)
        {
            // Get input from the player
            movementInput.x = Input.GetAxis("Horizontal");
            movementInput.y = Input.GetAxis("Vertical") / 2f;

            // Normalize the input to prevent faster diagonal movement
            movementInput = movementInput.normalized;

            // Update the direction based on movement input
            UpdateDirection();
            FindClosestItem();

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                ToggleItem();
            }

            if (previousInteractableTilePosition.HasValue)
            {
                Vector3Int tilePosition = previousInteractableTilePosition.Value;
                selectedBlockPrefab = SingletonManager.Instance.shipGenerator.GetBlockPrefabAtTile(tilePosition);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithBlock(0);
            }

            else if (Input.GetKeyDown(KeyCode.Q))
            {
                InteractWithBlock(1);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                InteractWithBlock(3);
            }


            if (Input.GetMouseButtonDown(0) && !isFiring)
            {
                mouseDownTime = Time.time;

                if (selectedBlockPrefab != null)
                {
                    blockPrefabScript blockScript = selectedBlockPrefab.GetComponent<blockPrefabScript>();
                    BlockObject blockObject = blockScript.blockObject;
                    GameObject blockItem = null;

                    if (blockScript.itemPrefabObject != null && blockScript.itemPrefabObject.Count == 1)
                    {
                        blockItem = blockScript.itemPrefabObject[0];
                    }


                    ItemScript blockItemScript = null;
                    ItemObject blockItemObject = null;
                    if (blockItem != null)
                    {
                        blockItemScript = blockItem.GetComponent<ItemScript>();
                        blockItemObject = blockItemScript.itemObject;
                    }

                    if (blockItemObject != null)
                    {
                        if (blockItemObject.projectileData != null && blockItemObject.projectileData.Count == 1)
                        {
                            ProjectileData projectile = blockItemObject.projectileData[0];

                            float attackRate = projectile.reloadSpeed;
                            AbilityData haste = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Haste);

                            if (haste != null)
                            {
                                attackRate = attackRate * 1 / (haste.value);
                            }

                            if (Time.time >= lastFireTime + attackRate)
                            {
                                isFiring = true;
                                fireCoroutine = StartCoroutine(FireRoutine(attackRate));
                            }
                        }
                    }
                }
            }


            // When mouse is released
            if (Input.GetMouseButtonUp(0))
            {

                if (isFiring)
                {
                    isFiring = false;
                    StopCoroutine(fireCoroutine); // Stop the firing if it was happening
                }
                else
                {
                    float heldDuration = Time.time - mouseDownTime;
                    SingletonManager.Instance.fishingLine.FireLine(gameObject.transform, SingletonManager.Instance.worldGenerator.mouseTilePosition, heldDuration);
                }
            }


            /*
            else if (Input.GetKeyDown(KeyCode.F))
            {
                CollectItem();
            }
            */
            if (equippedItem != null)
            {
                // Set the equipped item's position to the player's position
                equippedItem.transform.localPosition = Vector3.zero;
            }
        }
    }

    private IEnumerator FireRoutine(float attackRate)
    {
        while (isFiring)
        {
            InteractWithBlock(2);
            lastFireTime = Time.time;
            yield return new WaitForSeconds(attackRate);
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
        if (SingletonManager.Instance.interactionManager.playerBlockRelations.ContainsKey(gameObject))
        {
            return;
        }

        // Apply acceleration or deceleration
        if (movementInput != Vector2.zero)
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, movementInput * moveSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Calculate the potential new local position (relative to the parent ship)
        Vector3 newLocalPosition = transform.localPosition + (Vector3)(currentVelocity * Time.fixedDeltaTime);

        // Convert the local position to world position
        Vector3 worldPosition = transform.parent.TransformPoint(newLocalPosition);

        // Convert the world position to a tilemap position
        Vector3Int tilePosition = SingletonManager.Instance.shipGenerator.tilemap.WorldToCell(new Vector3(worldPosition.x, worldPosition.y, 0)) + new Vector3Int((int)tileOffset.x, (int)tileOffset.y, 0);

        // Check if the new tile position is walkable
        if (SingletonManager.Instance.shipGenerator.IsTileWalkable(tilePosition))
        {
            // Move the player by updating the local transform
            transform.localPosition = newLocalPosition;
        }
        else
        {
            // Apply bounce back effect
            currentVelocity = -currentVelocity * bounceBackFactor;
            newLocalPosition = transform.localPosition + (Vector3)(currentVelocity * Time.fixedDeltaTime);

            // Update the local position after bounce
            transform.localPosition = newLocalPosition;
        }
    }



    void FindClosestItem()
    {
        // Reset closest item
        closestItem = null;

        // Find all items in the scene
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");//make sure items inside blocks arent tagged as Item.  # alternate solution

        // Initialize the list
        List<GameObject> pickupableItems = new List<GameObject>();

        // Loop through each item
        foreach (GameObject item in items)
        {
            // Get the ItemScript component from the item
            ItemScript itemScript = item.GetComponent<ItemScript>();

            // Check if itemScript is not null and itemPickupable is true
            if (itemScript != null && itemScript.itemPickupable)
            {
                // Add the item to the list
                pickupableItems.Add(item);
            }
        }

        // Initialize the minimum distance with a large number
        float minDistance = Mathf.Infinity;

        // Get the player's position
        Vector2 playerPosition = new Vector2(transform.position.x, transform.position.y);

        foreach (GameObject item in pickupableItems)
        {
            // Get the item's position
            Vector2 itemPosition = new Vector2(item.transform.position.x, item.transform.position.y);

            // Calculate the distance with squashed Y value
            float distance = Mathf.Sqrt(
                Mathf.Pow(playerPosition.x - itemPosition.x, 2) +
                Mathf.Pow((playerPosition.y - itemPosition.y) * 2, 2)
            );

            // If the distance is within the pickup radius and is the closest one found
            if (distance < playerPickupRadius && distance < minDistance)
            {
                minDistance = distance;
                closestItem = item;
            }
        }
    }

    void CalculateFacingTile()
    {
        Vector3Int playerTilePosition = SingletonManager.Instance.shipGenerator.tilemap.WorldToCell(rb.position) + new Vector3Int((int)tileOffset.x, (int)tileOffset.y, 0);

        // Get the list of interactable tiles around the player
        List<(Vector3Int position, Direction direction)> interactableTiles = SingletonManager.Instance.shipGenerator.GetInteractableNeighbors(playerTilePosition);

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

    (Vector3Int position, Direction direction)? FindClosestDirectionTile(List<(Vector3Int position, Direction direction)> tiles, Direction targetDirection)
    {
        if (tiles == null || tiles.Count == 0) return null;

        // Create a sorted list of directions in the order of preference (exact match first, then closest)
        Direction[] directionOrder = new Direction[]
        {
            targetDirection,
            targetDirection.Rotate(2),
            targetDirection.Rotate(-2),
            targetDirection.Rotate(4),
            targetDirection.Rotate(-4),
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


    void HighlightTile(Vector3Int tilePosition)
    {
        // Reset the previous interactable tile to the default sprite
        if (previousInteractableTilePosition.HasValue && previousInteractableTilePosition.Value != tilePosition)
        {
            ResetTile(previousInteractableTilePosition.Value);
        }

        // Set the new tile to the interactable sprite
        SingletonManager.Instance.shipGenerator.tilemap.SetTile(tilePosition, interactableTileSprite);

        // Check if the new tile has a blockPrefab and set its sprite to selectedSprite
        GameObject blockPrefab = SingletonManager.Instance.shipGenerator.GetBlockPrefabAtTile(tilePosition);
        if (blockPrefab != null)
        {
            blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
            blockScript.RotateBlock(0, true);

            GameObject blockItem = null;

            if (blockScript.itemPrefabObject != null && blockScript.itemPrefabObject.Count == 1)
            {
                blockItem = blockScript.itemPrefabObject[0];

                ItemScript blockItemScript;
                ItemObject blockItemObject;
                if (blockItem != null)
                {
                    blockItemScript = blockItem.GetComponent<ItemScript>();
                    blockItemObject = blockItemScript.itemObject;
                }
            }
            List<string> helpfulUIList = new List<string>(); // Initialize the list
            switch (blockScript.blockObject.blockType)
            {
                case BlockType.Cannon:
                    helpfulUIList.Add("[E] Enter / Exit");
                    if (blockItem != null)
                    {
                        helpfulUIList.Add("[Left Click] Fire");
                    }
                    else
                    {
                        helpfulUIList.Add("[Q] Arm Cannon");
                    }
                    break; // Add break statement to prevent fall-through

                case BlockType.Payload:
                    if (blockItem != null)
                    {
                        ItemScript blockItemScript = blockItem.GetComponent<ItemScript>();
                        ItemObject blockItemObject = blockItemScript.itemObject;
                        if (blockItemObject.activeAbility)
                        {
                            helpfulUIList.Add("[E] Activate");
                        }
                    }
                    else
                    {
                        helpfulUIList.Add("[Q] Arm Payload");
                    }
                    break; // Add break statement to prevent fall-through

                // if an item pickable has spawned, shift is available.
                case BlockType.Mast:
                    helpfulUIList.Add("[E] Start / Stop");
                    helpfulUIList.Add("[Q] Rotate");
                    break; // Add break statement to prevent fall-through
            }

            SingletonManager.Instance.uiManager.ToggleHelpfulUI(blockPrefab, helpfulUIList, false);

            // Update the previous interactable tile position
            previousInteractableTilePosition = tilePosition;
        }
    }


    void ResetTile(Vector3Int tilePosition)
    {
        // Reset the tile to the default sprite
        SingletonManager.Instance.shipGenerator.tilemap.SetTile(tilePosition, defaultTileSprite);

        // Check if the tile has a blockPrefab and reset its sprite
        GameObject blockPrefab = SingletonManager.Instance.shipGenerator.GetBlockPrefabAtTile(tilePosition);
        if (blockPrefab != null)
        {
            blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
            blockScript.RotateBlock(0, false);

            List<string> helpfulUIList = new List<string>();
            SingletonManager.Instance.uiManager.ToggleHelpfulUI(blockPrefab, helpfulUIList, false);
        }
    }

    void UpdateDirection()
    {
        if (movementInput == Vector2.zero)
        {
            return;
        }

        if (movementInput.x > 0 && movementInput.y > 0)
        {
            SetDirection(Direction.NE, spriteNE);
        }
        else if (movementInput.x > 0 && movementInput.y < 0)
        {
            SetDirection(Direction.SE, spriteSE);
        }
        else if (movementInput.x < 0 && movementInput.y > 0)
        {
            SetDirection(Direction.NW, spriteNW);
        }
        else if (movementInput.x < 0 && movementInput.y < 0)
        {
            SetDirection(Direction.SW, spriteSW);
        }
        else if (movementInput.x > 0)
        {
            SetDirection(Direction.E, spriteE);
        }
        else if (movementInput.x < 0)
        {
            SetDirection(Direction.W, spriteW);
        }
        else if (movementInput.y > 0)
        {
            SetDirection(Direction.N, spriteN);
        }
        else if (movementInput.y < 0)
        {
            SetDirection(Direction.S, spriteS);
        }

        // Check if space is pressed and if the player can dash
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }

        // Handle dash logic
        if (isDashing)
        {
            Dash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = 0f;
        lastDashTime = Time.time; // Reset the cooldown timer
    }

    void Dash()
    {
        // Increase the dash timer
        dashTime += Time.deltaTime;

        // Move in the direction the player is looking at or moving
        transform.position += (Vector3)currentDirection.ToVector2().normalized * dashSpeed * Time.deltaTime;

        // End the dash if it has lasted for the dashDuration
        if (dashTime >= dashDuration)
        {
            isDashing = false;
        }
    }


    void SetDirection(Direction direction, Sprite sprite)
    {
        currentDirection = direction;
        spriteRenderer.sprite = sprite;
    }


    void InteractWithBlock(int interaction)
    {
        if (previousInteractableTilePosition.HasValue)
        {
            Vector3Int tilePosition = previousInteractableTilePosition.Value;
            selectedBlockPrefab = SingletonManager.Instance.shipGenerator.GetBlockPrefabAtTile(tilePosition);
            SingletonManager.Instance.interactionManager.InteractWithBlock(interaction, gameObject);
        }
    }

    void ToggleItem()
    {
        if (equippedItem != null)
        {
            // Set the equipped item's position to the player's current position
            equippedItem.transform.position = rb.position;

            // Remove the parent from the equipped item
            ItemScript itemScript = equippedItem.GetComponent<ItemScript>();
            itemScript.NewParent(null);

            // Set equippedItem to null as the player no longer has the item equipped
            equippedItem = null;
        }
        else if (closestItem != null)
        {
            ItemScript itemScript = closestItem.GetComponent<ItemScript>();
            if (itemScript.itemPickupable == true)
            {
                equippedItem = closestItem;
                // Set the new parent for the equipped item
                itemScript.NewParent(gameObject);
                itemScript.itemTaken = true;
            }

        }
    }

    /*
    void CollectItem()
    {
        if (equippedItem != null)
        {
            ItemScript itemScript = equippedItem.GetComponent<ItemScript>();
            int goldAmount = itemScript.itemObject.goldAmount;
            if (goldAmount == 0)
            {
                return;
            }
            SingletonManager.Instance.goldManager.AddGold(goldAmount);
            Destroy(equippedItem);
            equippedItem = null;

        }
        else if (closestItem != null)
        {
            ItemScript itemScript = closestItem.GetComponent<ItemScript>();
            int goldAmount = itemScript.itemObject.goldAmount;
            if (goldAmount == 0)
            {
                return;
            }
            SingletonManager.Instance.goldManager.AddGold(goldAmount);
            Destroy(closestItem);
            closestItem = null;
        }
    }
    */



}

