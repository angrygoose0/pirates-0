using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToTarget : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        RotateTowardsMouse();
        AdjustScale();
    }

    void RotateTowardsMouse()
    {
        // Get the mouse position in screen coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the mouse position to world coordinates
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0; // Assuming a 2D game, you might need to adjust this for a 3D game

        // Calculate the direction from the creature to the mouse position
        Vector3 directionToMouse = mouseWorldPosition - transform.position;

        // Calculate the angle between the current forward direction and the direction to the mouse
        float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

        // Apply the rotation with a 90 degrees anticlockwise offset
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 225));
    }

    void AdjustScale()
    {
        // Get the mouse position in screen coordinates
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convert the mouse position to world coordinates
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0; // Assuming a 2D game, you might need to adjust this for a 3D game

        // Calculate the direction from the creature to the mouse position
        Vector3 directionToMouse = mouseWorldPosition - transform.position;

        // Adjust scale based on the quadrant
        if (directionToMouse.x >= 0 && directionToMouse.y >= 0)
        {
            // Right top
            transform.localScale = new Vector3(1, -1, 1);
        }
        else if (directionToMouse.x >= 0 && directionToMouse.y < 0)
        {
            // Right bottom
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (directionToMouse.x < 0 && directionToMouse.y < 0)
        {
            // Left bottom
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (directionToMouse.x < 0 && directionToMouse.y >= 0)
        {
            // Left top
            transform.localScale = new Vector3(-1, -1, 1);
        }
    }
}
