using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class BlockData
{
    public BlockObject blockObject;
    public Direction blockDirection;
    public int ammoCount;
    public Coroutine directionFadeCoroutine;
    public List<GameObject> itemsInBlock;
    public GameObject spawnedItem;
    public Coroutine itemSpawningCoroutine;
    // Constructor
    public BlockData(BlockObject blockObject)
    {
        this.blockObject = blockObject;
        this.blockDirection = Direction.N;
        this.ammoCount = 0;
        this.itemsInBlock = new List<GameObject>();
        this.spawnedItem = null;
        this.itemSpawningCoroutine = null;
        this.directionFadeCoroutine = null;
    }

}


public class BlockManager : MonoBehaviour
{
    public Dictionary<GameObject, BlockData> blockDictionary = new Dictionary<GameObject, BlockData>();
    public GameObject blockPrefab;

    void Start()
    {
        StartCoroutine(GlobalCoroutine());
    }

    private IEnumerator GlobalCoroutine()
    {
        while (true)
        {
            foreach (KeyValuePair<GameObject, BlockData> entry in blockDictionary)
            {
                SpawnItemFromPayload(entry.Key);
            }

            yield return new WaitForSeconds(0.01f);  // Check every second
        }
    }

    // call this whenever an item is added to a block
    public void SpawnItemFromPayload(GameObject blockGameObject)
    {
        if (blockDictionary.TryGetValue(blockGameObject, out BlockData blockData))
        {
            if (blockData.blockObject.blockType != BlockType.Payload) return;
            // Proceed only if there is exactly one item in the block
            if (blockData.itemsInBlock.Count != 1) return;

            // Get item data for the single item in the block
            ItemData itemData = SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[0]];

            // Check if the block can spawn and if the spawning item exists
            if (blockData.itemSpawningCoroutine != null || itemData.itemObject.spawningItem == null) return;

            // Check conditions to spawn the item
            bool shouldSpawnItem = blockData.spawnedItem == null || SingletonManager.Instance.itemManager.itemDictionary[blockData.spawnedItem].itemTaken;

            if (shouldSpawnItem)
            {
                blockData.spawnedItem = null;
                blockData.itemSpawningCoroutine = StartCoroutine(SpawnItemWithDelay(blockGameObject, 2f, itemData.itemObject.spawningItem));
            }
        }
    }

    public GameObject CreateNewBlock(BlockObject blockObject, Vector3 worldPosition, Transform parentTransform)
    {
        GameObject blockInstance = Instantiate(blockPrefab, worldPosition, Quaternion.identity, parentTransform);
        blockInstance.GetComponentInChildren<Light2D>().intensity = 0f;
        BlockData newBlockData = new BlockData(blockObject);
        blockDictionary[blockInstance] = newBlockData;

        RotateBlock(blockInstance, 0, false);

        return blockInstance;
    }

    public void RemoveBlock(GameObject blockInstance)
    {
        if (!blockDictionary.ContainsKey(blockInstance))
        {
            return;
        }

        BlockData blockData = blockDictionary[blockInstance];

        if (blockData.directionFadeCoroutine != null)
        {
            StopCoroutine(blockData.directionFadeCoroutine);
        }

        blockDictionary.Remove(blockInstance);
        Destroy(blockInstance);
    }

    public void DestroyAllBlocks()
    {
        // Iterate over a copy of the dictionary keys to avoid modifying the dictionary during iteration
        foreach (GameObject blockInstance in blockDictionary.Keys.ToList())
        {
            RemoveBlock(blockInstance);
        }
    }



    private IEnumerator FadeAlpha(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        float duration = 0.75f; // Duration of the fade-out effect
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

    public void RotateBlock(GameObject blockGameObject, int steps, bool highlighted)
    {
        if (blockDictionary.TryGetValue(blockGameObject, out BlockData blockData))
        {
            if (steps > 0)
            {
                blockData.blockDirection = blockData.blockDirection.Rotate(steps);
                Transform directionUI = blockGameObject.transform.Find("DirectionUI");
                directionUI.rotation = Quaternion.Euler(0, 0, blockData.blockDirection.GetZRotation());

                if (blockData.directionFadeCoroutine != null)
                {
                    StopCoroutine(blockData.directionFadeCoroutine);
                }

                blockData.directionFadeCoroutine = StartCoroutine(FadeAlpha(directionUI.gameObject.GetComponent<SpriteRenderer>()));

            }

            SpriteRenderer blockSpriteRenderer = blockGameObject.GetComponent<SpriteRenderer>();
            if (blockData.blockObject.DirectionToSpriteDict.ContainsKey(blockData.blockDirection))
            {
                if (!highlighted)
                {
                    blockSpriteRenderer.sprite = blockData.blockObject.DirectionToSpriteDict[blockData.blockDirection];
                }
                else if (highlighted)
                {
                    if (blockData.blockObject.HighlightSpriteDict.ContainsKey(blockData.blockDirection))
                    {
                        blockSpriteRenderer.sprite = blockData.blockObject.HighlightSpriteDict[blockData.blockDirection];
                    }
                }
            }
        }
    }

    public Vector3 GetRandomPositionInShipFromBlock(GameObject blockGameObject)
    {
        Vector3Int blockTilePosition = SingletonManager.Instance.shipGenerator.shipTilemap.WorldToCell(blockGameObject.transform.position);
        List<Vector3Int> possibleTiles = SingletonManager.Instance.creatureManager.GetSurroundingTiles(blockTilePosition, 4f);
        List<Vector3Int> validTiles = new List<Vector3Int>();

        foreach (Vector3Int tilePosition in possibleTiles)
        {
            if (SingletonManager.Instance.shipGenerator.shipTilemap.HasTile(tilePosition))
            {
                validTiles.Add(tilePosition);
            }
        }

        Vector3Int randomEndTile = validTiles[Random.Range(0, validTiles.Count)];
        Vector3 randomEndPosition = SingletonManager.Instance.shipGenerator.shipTilemap.CellToWorld(randomEndTile);
        Vector3 tileSize = SingletonManager.Instance.shipGenerator.shipTilemap.cellSize;
        float offsetX = Random.Range(-0.5f, 0.5f) * tileSize.x;
        float offsetY = Random.Range(-0.5f, 0.5f) * tileSize.y;
        randomEndPosition.x += offsetX;
        randomEndPosition.y += offsetY;

        return randomEndPosition;

    }

    public void CanCraftNew(GameObject blockGameObject) // call this whenever a new item is placed inside the block
    {
        if (blockDictionary.TryGetValue(blockGameObject, out BlockData blockData))
        {
            if (blockData.blockObject.blockType != BlockType.Payload)
            {
                return;
            }

            if (blockData.itemsInBlock.Count > 1)
            {
                List<List<ItemObject>> craftedItems = new List<List<ItemObject>>();

                for (int i = 0; i < blockData.itemsInBlock.Count; i++)
                {
                    ItemData itemData = SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[i]];
                    ItemObject itemObject = itemData.itemObject;

                    // filter by recipe
                    List<ItemObject> craftedItem = SingletonManager.Instance.itemManager.itemObjects.Where(item => item.recipes != null && item.recipes.Any(recipe => recipe.materials.Contains(itemObject))).ToList();
                    craftedItems.Add(craftedItem);
                }

                List<ItemObject> intersectionList = craftedItems.Skip(1)
                    .Aggregate(
                        new HashSet<ItemObject>(craftedItems.First()),
                        (h, e) => { h.IntersectWith(e); return h; }
                    ).ToList();  // Convert to List
                                 // (Optional) Set up any visu

                Random.InitState(System.DateTime.Now.Millisecond);
                if (intersectionList.Count > 0)
                {
                    foreach (GameObject item in blockData.itemsInBlock)
                    {
                        SingletonManager.Instance.itemManager.RemoveItem(item);
                    }
                    blockData.itemsInBlock.Clear();
                    foreach (ItemObject resultItem in intersectionList)
                    {
                        blockGameObject.GetComponentInChildren<Light2D>().intensity = 0f;

                        Vector3 randomEndPosition = GetRandomPositionInShipFromBlock(blockGameObject);
                        SingletonManager.Instance.itemManager.StartItemBounce(resultItem, blockGameObject.transform.position, randomEndPosition, SingletonManager.Instance.shipGenerator.shipTilemap.transform);
                    }
                }
                else
                {
                    Debug.Log("no recipes"); //shoot out both ingredients
                    blockGameObject.GetComponentInChildren<Light2D>().intensity = 0f;

                    foreach (GameObject itemGameObject in blockData.itemsInBlock)
                    {
                        Vector3 randomEndPosition = GetRandomPositionInShipFromBlock(blockGameObject);
                        
                        SingletonManager.Instance.itemManager.StartItemBounce(itemGameObject, blockGameObject.transform.position, randomEndPosition, SingletonManager.Instance.shipGenerator.shipTilemap.transform);
                    }

                    blockData.itemsInBlock.Clear();
                    return;
                }
            }
        }
    }

    private IEnumerator SpawnItemWithDelay(GameObject blockGameObject, float delay, ItemObject spawningItemObject)
    {
        if (blockDictionary.TryGetValue(blockGameObject, out BlockData blockData))
        {
            yield return new WaitForSeconds(delay);
            Vector3 spawnPosition = new Vector3(blockGameObject.transform.position.x, blockGameObject.transform.position.y + 0.25f, blockGameObject.transform.position.z);
            blockData.spawnedItem = SingletonManager.Instance.itemManager.CreateItem(spawningItemObject, spawnPosition, blockGameObject.transform);

            blockData.itemSpawningCoroutine = null;
        }
    }


}
