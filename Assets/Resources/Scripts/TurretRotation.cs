using UnityEngine;

public class TurretRotation : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        RotateTurretTowardsMouse();
    }

    void RotateTurretTowardsMouse()
    {
        // Get the world position of the mouse cursor
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction from the turret to the mouse position
        Vector3 direction = mousePos - transform.position;

        // Calculate the angle between the turret and the mouse position
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Offset the angle by 90 degrees because the turret sprite is facing up by default
        angle -= 90f;

        // Apply the rotation to the turret
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Flip the sprite if facing right
        if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
