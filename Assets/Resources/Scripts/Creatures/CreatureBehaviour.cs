using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureBehaviour : MonoBehaviour
{
    public enum State
    {
        Idle,
        Aggressive,
        // Add more states here as needed
    }

    public Vector2Int spawnTilePosition; // Define the tile to spawn the creature on
    public Tilemap tilemap; // Reference to the tilemap
    public float rotationSpeed = 200f; // Rotation speed in degrees per second
    public float maxMoveSpeed = 3f; // Maximum movement speed in units per second
    public float acceleration = 2f; // Acceleration in units per second squared
    public float deceleration = 2f; // Deceleration in units per second squared
    public float movementDelay; // Time to wait before moving again in seconds

    public GameObject targetShipPart; //for when raft pieces have invisible gameobjects that count the hp, etc

    private Vector3 targetPosition; // Position to move towards
    private Vector3 velocity = Vector3.zero; // Current velocity

    public float range = 5f;
    public float hostility; //0=passive, 100=AGGRESSIVE
    public float aggressionThreshold = 75f; // Threshold for changing state to aggressive

    private Vector3Int currentTilePosition; // Variable to store the current tile position
    public State currentState; // Current state of the creature
    private bool isMovementCoroutineRunning = false; // Flag to track if the movement coroutine is running

    // Start is called before the first frame update
    void Start()
    {
        // Convert the tile position to world position and set the creature's position
        Vector3 localPosition = tilemap.GetCellCenterLocal((Vector3Int)spawnTilePosition);
        transform.localPosition = localPosition;
        targetPosition = transform.localPosition; // Initial target is the spawn position

        // Initialize the current tile position
        currentTilePosition = tilemap.WorldToCell(transform.position);

        // Set the initial state
        currentState = State.Idle;

        if (!isMovementCoroutineRunning)
        {
            StartCoroutine(MovementCoroutine());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update the current tile position
        currentTilePosition = tilemap.WorldToCell(transform.position);

        // Check if hostility has reached the threshold
        if (hostility >= aggressionThreshold && currentState != State.Aggressive)
        {
            currentState = State.Aggressive;
        }
        else if (hostility < aggressionThreshold && currentState != State.Idle)
        {
            currentState = State.Idle;
        }

        UpdateMovement();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the collider has the specified tag
        if (other.CompareTag("Ship"))
        {
            // Perform actions when the creature collides with the ship
        }
    }

    private void EnterMovementState()
    {
        // Define behavior when entering a new movement state
    }

    private void UpdateMovement()
    {
        // Adjust the acceleration based on the current state
        float currentAcceleration = currentState == State.Aggressive ? acceleration * 1.5f : acceleration;

        // Calculate the direction and distance to the target position
        Vector3 direction = targetPosition - transform.localPosition;
        float distance = direction.magnitude;
        direction.Normalize();

        // Determine the target velocity
        Vector3 targetVelocity = direction * maxMoveSpeed;

        // Adjust the target velocity for isometric scaling
        targetVelocity.y *= 0.5f;

        // Calculate the acceleration or deceleration
        if (distance > 0.1f)
        {
            // Accelerate towards the target velocity
            velocity = Vector3.MoveTowards(velocity, targetVelocity, currentAcceleration * Time.deltaTime);
        }
        else
        {
            // Decelerate to a stop
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Move the creature
        transform.localPosition += velocity * Time.deltaTime;

        // Calculate the target angle for rotation
        if (velocity.magnitude > 0.1f) // Only rotate if moving
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            // Smoothly rotate towards the target angle
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private IEnumerator MovementCoroutine()
    {
        isMovementCoroutineRunning = true;
        while (true)
        {
            List<Vector3Int> surroundingTiles = GetSurroundingTiles(currentTilePosition, range);

            bool found = false;
            Vector3Int targetTile;
            if (surroundingTiles.Count > 0)
            {
                switch (currentState)
                {
                    case State.Idle:
                        // Pick a random tile.
                        targetTile = surroundingTiles[Random.Range(0, surroundingTiles.Count)];
                        targetPosition = tilemap.GetCellCenterLocal(targetTile);
                        break;
                    case State.Aggressive:


                        Vector3 targetShipPartLocalPosition = tilemap.WorldToCell(targetShipPart.transform.position);
                        targetTile = Vector3Int.FloorToInt(targetShipPartLocalPosition);
                        foreach (Vector3Int tile in surroundingTiles)
                        {

                            if (tile == targetTile)
                            {
                                targetPosition = tilemap.GetCellCenterLocal(tile);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            targetTile = surroundingTiles[Random.Range(0, surroundingTiles.Count)];
                            targetPosition = tilemap.GetCellCenterLocal(targetTile);
                            hostility -= 10; // Reduce hostility only when the exact tile is not found
                        }
                        break;
                        // Add cases for other states here
                }
            }

            movementDelay = 2.0f;
            // Wait for the delay before picking a new targetTile
            yield return new WaitForSeconds(movementDelay);
        }
    }



    // Method to get surrounding tiles within a given range
    private List<Vector3Int> GetSurroundingTiles(Vector3Int centerTile, float range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        int rangeInt = Mathf.CeilToInt(range);

        for (int x = -rangeInt; x <= rangeInt; x++)
        {
            for (int y = -rangeInt; y <= rangeInt; y++)
            {
                Vector3Int tile = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);
                if (Vector3Int.Distance(centerTile, tile) <= range)
                {
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }
}
