using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;




// this should all gp in blockObjects

public class blockPrefabScript : MonoBehaviour
{
    public BlockObject blockObject; // Variable to store block objects
    public Direction blockDirection;
    private Transform directionUI;
    private SpriteRenderer directionSpriteRenderer;
    public List<GameObject> itemPrefabObject = new List<GameObject>(); //items that are in the block
    private ItemObject itemObject;
    public GameObject player;
    public GameObject itemPrefab;
    public GameObject spawnedItem;
    public GameObject shipObject;
    public int ammoCount;
    public Light2D blockLight;


    public List<ItemObject> itemObjectList; // list for the global item scriptable objectlist
    private ItemScript spawnedItemScript;
    private Collider2D spawnedItemCollider;

    public bool active = false;
    private bool isSpawning = false;
    private Coroutine fadeCoroutine;



    private IEnumerator FadeAlpha(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        float duration = 1.5f; // Duration of the fade-out effect
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, newAlpha);

            // Wait for the next frame before continuing the fade
            yield return null;
        }

        // Ensure the alpha is set to 0 at the end
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
    }

    public void RotateBlock(int steps, bool highlighted)
    {
        if (steps > 0)
        {
            blockDirection = blockDirection.Rotate(steps);
            directionUI.rotation = Quaternion.Euler(0, 0, blockDirection.GetZRotation());

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeAlpha(directionSpriteRenderer));

        }

        SpriteRenderer blockSpriteRenderer = GetComponent<SpriteRenderer>();
        if (blockObject.DirectionToSpriteDict.ContainsKey(blockDirection))
        {
            if (!highlighted)
            {
                blockSpriteRenderer.sprite = blockObject.DirectionToSpriteDict[blockDirection];
            }
            else if (highlighted)
            {
                if (blockObject.HighlightSpriteDict.ContainsKey(blockDirection))
                {
                    blockSpriteRenderer.sprite = blockObject.HighlightSpriteDict[blockDirection];
                }
            }
        }
    }


    void Start()
    {

        shipObject = GameObject.Find("ship");
        blockDirection = Direction.NW;

        SpriteRenderer blockSpriteRenderer = GetComponent<SpriteRenderer>();
        if (blockObject.DirectionToSpriteDict.ContainsKey(blockDirection))
        {
            // Set the sprite to the one corresponding to the new direction
            blockSpriteRenderer.sprite = blockObject.DirectionToSpriteDict[blockDirection];
        }
        blockLight = GetComponentInChildren<Light2D>();
        blockLight.intensity = 0;

        directionUI = transform.Find("DirectionUI");
        directionSpriteRenderer = directionUI.gameObject.GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        if (blockObject.blockType == BlockType.Payload)
        {
            if (itemPrefabObject.Count > 1) // this should only be done when the itemPrefabObject list is changed,  not every frame.
            {
                Debug.Log("crafting");

                List<List<ItemObject>> craftedItems = new List<List<ItemObject>>();

                for (int i = 0; i < itemPrefabObject.Count; i++)
                {
                    ItemScript itemScript = itemPrefabObject[i].GetComponent<ItemScript>();
                    ItemObject itemObject = itemScript.itemObject;

                    List<ItemObject> craftedItem = FilterByRecipe(itemObject);

                    craftedItems.Add(craftedItem);


                }


                List<ItemObject> intersectionList = craftedItems.Skip(1)
                    .Aggregate(
                        new HashSet<ItemObject>(craftedItems.First()),
                        (h, e) => { h.IntersectWith(e); return h; }
                    ).ToList();  // Convert to List


                if (intersectionList.Count > 0)
                {
                    foreach (GameObject item in itemPrefabObject)
                    {
                        Destroy(item);
                    }
                    itemPrefabObject.Clear();
                    foreach (ItemObject resultItem in intersectionList)
                    {
                        GameObject craftedItemObject = Instantiate(itemPrefab, gameObject.transform.position, Quaternion.identity);
                        craftedItemObject = SingletonManager.Instance.itemManager.CreateItem(resultItem, gameObject.transform.position);

                        ItemScript craftedItemScript = craftedItemObject.GetComponent<ItemScript>();
                        craftedItemScript.NewParent(shipObject);

                        itemPrefabObject.Add(craftedItemObject);
                    }


                }
                else
                {
                    Debug.Log("no recipes"); //shoot out both ingredients
                    return;
                }

            }
            else if (itemPrefabObject.Count == 1)
            {
                ItemScript itemScript = itemPrefabObject[0].GetComponent<ItemScript>();
                ItemObject spawningItemObject = itemScript.itemObject; //the item doing the spawning
                ItemObject spawningItem = spawningItemObject.spawningItem; //the item being spanwed
                if (!isSpawning && spawningItem != null)
                {
                    if (spawnedItem == null || (spawnedItemScript?.itemTaken == true))
                    {
                        spawnedItem = null;
                        spawnedItemCollider = null;
                        spawnedItemScript = null;

                        StartCoroutine(SpawnItemWithDelay(2f, spawningItemObject));
                    }
                }
            }

        }


    }



    public List<ItemObject> FilterByRecipe(ItemObject targetItem)
    {
        return itemObjectList.Where(item => item.recipes != null && item.recipes.Any(recipe => recipe.materials.Contains(targetItem))).ToList();
    }




    private IEnumerator SpawnItemWithDelay(float delay, ItemObject itemObject)
    {
        isSpawning = true;
        yield return new WaitForSeconds(delay);
        SpawnItem(itemObject);
        isSpawning = false;
    }

    private void SpawnItem(ItemObject itemObject)
    {
        ItemObject spawningItem = itemObject.spawningItem;
        if (spawningItem != null)
        {
            Vector3 offset = new Vector3(0f, 0f, 0f);
            spawnedItem = Instantiate(itemPrefab, gameObject.transform.position + offset, Quaternion.identity);


            spawnedItemScript = spawnedItem.GetComponent<ItemScript>();
            spawnedItemCollider = spawnedItem.GetComponent<Collider2D>();

            spawnedItemCollider.enabled = false;
            spawnedItemScript.NewParent(gameObject);
            spawnedItemScript.itemObject = spawningItem;
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
}
