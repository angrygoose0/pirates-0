using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public ItemObject itemObject;
    public bool activeCooldown = true;

    public bool isActive;
    public GameObject targetObject; // The GameObject towards which the item will lerp
    public float lerpDuration; // Duration of the lerp movement

    public GoldManager goldManager;

    private float inactiveTime = 0f;
    private Coroutine fadeCoroutine;
    private Coroutine lerpCoroutine; // Coroutine for lerping
    public bool itemTaken = false;
    public bool itemPickupable;

    void Start()
    {
        itemPickupable = true;

        // Automatically find and assign the GameObject named "ghost" as the target
        targetObject = GameObject.Find("ghost");

        goldManager = targetObject.GetComponent<GoldManager>();

        // Check if the GameObject has a parent on startup and that parent is not the world Tilemap
        if (transform.parent != null && transform.parent.name != "world")
        {
            isActive = true;
        }
        else
        {
            isActive = false;
            StartInactiveTimer();
        }

        // If the itemObject is gold, start the lerping coroutine
        if (itemObject != null && itemObject.ammoCount == 0)
        {
            StartCoroutine(StartLerpingAfterDelay(2f, itemObject.name == "Gold")); // Start lerping after a 2-second delay
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
            isActive = newParent.name != "world"; // Set isActive based on the new parent
            if (isActive)
            {
                StopInactiveTimer();
            }
            else
            {
                StartInactiveTimer();
            }
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
        yield return new WaitForSeconds(10);

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

    // Coroutine to start lerping after a delay
    private IEnumerator StartLerpingAfterDelay(float delay, bool gold)
    {
        yield return new WaitForSeconds(delay);
        if (targetObject != null)
        {
            lerpCoroutine = StartCoroutine(LerpTowardsTarget(gold));
        }
    }

    // Coroutine to lerp the item towards the target object
    private IEnumerator LerpTowardsTarget(bool gold)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetObject.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < lerpDuration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the item reaches the target position
        transform.position = endPosition;

        if (gold)
        {
            goldManager.AddGold(1);

            // Destroy the GameObject after lerping
            Destroy(gameObject);
        }

        NewParent(null);
    }

    // Coroutine to handle active skill cooldown
    public void ActivateActive(Vector3 position)
    {
        if (itemObject.active == null)
        {
            return;
        }

        if (activeCooldown == false)
        {
            return;
        }

        itemObject.UseActive(position);
        StartCoroutine(ActiveCooldownCoroutine(itemObject.active.cooldown));
    }

    private IEnumerator ActiveCooldownCoroutine(float cooldown)
    {
        activeCooldown = false;
        yield return new WaitForSeconds(5f);
        activeCooldown = true;
    }
}
