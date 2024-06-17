using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CreatureMovement : MonoBehaviour
{
    public Vector2Int spawnTilePosition; // Define the tile to spawn the creature on
    public Tilemap tilemap; // Reference to the tilemap

    // Start is called before the first frame update
    void Start()
    {
        // Convert the tile position to world position and set the creature's position
        Vector3 worldPosition = tilemap.GetCellCenterWorld((Vector3Int)spawnTilePosition);
        transform.position = worldPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0; // Ensure the z position is zero since we are in 2D

        Vector3Int mouseTilePosition = tilemap.WorldToCell(mouseWorldPosition);

        Vector3 tileWorldCenter = tilemap.GetCellCenterWorld(mouseTilePosition);
        Vector3 direction = tileWorldCenter - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Determine if flipping is needed
        bool shouldFlip = (direction.y < 0 && direction.x > 0) || (direction.y > 0 && direction.x < 0);

        // Apply flipping by scaling
        transform.localScale = new Vector3(1, shouldFlip ? -1 : 1, 1);

        // Apply the angle offset and adjust for flipping
        transform.rotation = Quaternion.Euler(0, 0, angle + 135 + (shouldFlip ? 90 : 0));
    }
}
