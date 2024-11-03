using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
// Assuming you're using Universal Render Pipeline

public class ItemData
{
    public ItemObject itemObject;

    public float abilityActiveCooldown; // 0 means user can use ability.
    public bool itemTaken = false;
    public bool itemPickupable = true;
    public bool beingReeled = false;
    public bool onPayload = false;

    public bool isActive = false;
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
    public SpriteRenderer spriteRenderer => itemGameObject.GetComponentInChildren<SpriteRenderer>();
    private Collider2D collider => itemGameObject.GetComponent<Collider2D>();
    public void SetCollider(bool boolean)
    {
        collider.enabled = boolean;
    }

    public void NewParent(Transform parentTransform)
    {
        itemGameObject.transform.SetParent(parentTransform);

        if (parentTransform.GetComponent<Tilemap>() != null) // ship or sea
        {
            isActive = false;
            this.SetCollider(true);
        }
        else // not a tilemap
        {
            SingletonManager.Instance.itemManager.ItemIsActive(this);
            this.SetCollider(false);
        }
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
        StartCoroutine(PulseLights());
        StartCoroutine(GlobalCoroutine());
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
        // Loop through number keys 0-9
        for (int i = 0; i <= 9; i++)
        {
            // Check if the key corresponding to the current number is pressed
            if (Input.GetKeyDown(i.ToString()))
            {
                CreateItem(itemObjects[i], Vector3.zero, SingletonManager.Instance.shipGenerator.shipTilemap.transform);
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

    private IEnumerator GlobalCoroutine()
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
        if (itemDictionary.TryGetValue(itemGameObject, out ItemData itemData))
        {
            Color originalColor = itemData.spriteRenderer.color;

            while (fadeTime < fadeDuration)
            {
                float alpha = Mathf.PingPong(Time.time * fadeSpeed, 1f);
                itemData.spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;

                fadeTime += Time.deltaTime;
                // Increase the fading speed over time
                fadeSpeed = Mathf.Min(10f, fadeSpeed + Time.deltaTime);
            }

            // remove item
            RemoveItem(itemGameObject);
        }
    }

    public void ItemIsActive(ItemData itemData)
    {
        itemData.isActive = true;
        itemData.inactiveTime = 0f;
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
        float newY = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight + 0.25f;
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

    public void PlaceItemOnBlock(GameObject itemGameObject, GameObject blockGameObject)
    {
        if (itemDictionary.TryGetValue(itemGameObject, out ItemData itemData))
        {
            if (SingletonManager.Instance.blockManager.blockDictionary.TryGetValue(blockGameObject, out BlockData blockData))
            {
                Rigidbody rb = itemGameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;           // Reset linear velocity
                    rb.angularVelocity = Vector3.zero;    // Reset angular velocity
                }
                if (blockData.blockObject.blockType == BlockType.Payload)
                {
                    if (itemData.itemObject.projectile.Count > 0) // item is projectile
                    {
                        return;
                    }

                    blockData.itemsInBlock.Add(itemGameObject);
                    itemData.NewParent(blockGameObject.transform);
                    itemGameObject.transform.localPosition = new Vector3(0, 0.25f, 0);
                    itemData.onPayload = true;
                    itemData.itemPickupable = false;

                    if (itemData.itemObject.spawningItem != null)
                    {
                        itemData.spriteRenderer.sprite = itemData.itemObject.spawningItem.itemSprite;
                        Color newColor = itemData.spriteRenderer.color;
                        newColor.a = 0.5f;
                        itemData.spriteRenderer.color = newColor;
                    }

                    blockGameObject.GetComponentInChildren<Light2D>().intensity = 1.5f;

                    float multiplier = 1f;
                    Vector3 newBlockPosition = blockGameObject.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                    SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(newBlockPosition, multiplier);

                    SingletonManager.Instance.shipGenerator.UpdateBlockEffects();

                    if (itemData.itemObject.affectsCannons)
                    {
                        SingletonManager.Instance.shipGenerator.MakeTrailEffects(blockGameObject.transform);
                    }

                    SingletonManager.Instance.blockManager.CanCraftNew(blockGameObject);
                }
                else if (blockData.blockObject.blockType == BlockType.Cannon)
                {
                    if (itemData.itemObject.projectile.Count != 1) // item is not projectile
                    {
                        return;
                    }
                    if (blockData.itemsInBlock.Count == 0)
                    {
                        blockData.itemsInBlock.Add(itemGameObject);
                    }
                    else
                    {
                        Destroy(blockData.itemsInBlock[0]);
                        blockData.itemsInBlock[0] = itemGameObject;
                    }

                    itemData.NewParent(blockGameObject.transform);
                    itemGameObject.transform.localPosition = new Vector3(0, 0.25f, 0);
                    itemData.onPayload = true;
                    itemData.itemPickupable = false;

                    float multiplier = 1f;
                    Vector3 newBlockPosition = blockGameObject.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                    SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(newBlockPosition, multiplier);

                    AbilityData extra = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Extra);

                    int updatedAmmoCount = itemData.itemObject.projectile[0].ammoCount;
                    if (extra != null)
                    {
                        updatedAmmoCount = Mathf.RoundToInt(updatedAmmoCount * extra.value);
                    }
                    blockData.ammoCount = updatedAmmoCount;
                    SingletonManager.Instance.uiManager.ShowAmmoCount(blockGameObject, blockData.ammoCount, updatedAmmoCount);
                }
            }
        }
    }
    public GameObject CreateItem(ItemObject createItemObject, Vector3 position, Transform parentTransform)
    {
        GameObject createdItemGameObject = Instantiate(itemPrefab, position, Quaternion.identity);

        ItemData itemData = new ItemData(createItemObject, createdItemGameObject);
        itemData.NewParent(parentTransform);
        itemDictionary[createdItemGameObject] = itemData;
        itemData.spriteRenderer.sprite = createItemObject.itemSprite;

        return createdItemGameObject;
    }

    public void RemoveItem(GameObject itemGameObject)
    {
        if (itemDictionary.TryGetValue(itemGameObject, out ItemData itemData))
        {
            // Remove the item data from the dictionary
            itemDictionary.Remove(itemGameObject);

            // Destroy the GameObject to remove it from the scene
            Destroy(itemGameObject);
        }
    }

    public void RemoveAllItems()
    {
        // Create a list of all GameObjects to avoid modifying the dictionary while iterating
        List<GameObject> itemsToRemove = new List<GameObject>(itemDictionary.Keys);

        // Use RemoveItem to remove each item individually
        foreach (GameObject itemGameObject in itemsToRemove)
        {
            RemoveItem(itemGameObject);
        }

        // Clear the dictionary to ensure it's empty
        itemDictionary.Clear();
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

    public void StartItemBounce(object itemOrGameObject, Vector3 startPosition, Vector3 endPosition, Transform parentTransform)
    {
        GameObject craftedItemObject;

        if (itemOrGameObject is ItemObject itemObject)
        {
            // If the input is an ItemObject, create the item.
            craftedItemObject = SingletonManager.Instance.itemManager.CreateItem(itemObject, startPosition, parentTransform);
        }
        else if (itemOrGameObject is GameObject itemGameObject)
        {
            // If the input is a GameObject, use it directly.
            craftedItemObject = itemGameObject;
            itemDictionary[craftedItemObject].NewParent(parentTransform);
            itemDictionary[craftedItemObject].onPayload = false;
            craftedItemObject.transform.position = startPosition;  // Set start position in case it's not already set
        }
        else
        {
            Debug.LogError("StartItemBounce: Unsupported type. Must be ItemObject or GameObject.");
            return;
        }
        StartCoroutine(ItemBounce(craftedItemObject, startPosition, endPosition, parentTransform, 10f));
    }

    private IEnumerator ItemBounce(GameObject craftedItemObject, Vector3 startPosition, Vector3 endPosition, Transform parentTransform, float firingForce)
    {
        Vector3 localStartPosition = parentTransform.InverseTransformPoint(startPosition);
        Vector3 localEndPosition = parentTransform.InverseTransformPoint(endPosition);

        ItemData craftedItemData = SingletonManager.Instance.itemManager.itemDictionary[craftedItemObject];
        craftedItemData.spriteRenderer.sprite = craftedItemData.itemObject.itemSprite;
        craftedItemData.itemPickupable = false;

        Color newColor = craftedItemData.spriteRenderer.color;
        newColor.a = 0f;
        craftedItemData.spriteRenderer.color = newColor;

        GameObject itemEffectObject = Instantiate(itemEffectPrefab, startPosition, Quaternion.identity, parentTransform);

        // Get the LineRenderer component
        LineRenderer lineRenderer = itemEffectObject.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.2f;

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
