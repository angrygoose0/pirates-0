using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingHook : MonoBehaviour
{

    float speed = 0f; // Start speed, adjust as necessary
    float maxSpeed = 50f; // Maximum speed it can accelerate to
    float acceleration = 50f; // How quickly the item will speed up

    public Transform playerTransform;

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("touched someting");
        if (other.CompareTag("Item"))
        {
            Debug.Log("touched item");
            StartCoroutine(PullToPlayer(playerTransform, other.gameObject));
        }
    }

    private IEnumerator PullToPlayer(Transform playerTransform, GameObject itemObject)
    {
        while (Vector3.Distance(itemObject.transform.position, playerTransform.position) > 0.1f)
        {
            // Calculate the distance between the item and the player
            float distance = Vector3.Distance(itemObject.transform.position, playerTransform.position);

            // Gradually increase speed, with a cap on maximum speed
            speed = Mathf.Min(speed + acceleration * Time.deltaTime, maxSpeed);

            // Calculate a time-based interpolation factor for smooth movement
            float step = speed * Time.deltaTime / distance; // Normalized movement factor

            // Lerp the item's position towards the player's position using the step
            itemObject.transform.position = Vector3.Lerp(itemObject.transform.position, playerTransform.position, step);

            // Wait for the next frame

            yield return null;
        }

        ItemScript itemScript = itemObject.GetComponent<ItemScript>();
        itemScript.beingReeled = false;
    }
}