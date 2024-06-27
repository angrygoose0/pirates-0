using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject itemPrefab;
    public List<ItemObject> itemObjects;
    private Dictionary<string, ItemObject> itemDictionary;

    private GameObject worldTilemap;

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
    public void CreateItem(string itemName, Vector3 position)
    {
        ItemObject createItemObject = GetItemByName(itemName);

        if (createItemObject == null)
        {
            return;
        }

        GameObject createdItem = Instantiate(itemPrefab, position, Quaternion.identity);
        createdItem.transform.SetParent(worldTilemap.transform);

        SpriteRenderer spriteRenderer = createdItem.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = createItemObject.itemSprite;
        }

        ItemScript itemScript = createdItem.GetComponent<ItemScript>();
        if (itemScript != null)
        {
            itemScript.itemObject = createItemObject;
        }

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
    }

}
