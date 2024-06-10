using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShipMovement : MonoBehaviour
{
    public ShipGenerator shipGenerator;
    public Transform tilemap; // Reference to the tilemap or parent object containing the tilemap
    public float maxSpeed = 5f; // The maximum speed at which the ship moves
    public float mass = 1f; // The mass of the ship, which affects acceleration and deceleration

    private Vector2 currentVelocity; // The current velocity of the ship
    private Vector2 totalForce; // The total force exerted by the mast blocks

    void Start()
    {

        // Call the method to collect all mast blocks
        CollectMastBlocks();
    }

    void Update()
    {
        // Calculate the total force from mast blocks
        CalculateTotalForce();

        // Update the current velocity based on the total force
        UpdateVelocity();

        // Move the tilemap in the opposite direction of the ship's movement
        MoveTilemap();
    }

    void CollectMastBlocks()
    {
        // left blank for now.
    }

    void CalculateTotalForce()
    {
        totalForce = Vector2.zero;
        float mastForce = 5f; // Each mast block gives off a force of 5

        foreach (GameObject mastBlock in shipGenerator.mastBlocks)
        {
            blockPrefabScript blockScript = mastBlock.GetComponent<blockPrefabScript>();
            if (blockScript != null)
            {
                totalForce += blockScript.GetForceVector(mastForce);
            }
        }
    }

    void UpdateVelocity()
    {
        if (totalForce != Vector2.zero)
        {
            // Calculate the acceleration based on the total force and mass
            Vector2 acceleration = totalForce / mass;
            currentVelocity = Vector2.ClampMagnitude(currentVelocity + acceleration * Time.deltaTime, maxSpeed);
        }
        else
        {
            // Decelerate to a stop
            Vector2 deceleration = -currentVelocity.normalized * maxSpeed / mass;
            currentVelocity += deceleration * Time.deltaTime;

            // Ensure we don't overshoot and reverse direction
            if (currentVelocity.magnitude < deceleration.magnitude * Time.deltaTime)
            {
                currentVelocity = Vector2.zero;
            }
        }
    }

    void MoveTilemap()
    {
        // Calculate the movement vector
        Vector3 movement = new Vector3(currentVelocity.x, currentVelocity.y, 0) * Time.deltaTime;

        // Apply the movement to the tilemap
        tilemap.position -= movement;
    }

}
