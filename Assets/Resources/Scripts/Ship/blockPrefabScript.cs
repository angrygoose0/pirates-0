using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;

public class blockPrefabScript : MonoBehaviour
{
    public BlockObject blockObject; // Variable to store block objects
    public Vector2 blockDirection = Vector2.up; // Variable to store the direction as a Vector2      
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
    private Dictionary<Vector2, Sprite> directionToSprite;

    public Sprite northSprite;
    public Sprite northEastSprite;
    public Sprite eastSprite;
    public Sprite southEastSprite;
    public Sprite southSprite;
    public Sprite southWestSprite;
    public Sprite westSprite;
    public Sprite northWestSprite;


    void Start()
    {
        shipObject = GameObject.Find("ship");

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = blockObject.blockSprite;
        blockLight = GetComponentInChildren<Light2D>();
        blockLight.intensity = 0;
        directionSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        directionToSprite = new Dictionary<Vector2, Sprite>
        {
            { new Vector2(0, 1), northSprite },         // North
            { new Vector2(1, 1), northEastSprite },     // North-East
            { new Vector2(1, 0), eastSprite },          // East
            { new Vector2(1, -1), southEastSprite },    // South-East
            { new Vector2(0, -1), southSprite },        // South
            { new Vector2(-1, -1), southWestSprite },   // South-West
            { new Vector2(-1, 0), westSprite },         // West
            { new Vector2(-1, 1), northWestSprite }     // North-West
        };

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


    public void ChangeDirection(Vector2 direction)
    {


        // Set the block direction to the closest allowed direction
        blockDirection = direction;
        Debug.Log(direction);

        // Change the sprite to match the new direction
        if (directionToSprite.ContainsKey(direction))
        {
            directionSpriteRenderer.sprite = directionToSprite[direction];
            Debug.Log("changedSprite");
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
