using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject itemPrefab;
    public List<ItemObject> itemObjects;
    private Dictionary<string, ItemObject> itemDictionary;

    private GameObject worldTilemap;
    public GameObject examplePrefab;

    private void Awake()
    {
        itemDictionary = new Dictionary<string, ItemObject>();
        foreach (var item in itemObjects)
        {
            itemDictionary[item.itemName] = item;
        }
    }
    void Start()
    {
        worldTilemap = GameObject.Find("world");
    }

    public ItemObject GetItemByName(string itemName)
    {
        if (itemDictionary.TryGetValue(itemName, out ItemObject itemObject))
        {
            return itemObject;
        }
        else
        {
            Debug.LogWarning($"Item with name {itemName} not found!");
            return null;
        }
    }
    // Start is called before the first frame update
    public GameObject CreateItem(ItemObject createItemObject, Vector3 position)
    {
        if (createItemObject == null)
        {
            return null;
        }

        GameObject createdItem = Instantiate(itemPrefab, position, Quaternion.identity);
        createdItem.transform.SetParent(worldTilemap.transform);

        ItemScript itemScript = createdItem.GetComponent<ItemScript>();

        itemScript.spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        itemScript.spriteRenderer.sprite = createItemObject.itemSprite;

        itemScript.itemObject = createItemObject;


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

        return createdItem;
    }

    public float itemBounceForce;
    public float gravity = -9.81f;
    public GameObject itemEffectPrefab;

    public void StartItemBounce(GameObject itemGameObject, Vector3 endLocalShipPosition)
    {
        StartCoroutine(ItemBounce(itemGameObject, endLocalShipPosition, 10f));
    }
    private IEnumerator ItemBounce(GameObject itemGameObject, Vector3 endLocalShipPosition, float firingForce)
    {
        Transform shipTransform = SingletonManager.Instance.shipGenerator.shipTilemapObject.transform;
        GameObject itemEffectObject = Instantiate(itemEffectPrefab, shipTransform.TransformPoint(itemGameObject.transform.localPosition), Quaternion.identity, shipTransform);

        // Get the LineRenderer component
        LineRenderer lineRenderer = itemEffectObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer not found on linePrefab.");
            yield break;
        }

        // Initialize LineRenderer
        List<Vector3> linePoints = new List<Vector3> { shipTransform.TransformPoint(itemEffectObject.transform.localPosition) };

        Rigidbody2D rb = itemEffectObject.GetComponent<Rigidbody2D>();

        float initialDistance = Vector3.Distance(itemEffectObject.transform.localPosition, endLocalShipPosition);
        float timeToTarget = Mathf.Sqrt(2 * initialDistance / (firingForce));

        Vector3 displacement = endLocalShipPosition - itemEffectObject.transform.localPosition;

        float initialVelocityX = displacement.x / timeToTarget;
        float initialVelocityY = (displacement.y - 0.5f * gravity * Mathf.Pow(timeToTarget, 2)) / timeToTarget;

        rb.velocity = new Vector2(initialVelocityX, initialVelocityY);

        float elapsedTime = 0f;

        // Simulate the line flight to the end position
        while (elapsedTime < timeToTarget)
        {
            elapsedTime += Time.deltaTime;

            displacement = endLocalShipPosition - itemEffectObject.transform.localPosition;

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
            linePoints.Add(shipTransform.TransformPoint(itemEffectObject.transform.localPosition));
            lineRenderer.positionCount = linePoints.Count;
            lineRenderer.SetPositions(linePoints.ToArray());

            yield return null;
        }
        Destroy(itemEffectObject);

        itemGameObject.transform.localPosition = endLocalShipPosition;

        ItemScript itemScript = itemGameObject.GetComponent<ItemScript>();
        itemScript.itemPickupable = true;
        Color newColor = itemScript.spriteRenderer.color;
        newColor.a = 1f;
        itemScript.spriteRenderer.color = newColor;

    }


}
