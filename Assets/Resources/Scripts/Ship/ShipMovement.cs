using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

public class ShipMovement : MonoBehaviour
{
    public GameObject shipTilemap; // Reference to the tilemap or parent object containing the tilemap
    public float kFactor; // constant that multiplies with totalForce to give maxSpeed;
    private float maxSpeed; // The maximum speed at which the ship moves
    public float mass = 1f; // The mass of the ship, which affects acceleration and deceleration

    public Vector2 currentVelocity; // The current velocity of the ship
    private Vector2 totalForce; // The total force exerted by the mast blocks

    void Update()
    {
        // Calculate the total force from mast blocks
        CalculateTotalForce();

        // Update the current velocity based on the total force
        UpdateVelocity();

        MoveShip();


    }


    void CalculateTotalForce()
    {
        totalForce = Vector2.zero;

        foreach (GameObject mastBlock in SingletonManager.Instance.shipGenerator.mastBlocks)
        {
            blockPrefabScript blockScript = mastBlock.GetComponent<blockPrefabScript>();
            if (blockScript.active)
            {
                totalForce += blockScript.blockDirection.ToVector2().normalized * blockScript.GetBlockValueByName("mastForce");

                maxSpeed = totalForce.magnitude * kFactor;
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

    void MoveShip()
    {
        // Calculate the movement vector
        Vector3 movement = new Vector3(currentVelocity.x, currentVelocity.y, 0) * Time.deltaTime;

        // Apply the movement to the tilemap
        shipTilemap.transform.position += movement;

    }

    public void ApplyRecoilForce(Vector2 recoilForce)
    {
        // Apply the recoil force to the current velocity
        currentVelocity += recoilForce / mass;
        // Ensure the velocity doesn't exceed the maximum speed
        currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxSpeed);
    }
}
