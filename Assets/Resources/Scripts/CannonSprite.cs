using UnityEngine;
using UnityEngine.Tilemaps;

public class CannonSprite : MonoBehaviour
{
    public Tilemap tilemap;
    public LineRenderer lineRenderer;
    public Vector3 mouseWorldPos;
    public Vector3Int cellPosition;

    void Start()
    {
        // Get or Add the LineRenderer component
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Optionally configure the LineRenderer
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2; // Two points: start and end
    }

    void Update()
    {
        // Step 1: Get mouse position in world space
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Make sure Z is 0 for 2D

        // Step 2: Convert world position to tilemap cell position
        cellPosition = tilemap.WorldToCell(mouseWorldPos);

        // Step 3: Get the world position of the cell
        Vector3 cellWorldPos = tilemap.GetCellCenterWorld(cellPosition);

        // Step 4: Update the LineRenderer positions
        lineRenderer.SetPosition(0, transform.position); // Start position (cannon's position)
        lineRenderer.SetPosition(1, cellWorldPos); // End position (tile's center position)
    }
}
