using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    public ItemObject itemObject;
    private ProjectileData projectile = null;
    public bool activeCooldown = true;
    public SpriteRenderer spriteRenderer;

    public bool isActive;
    public GameObject targetObject; // The GameObject towards which the item will lerp
    public float lerpDuration; // Duration of the lerp movement
    public float inactiveTimer;

    private float inactiveTime = 0f;
    private Coroutine fadeCoroutine;
    private Coroutine lerpCoroutine; // Coroutine for lerping
    public bool itemTaken = false;
    public bool itemPickupable;
    public bool beingReeled = false;
    public bool onPayload = false;

    void Start()
    {
        if (itemObject.projectileData != null && itemObject.projectileData.Count == 1)
        {
            projectile = itemObject.projectileData[0];
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (itemObject != null)
        {
            spriteRenderer.sprite = itemObject.itemSprite;
        }

        itemPickupable = true; //

        // Automatically find and assign the GameObject named "ghost" as the target
        targetObject = GameObject.Find("ghost");

        glowLight = GetComponentInChildren<Light2D>();
        glowLight.color = itemObject.glowColor;
        glowLight.intensity = itemObject.glowIntensity;

        StartCoroutine(PulseLight(glowLight));

        // Check if the GameObject has a parent on startup and that parent is not the world Tilemap
        if (transform.parent != null && transform.parent.name != "world")
        {
            isActive = true;
        }
        else
        {
            isActive = false; //
            StartInactiveTimer();
        }

    }

    public float bobbingSpeed = 2f; // Speed of bobbing
    public float bobbingHeight = 0.5f; // Height of bobbing
    private Light2D glowLight;
    void Update()
    {
        if (onPayload)
        {
            float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            spriteRenderer.gameObject.transform.localPosition = new Vector3(0f, newY, 0f);
        }

    }

    IEnumerator PulseLight(Light2D lightComponent)
    {
        // Set the range for intensity (20% down and 20% up from base)
        float minIntensity = itemObject.glowIntensity * (1 - itemObject.glowAmplitude);
        float maxIntensity = itemObject.glowIntensity * (1 + itemObject.glowAmplitude);

        while (true)
        {
            // Lerp between min and max intensity using Mathf.PingPong
            float t = Mathf.PingPong(Time.time * itemObject.pulseSpeed, 1.0f);
            lightComponent.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

            // Wait until the next frame
            yield return null;
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



    // Coroutine to handle active skill cooldown
    public void ActivateActive(Vector3 position, Direction blockDirection, Vector3 feedbackPosition)
    {
        if (itemObject.activeAbility == null)
        {
            return;
        }

        if (activeCooldown == false)
        {
            return;
        }

        float multiplier = 1f;
        SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(feedbackPosition, multiplier);

        itemObject.UseActive(position, blockDirection);
        StartCoroutine(ActiveCooldownCoroutine(itemObject.activeAbility.cooldown));
    }

    private IEnumerator ActiveCooldownCoroutine(float cooldown)
    {
        activeCooldown = false;
        yield return new WaitForSeconds(cooldown);
        activeCooldown = true;
    }
}
