using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Tilemaps;

public class ShipMovement : MonoBehaviour
{
    public GameObject tilemap; // Reference to the tilemap or parent object containing the tilemap
    public float kFactor; // constant that multiplies with totalForce to give maxSpeed;
    private float maxSpeed; // The maximum speed at which the ship moves
    public float mass = 1f; // The mass of the ship, which affects acceleration and deceleration

    public Vector2 currentVelocity; // The current velocity of the ship
    private Vector2 totalForce; // The total force exerted by the mast blocks
    private TilemapRenderer tilemapRenderer;
    private Material tilemapMaterial;

    void Start()
    {
        tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
        tilemapMaterial = tilemapRenderer.material;
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


    void CalculateTotalForce()
    {
        totalForce = Vector2.zero;

        foreach (GameObject mastBlock in SingletonManager.Instance.shipGenerator.mastBlocks)
        {
            blockPrefabScript blockScript = mastBlock.GetComponent<blockPrefabScript>();
            if (blockScript.active)
            {
                totalForce += blockScript.blockDirection.normalized * blockScript.GetBlockValueByName("mastForce");

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

    void MoveTilemap()
    {
        // Calculate the movement vector
        Vector3 movement = new Vector3(currentVelocity.x, currentVelocity.y, 0) * Time.deltaTime;

        // Apply the movement to the tilemap
        tilemap.transform.position -= movement;

        Vector2 tilemapPosition = (Vector2)tilemap.transform.position;
        Vector2 inverseTilemapPosition = new Vector2(-tilemapPosition.x, -tilemapPosition.y);

        // Set the _TilemapPosition in the material to this inverse position
        tilemapMaterial.SetVector("_TilemapPosition", inverseTilemapPosition);
    }

    public void ApplyRecoilForce(Vector2 recoilForce)
    {
        // Apply the recoil force to the current velocity
        currentVelocity += recoilForce / mass;
        // Ensure the velocity doesn't exceed the maximum speed
        currentVelocity = Vector2.ClampMagnitude(currentVelocity, maxSpeed);
    }
}
