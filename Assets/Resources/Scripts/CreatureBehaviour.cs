using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureBehaviour : MonoBehaviour
{
    public Vector2Int spawnTilePosition; // Define the tile to spawn the creature on
    public Tilemap tilemap; // Reference to the tilemap
    public float rotationSpeed = 200f; // Rotation speed in degrees per second
    public float maxMoveSpeed = 3f; // Maximum movement speed in units per second
    public float acceleration = 2f; // Acceleration in units per second squared
    public float deceleration = 2f; // Deceleration in units per second squared

    private Vector3 targetPosition; // Position to move towards
    private Vector3 velocity = Vector3.zero; // Current velocity

    // Start is called before the first frame update
    void Start()
    {
        // Convert the tile position to world position and set the creature's position
        Vector3 worldPosition = tilemap.GetCellCenterWorld((Vector3Int)spawnTilePosition);
        transform.position = worldPosition;
        targetPosition = transform.position; // Initial target is the spawn position
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0; // Ensure the z position is zero since we are in 2D

        Vector3Int mouseTilePosition = tilemap.WorldToCell(mouseWorldPosition);

        // Set the new target position to the center of the tile under the mouse cursor
        targetPosition = tilemap.GetCellCenterWorld(mouseTilePosition);

        // Adjust the target position for isometric scaling
        Vector3 adjustedTargetPosition = new Vector3(targetPosition.x, targetPosition.y * 0.5f, targetPosition.z);

        // Calculate the direction and distance to the adjusted target position
        Vector3 direction = adjustedTargetPosition - transform.position;
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
        transform.position += velocity * Time.deltaTime;

        // Calculate the target angle for rotation
        if (velocity.magnitude > 0.1f) // Only rotate if moving
        {
            float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            // Smoothly rotate towards the target angle
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
