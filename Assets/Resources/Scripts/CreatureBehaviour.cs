using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureBehaviour : MonoBehaviour
{
    public enum State
    {
        Idle,
        // Add more states here as needed
    }

    public Vector2Int spawnTilePosition; // Define the tile to spawn the creature on
    public Tilemap tilemap; // Reference to the tilemap
    public float rotationSpeed = 200f; // Rotation speed in degrees per second
    public float maxMoveSpeed = 3f; // Maximum movement speed in units per second
    public float acceleration = 2f; // Acceleration in units per second squared
    public float deceleration = 2f; // Deceleration in units per second squared
    public float idleDelay = 2f; // Time to wait before moving again in seconds

    private Vector3 targetPosition; // Position to move towards
    private Vector3 velocity = Vector3.zero; // Current velocity

    public float range = 5f;

    private Vector3Int currentTilePosition; // Variable to store the current tile position
    private State currentState; // Current state of the creature
    private bool isIdleCoroutineRunning = false; // Flag to track if the idle coroutine is running

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
        ChangeState(State.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        // Update the current tile position
        currentTilePosition = tilemap.WorldToCell(transform.position);

        // Update the current state
        switch (currentState)
        {
            case State.Idle:
                UpdateIdleState();
                break;

                // Add cases for other states here
        }
    }

    private void ChangeState(State newState)
    {
        currentState = newState;
        switch (newState)
        {
            case State.Idle:
                EnterIdleState();
                break;

                // Add cases for entering other states here
        }
    }

    private void EnterIdleState()
    {
        if (!isIdleCoroutineRunning)
        {
            StartCoroutine(IdleStateCoroutine());
        }
    }

    private void UpdateIdleState()
    {
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
            velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.deltaTime);
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

    private IEnumerator IdleStateCoroutine()
    {
        isIdleCoroutineRunning = true;
        while (currentState == State.Idle)
        {
            // Get surrounding tiles within the given range and pick a random one
            List<Vector3Int> surroundingTiles = GetSurroundingTiles(currentTilePosition, range);
            if (surroundingTiles.Count > 0)
            {
                Vector3Int randomTile = surroundingTiles[Random.Range(0, surroundingTiles.Count)];
                targetPosition = tilemap.GetCellCenterLocal(randomTile);
                Debug.Log("Random surrounding tile: " + randomTile);
            }

            // Wait for the idle delay before picking a new target
            yield return new WaitForSeconds(idleDelay);
        }
        isIdleCoroutineRunning = false;
    }

    // Optional: Method to get the current tile position
    public Vector3Int GetCurrentTilePosition()
    {
        return currentTilePosition;
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
