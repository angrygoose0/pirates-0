using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

public class ShipMovement : MonoBehaviour
{
    public GameObject shipTilemap; // Reference to the tilemap or parent object containing the tilemap
    public float kFactor; // constant that multiplies with totalForce to give maxSpeed;
    public float maxSpeed; // The maximum speed at which the ship moves
    public float mass = 1f; // The mass of the ship, which affects acceleration and deceleration

    public Material backgroundMaterial;

    public Vector2 currentVelocity; // The current velocity of the ship
    private Vector2 totalForce; // The total force exerted by the mast blocks

    void Update()
    {
        // Calculate the total force from mast blocks
        CalculateTotalForce();

        // Update the current velocity based on the total force
        UpdateVelocity();

        MoveShip();

        backgroundMaterial.SetVector("_ShipOffset", shipTilemap.transform.position);


    }


    void CalculateTotalForce()
    {
        totalForce = Vector2.zero;

        //rework movement system, with certain items in payloads giving movement instead of mast blocks
        /*
        foreach (GameObject mastBlock in SingletonManager.Instance.shipGenerator.mastBlocks)
        {
            BlockData blockData = SingletonManager.Instance.blockManager.blockDictionary[mastBlock];
            if (blockData.active)
            {
                totalForce += blockData.blockDirection.ToVector2().normalized * blockData.GetBlockValueByName("mastForce");

                maxSpeed = totalForce.magnitude * kFactor;
            }
        }
        */
    }


    public Vector2 targetVelocity = new Vector2(2, 0); // Target velocity to decelerate to
    private Vector2 velocityChange = Vector2.zero; // This ref value will track velocity changes in SmoothDamp
    public float smoothTime = 0.3f; // The time it takes to decelerate (higher values slow down deceleration)

    void UpdateVelocity()
    {
        if (totalForce != Vector2.zero)
        {
            // Calculate the acceleration based on the total force and mass
            Vector2 acceleration = totalForce / mass;
            currentVelocity = Vector2.ClampMagnitude(currentVelocity + acceleration * Time.deltaTime, maxSpeed);

        }
        if (currentVelocity != Vector2.zero)
        {
            // Smoothly decelerate to the target velocity
            currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref velocityChange, smoothTime);
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
    }
}
