using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CannonBehaviour : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject selectorPrefab;
    public GameObject explosionEffectPrefab;
    public GameObject cannonballPrefab; // Add a prefab for the cannonball


    public float cannonForce = 20f; // Adjust the force applied to the cannonball
    public float cannonballMass = 1f; // Mass of the cannonball
    public float gravity = -9.81f; // Gravity affecting the cannonball

    private Vector3Int previousMousePosition = new Vector3Int();
    public GameObject selectorInstantiated;
    private Vector3 previousTilemapPosition;
    public Vector3Int currentMouseTileCoordinate; // Variable to store the mouse's tile coordinate

    void Start()
    {
        previousTilemapPosition = tilemap.transform.position;

    }

    public void cannonSelector()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);

        // Update the current mouse tile coordinate
        currentMouseTileCoordinate = cellPosition;

        // Check if the mouse position has changed or if the tilemap has moved
        if (cellPosition != previousMousePosition || tilemap.transform.position != previousTilemapPosition)
        {
            previousMousePosition = cellPosition;
            previousTilemapPosition = tilemap.transform.position;

            if (selectorInstantiated == null)
            {
                selectorInstantiated = Instantiate(selectorPrefab);
            }

            // Convert cell position to world position with proper alignment
            Vector3 cellWorldPosition = tilemap.GetCellCenterWorld(cellPosition);
            cellWorldPosition.z = 0; // Ensure Z coordinate is correct

            selectorInstantiated.transform.position = cellWorldPosition;
        }
    }

    public void FireInTheHole(Vector3 startCoordinate, Vector3Int endTileCoordinate, ProjectileData projectile, float mass)
    {

        // Calculate the direction of the shot
        Vector3 end = tilemap.GetCellCenterWorld(endTileCoordinate);
        Vector3 shotDirection = (end - startCoordinate).normalized;

        // Apply recoil to the ship
        ApplyRecoil(shotDirection);

        // Start the coroutine to move the cannonball and show explosion
        StartCoroutine(MoveCannonball(startCoordinate, endTileCoordinate, projectile, mass));
    }

    private void ApplyRecoil(Vector3 shotDirection)
    {
        // Calculate the recoil force
        Vector3 recoilForce = -shotDirection * cannonForce;

        // Apply the recoil force to the ship's velocity
        //SingletonManager.Instance.shipMovement.ApplyRecoilForce(new Vector2(recoilForce.x, recoilForce.y));
    }

    private IEnumerator MoveCannonball(Vector3 start, Vector3Int endTile, ProjectileData projectile, float mass)
    {

        // Instantiate the cannonball at the start position
        GameObject cannonball = Instantiate(cannonballPrefab, start, Quaternion.identity);

        SpriteRenderer spriteRenderer = cannonballPrefab.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = projectile.sprite;

        Rigidbody2D rb = cannonball.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0; // We'll handle gravity manually

        Vector3 initialEnd = tilemap.GetCellCenterWorld(endTile);
        float initialDistance = Vector3.Distance(start, initialEnd);

        float timeToTarget = Mathf.Sqrt(2 * initialDistance / (cannonForce / mass));

        // Calculate initial velocity components


        Vector3 displacement = initialEnd - start;

        float initialVelocityX = displacement.x / timeToTarget;
        float initialVelocityY = (displacement.y - 0.5f * gravity * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        rb.velocity = new Vector2(initialVelocityX, initialVelocityY);

        // Simulate the cannonball flight
        float elapsedTime = 0f;

        while (elapsedTime < timeToTarget)
        {
            elapsedTime += Time.deltaTime;

            // Recalculate the target position based on the tilemap's current position
            Vector3 currentEnd = tilemap.GetCellCenterWorld(endTile);

            // Adjust the velocity to account for the tilemap movement
            displacement = currentEnd - cannonball.transform.position;

            float remainingTime = timeToTarget - elapsedTime;

            if (remainingTime > 0)
            {
                rb.velocity = new Vector2(
                    displacement.x / remainingTime,
                    (displacement.y - 0.5f * gravity * Mathf.Pow(remainingTime, 2)) / remainingTime
                );
            }

            // Apply gravity manually
            rb.velocity += new Vector2(0, gravity) * Time.deltaTime;

            yield return null;
        }

        // Destroy the cannonball
        Destroy(cannonball);

        // Ensure Z coordinate is correct
        Vector3 finalPosition = tilemap.GetCellCenterWorld(endTile);
        finalPosition.z = 0;

        // Simulate explosion using raycasts
        SingletonManager.Instance.explosions.Explode(finalPosition, projectile, 0f, 360f);


    }


    public Vector3 GetSelectorPosition()
    {
        if (selectorInstantiated != null)
        {
            return selectorInstantiated.transform.position;
        }
        else
        {
            // Return a default value or throw an exception
            return Vector3.zero; // Or throw new Exception("Selector is not instantiated.");
        }
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return tilemap.WorldToCell(worldPosition);
    }
}