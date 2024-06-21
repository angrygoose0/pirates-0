using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public ItemObject itemObject;
    public bool isActive;

    private float inactiveTime = 0f;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Check if the GameObject has a parent on startup
        if (transform.parent != null)
        {
            isActive = true;
        }
        else
        {
            isActive = false;
            StartInactiveTimer();
        }
    }

    // Method to toggle the item's visibility and interactivity
    public void SetItemVisibility(bool isVisible)
    {
        // Toggle the SpriteRenderer to make the item visible or invisible
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = isVisible;
        }

        // Toggle the Collider2D to make the item interactable or non-interactable
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = isVisible;
        }
    }

    // Method to unparent the GameObject
    public void UnparentGameObject()
    {
        transform.parent = null;
        isActive = false; // Set isActive to false when unparented
        StartInactiveTimer();
    }

    // Method to set a new parent for the GameObject
    public void NewParent(GameObject newParent)
    {
        if (newParent != null)
        {
            transform.parent = newParent.transform;
            isActive = true; // Set isActive to true when parented
            StopInactiveTimer();
        }
        else
        {
            // If the newParent is null, unparent the GameObject
            UnparentGameObject();
        }
    }

    // Start the timer for inactivity
    private void StartInactiveTimer()
    {
        inactiveTime = Time.time;
        if (fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeAndDestroy());
        }
    }

    // Stop the timer for inactivity
    private void StopInactiveTimer()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    // Coroutine to handle fading and destruction
    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            yield break;
        }

        // Wait for one minute of inactivity
        yield return new WaitForSeconds(5);

        float fadeDuration = 5f; // Duration of the fading period
        float fadeTime = 0f;
        float fadeSpeed = 0.5f;
        Color originalColor = spriteRenderer.color;

        while (fadeTime < fadeDuration)
        {
            float alpha = Mathf.PingPong(Time.time * fadeSpeed, 1f);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;

            fadeTime += Time.deltaTime;
            // Increase the fading speed over time
            fadeSpeed = Mathf.Min(10f, fadeSpeed + Time.deltaTime);
        }

        // Destroy the GameObject after fading
        Destroy(gameObject);
    }
}
