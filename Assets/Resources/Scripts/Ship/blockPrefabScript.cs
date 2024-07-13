using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blockPrefabScript : MonoBehaviour
{
    public BlockObject blockObject; // Variable to store block objects
    public Vector2 blockDirection = Vector2.up; // Variable to store the direction as a Vector2                                  
    public GameObject itemPrefabObject;
    private ItemObject itemObject;
    public GameObject player;
    public GameObject itemPrefab;
    public GameObject spawnedItem;

    private ItemScript spawnedItemScript;
    private Collider2D spawnedItemCollider;

    private bool isSpawning = false;

    void Update()
    {
        DisplayItemInfo();
        if (blockObject.blockType == BlockType.Payload && !isSpawning && itemObject != null)
        {
            if (spawnedItem == null || (spawnedItemScript?.itemTaken == true))
            {
                spawnedItem = null;
                spawnedItemCollider = null;
                spawnedItemScript = null;
                if (itemObject.spawningSprite != null)
                {
                    Debug.Log("started");
                    StartCoroutine(SpawnItemWithDelay(2f));

                }
            }
        }
    }

    private IEnumerator SpawnItemWithDelay(float delay)
    {
        isSpawning = true;
        yield return new WaitForSeconds(delay);
        SpawnItem();
        isSpawning = false;
    }

    private void SpawnItem()
    {
        Sprite spawningSprite = itemObject.spawningSprite;
        if (spawningSprite != null)
        {
            Vector3 offset = new Vector3(0f, 0.5f, 0f);
            spawnedItem = Instantiate(itemPrefab, gameObject.transform.position + offset, Quaternion.identity);


            spawnedItemScript = spawnedItem.GetComponent<ItemScript>();
            spawnedItemCollider = spawnedItem.GetComponent<Collider2D>();

            spawnedItemCollider.enabled = false;
            spawnedItemScript.NewParent(gameObject);
            spawnedItemScript.itemTaken = false;
            spawnedItemScript.itemPickupable = true;

        }
    }

    private List<BlockValue> GetBlockValues()
    {
        if (blockObject != null)
        {
            return blockObject.blockValues;
        }
        else
        {
            Debug.LogWarning("BlockObject is not assigned.");
            return null;
        }
    }

    void Start()
    {
        // Ensure the sprite renderer is set up with the correct sprite
        if (blockObject != null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = blockObject.blockSprite;
            }
        }
        else
        {
            Debug.LogWarning("BlockObject is not assigned.");
        }
    }

    public float GetBlockValueByName(string name)
    {
        if (blockObject != null)
        {
            foreach (BlockValue blockValue in blockObject.blockValues)
            {
                if (blockValue.name == name)
                {
                    return blockValue.value;
                }
            }
            Debug.LogWarning($"BlockValue with name {name} not found.");
            return float.NaN;
        }
        else
        {
            Debug.LogWarning("BlockObject is not assigned.");
            return float.NaN;
        }
    }

    public void DisplayItemInfo()
    {
        if (itemPrefabObject != null)
        {
            ItemScript itemScript = itemPrefabObject.GetComponent<ItemScript>();
            if (itemScript != null)
            {
                itemObject = itemScript.itemObject;
            }
        }
    }
}
