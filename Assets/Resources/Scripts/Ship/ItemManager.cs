using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Assuming you're using Universal Render Pipeline

public class ItemData
{
    public ItemObject itemObject;

    public float abilityActiveCooldown; // 0 means user can use ability.
    public bool itemTaken = false;
    public bool itemPickupable = true;
    public bool beingReeled = false;
    public bool onPayload = false;

    public bool isActive;
    public float inactiveTime = 0f;
    public Coroutine fadeCoroutine;
    // Store the GameObject reference
    private GameObject itemGameObject;

    // Constructor to initialize itemObject and itemGameObject
    public ItemData(ItemObject itemObject, GameObject itemGameObject)
    {
        this.itemObject = itemObject;
        this.itemGameObject = itemGameObject;
    }
    public UnityEngine.Rendering.Universal.Light2D Light => itemGameObject.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
    public SpriteRenderer spriteRenderer => itemGameObject.GetComponent<SpriteRenderer>();
    private Collider2D collider => itemGameObject.GetComponent<Collider2D>();
    public void SetCollider(bool boolean)
    {
        collider.enabled = boolean;
    }

    public void NewParent(Transform parentTransform)
    {
        itemGameObject.transform.SetParent(parentTransform);
    }
}



public class ItemManager : MonoBehaviour
{

    public Dictionary<GameObject, ItemData> itemDictionary = new Dictionary<GameObject, ItemData>();
    public GameObject itemPrefab;
    public List<ItemObject> itemObjects;

    public float bobbingSpeed = 2f; // Speed of bobbing
    public float bobbingHeight = 0.5f; // Height of bobbing

    private GameObject worldTilemap;

    public float inactivityThreshold = 30f;


    public float fadeDuration = 5f; // Duration of the fading period
    public float fadeTime = 0f;
    public float fadeSpeed = 0.5f;
    void Start()
    {
        worldTilemap = GameObject.Find("world");
    }

    void Update()
    {
        foreach (KeyValuePair<GameObject, ItemData> entry in itemDictionary)
        {
            if (entry.Value.onPayload)
            {
                HoverItem(entry.Key);
            }

        }
    }

    IEnumerator PulseLights()
    {
        while (true)
        {
            foreach (KeyValuePair<GameObject, ItemData> entry in itemDictionary)
            {
                ItemObject itemObject = entry.Value.itemObject;

                // Set the range for intensity (20% down and 20% up from base)
                float minIntensity = itemObject.glowIntensity * (1 - itemObject.glowAmplitude);
                float maxIntensity = itemObject.glowIntensity * (1 + itemObject.glowAmplitude);

                // Calculate the intensity using Mathf.PingPong
                float t = Mathf.PingPong(Time.time * itemObject.pulseSpeed, 1.0f);
                float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);

                // Set the light intensity on the item's light component
                Light lightComponent = entry.Key.GetComponent<Light>();
                if (lightComponent != null)
                {
                    lightComponent.intensity = intensity;
                }
            }

            // Wait until the next frame to update all items again
            yield return null;
        }
    }

    private IEnumerator CheckItemsInactivity()
    {
        while (true)
        {
            foreach (KeyValuePair<GameObject, ItemData> entry in itemDictionary)
            {
                ItemData itemData = entry.Value;

                if (itemData.inactiveTime >= inactivityThreshold)
                {
                    itemData.fadeCoroutine = StartCoroutine(FadeAndDestroy(entry.Key));
                }

                if (!itemData.isActive)
                {
                    itemData.inactiveTime += 0.01f;
                }

                if (itemData.abilityActiveCooldown != 0)
                {
                    itemData.abilityActiveCooldown -= 0.01f;
                }
            }
            yield return new WaitForSeconds(0.01f);  // Check every second
        }
    }



    private IEnumerator FadeAndDestroy(GameObject itemGameObject)
    {
        SpriteRenderer spriteRenderer = itemGameObject.GetComponent<SpriteRenderer>();


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

    public void ItemIsActive(ItemData itemData)
    {
        itemData.isActive = true;
        // Stop the fading coroutine if it's running
        if (itemData.fadeCoroutine != null)
        {
            StopCoroutine(itemData.fadeCoroutine);
            itemData.fadeCoroutine = null;
        }

        Color currentColor = itemData.spriteRenderer.color;
        itemData.spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, 1.0f);
    }


    public void HoverItem(GameObject itemGameObject)
    {
        float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        itemGameObject.transform.localPosition = new Vector3(0f, newY, 0f);
    }



    public void ActivateActive(GameObject itemGameObject, Vector3 position, Direction blockDirection, Vector3 feedbackPosition)
    {

        if (itemDictionary.TryGetValue(itemGameObject, out ItemData itemData))

            if (itemData.itemObject.activeAbility == null)
            {
                return;
            }

        if (itemData.abilityActiveCooldown != 0f)
        {
            return;
        }

        float multiplier = 1f;
        SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(feedbackPosition, multiplier);

        itemData.itemObject.UseActive(position, blockDirection);
        itemData.abilityActiveCooldown = itemData.itemObject.activeAbility.cooldown;
    }

    public void DisableItemRigidBody(GameObject itemGameObject)
    { }

    public void NewParent(GameObject itemGameObject, GameObject parentGameObject)
    { }

    public void PlaceItemOnBlock(GameObject itemGameObject, GameObject blockGameObject)
    {
        if (itemDictionary.TryGetValue(itemGameObject, out ItemData itemData))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;           // Reset linear velocity
                rb.angularVelocity = Vector3.zero;    // Reset angular velocity
            }

            itemData.NewParent(blockGameObject.transform);
            itemGameObject.transform.localPosition = new Vector3(0, 0.25f, 0);

            ItemIsActive(itemData);
        }
    }
    public GameObject CreateItem(ItemObject createItemObject, Vector3 position, Transform parentTransform)
    {
        GameObject createdItemGameObject = Instantiate(itemPrefab, position, Quaternion.identity, parentTransform);

        ItemData itemData = new ItemData(createItemObject, createdItemGameObject);
        itemDictionary[createdItemGameObject] = itemData;
        //itemData.spriteRenderer.sprite = createItemObject.itemSprite;

        return createdItemGameObject;
    }
    /*
        Rigidbody2D rigidbody2D = createdItem.GetComponent<Rigidbody2D>();
        if (rigidbody2D != null)
        {
            // Apply a random force to the rigidbody to simulate an explosion
            float forceMagnitude = 100f; // Adjust the magnitude of the force as needed
            Vector2 forceDirection = Random.insideUnitCircle.normalized; // Random direction

            // Adjust the y component to be half of the x component for isometric perspective
            forceDirection.y *= 0.5f;
            rigidbody2D.mass = createItemObject.mass;
            rigidbody2D.AddForce(forceDirection * forceMagnitude);
        }
    */


    public float itemBounceForce;
    public float gravity = -9.81f;
    public GameObject itemEffectPrefab;

    public void StartItemBounce(ItemObject itemObject, Vector3 startPosition, Vector3 endPosition, Transform parentTransform)
    {
        StartCoroutine(ItemBounce(itemObject, startPosition, endPosition, parentTransform, 10f));
    }
    private IEnumerator ItemBounce(ItemObject itemObject, Vector3 startPosition, Vector3 endPosition, Transform parentTransform, float firingForce)
    {
        GameObject craftedItemObject = SingletonManager.Instance.itemManager.CreateItem(itemObject, startPosition, parentTransform);

        Vector3 localStartPosition = parentTransform.InverseTransformPoint(startPosition);
        Vector3 localEndPosition = parentTransform.InverseTransformPoint(endPosition);

        ItemData craftedItemData = SingletonManager.Instance.itemManager.itemDictionary[craftedItemObject];
        craftedItemData.itemPickupable = false;

        Color newColor = craftedItemData.spriteRenderer.color;
        newColor.a = 0f;
        craftedItemData.spriteRenderer.color = newColor;

        craftedItemData.spriteRenderer.enabled = false;

        GameObject itemEffectObject = Instantiate(itemEffectPrefab, startPosition, Quaternion.identity, parentTransform);

        // Get the LineRenderer component
        LineRenderer lineRenderer = itemEffectObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer not found on linePrefab.");
            yield break;
        }

        // Initialize LineRenderer
        List<Vector3> linePoints = new List<Vector3> { startPosition };

        Rigidbody2D rb = itemEffectObject.GetComponent<Rigidbody2D>();

        float initialDistance = Vector3.Distance(localStartPosition, localEndPosition);
        float timeToTarget = Mathf.Sqrt(2 * initialDistance / (firingForce));

        Vector3 displacement = localEndPosition - localStartPosition;

        float initialVelocityX = displacement.x / timeToTarget;
        float initialVelocityY = (displacement.y - 0.5f * gravity * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        rb.velocity = new Vector2(initialVelocityX, initialVelocityY);

        float elapsedTime = 0f;

        // Simulate the line flight to the end position
        while (elapsedTime < timeToTarget)
        {
            elapsedTime += Time.deltaTime;

            displacement = localEndPosition - itemEffectObject.transform.localPosition;

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

            // Add the current position of the line to the line points
            linePoints.Add(parentTransform.TransformPoint(itemEffectObject.transform.localPosition));
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());

            yield return null;
        }
        Destroy(itemEffectObject);

        craftedItemObject.transform.localPosition = localEndPosition;
        ItemData itemData = SingletonManager.Instance.itemManager.itemDictionary[craftedItemObject];
        itemData.itemPickupable = true;

        newColor.a = 1f;
        itemData.spriteRenderer.color = newColor;

    }


}
