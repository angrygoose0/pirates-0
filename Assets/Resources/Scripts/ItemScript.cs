using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public ItemObject itemObject;
    private ProjectileData projectile = null;
    public bool activeCooldown = true;

    public bool isActive;
    public GameObject targetObject; // The GameObject towards which the item will lerp
    public float lerpDuration; // Duration of the lerp movement
    public float inactiveTimer;

    private float inactiveTime = 0f;
    private Coroutine fadeCoroutine;
    private Coroutine lerpCoroutine; // Coroutine for lerping
    public bool itemTaken = false;
    public bool itemPickupable;

    void Start()
    {
        if (itemObject.projectileData != null && itemObject.projectileData.Count == 1)
        {
            projectile = itemObject.projectileData[0];
        }

        itemPickupable = true;

        // Automatically find and assign the GameObject named "ghost" as the target
        targetObject = GameObject.Find("ghost");



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
        if (itemObject != null && projectile == null)
        {
            StartCoroutine(StartLerpingAfterDelay(2f, itemObject.name == "Gold", itemObject.name == "HealingOrb")); // Start lerping after a 2-second delay
        }
    }



    public void SetCollider(bool boolean)
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = boolean;
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
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;           // Reset linear velocity
                rb.angularVelocity = Vector3.zero;    // Reset angular velocity
            }

            transform.parent = newParent.transform;
            transform.localPosition = Vector3.zero;


            transform.parent = newParent.transform;
            transform.localPosition = new Vector3(0, 0.25f, 0);

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
        yield return new WaitForSeconds(inactiveTimer);

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
    private IEnumerator StartLerpingAfterDelay(float delay, bool gold, bool heal)
    {
        yield return new WaitForSeconds(delay);
        if (targetObject != null)
        {
            lerpCoroutine = StartCoroutine(LerpTowardsTarget(gold, heal));
        }
    }

    // Coroutine to lerp the item towards the target object
    private IEnumerator LerpTowardsTarget(bool gold, bool heal)
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
            SingletonManager.Instance.goldManager.AddGold(1);

            // Destroy the GameObject after lerping
            Destroy(gameObject);
        }

        if (heal)
        {
            Debug.Log("heal");

            // Destroy the GameObject after lerping
            Destroy(gameObject);
        }

        NewParent(null);
    }

    // Coroutine to handle active skill cooldown
    public void ActivateActive(Vector3 position, Vector2 blockDirection)
    {
        if (itemObject.active == null)
        {
            return;
        }

        if (activeCooldown == false)
        {
            return;
        }

        itemObject.UseActive(position, blockDirection);
        StartCoroutine(ActiveCooldownCoroutine(itemObject.active.cooldown));
    }

    private IEnumerator ActiveCooldownCoroutine(float cooldown)
    {
        activeCooldown = false;
        yield return new WaitForSeconds(5f);
        activeCooldown = true;
    }
}
