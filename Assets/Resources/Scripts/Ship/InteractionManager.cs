using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InteractionManager : MonoBehaviour
{
    public Dictionary<GameObject, GameObject> playerBlockRelations = new Dictionary<GameObject, GameObject>(); // adds two instances where block/players are swapped key/value to make it more efficient when searching.
    public GameObject equippedCannon;


    public GameObject playerOne; //temporary player variable

    void Update()
    {
        if (playerBlockRelations.ContainsKey(playerOne))
        {
            SingletonManager.Instance.cannonBehaviour.cannonSelector();
        }
        else
        {
            Destroy(SingletonManager.Instance.cannonBehaviour.selectorInstantiated);
        }

    }

    private IEnumerator WhiteFlashCoroutine(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.material.SetFloat("_WhiteAmount", 1f);

        yield return new WaitForSeconds(0.2f);

        spriteRenderer.material.SetFloat("_WhiteAmount", 0f);
    }

    public List<Vector3Int> GetSurroundingTiles(Vector3Int centerTile, float range)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        int rangeInt = Mathf.Max(1, Mathf.CeilToInt(range));

        for (int x = -rangeInt; x <= rangeInt; x++)
        {
            for (int y = -rangeInt; y <= rangeInt; y++)
            {
                Vector3Int tile = new Vector3Int(centerTile.x + x, centerTile.y + y, centerTile.z);
                if (Vector3Int.Distance(centerTile, tile) <= range)
                {
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }



    public void InteractWithBlock(int interaction, GameObject player) // 0=primary 1=secondary interaction  
    //sometimes when a player interacts, the blockGameObject shouldnt have to be defined, as the player might already be equipped to a block, so it should check the dictionary to see what block the player is attatched to first.
    {
        PlayerBehaviour playerScript = player.GetComponent<PlayerBehaviour>();

        GameObject blockGameObject = null;

        if (playerBlockRelations.ContainsKey(player))
        {
            blockGameObject = playerBlockRelations[player];
            // if exists, continue to use the block the player is equipped rn

        }
        else
        {
            blockGameObject = playerScript.selectedBlockPrefab;
            //use the blockprefab the player is looking at rn. because the player hasnt equipped a block.
        }

        BlockData blockData = SingletonManager.Instance.blockManager.blockDictionary[blockGameObject];

        Debug.Log("ini");
        Debug.Log(interaction);

        switch (interaction)
        {
            case 0:
                if (blockData.blockObject.blockType == BlockType.Cannon)
                {
                    Vector3 feedbackPosition = blockGameObject.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                    SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(feedbackPosition, 3f);
                    //the player is equipped to a block, so he is leaving.
                    if (playerBlockRelations.ContainsKey(player))
                    {
                        playerBlockRelations.Remove(player);
                        playerBlockRelations.Remove(blockGameObject);
                        //player exits cannon
                    }
                    else //the player isn't equipped to a block, so he's entering
                    {
                        playerBlockRelations[player] = blockGameObject; // the player shouldn't have another instance with the same key

                        if (playerBlockRelations.ContainsKey(blockGameObject))
                        {
                            GameObject existingPlayer = playerBlockRelations[blockGameObject];
                            playerBlockRelations.Remove(existingPlayer);
                        }

                        playerBlockRelations[blockGameObject] = player;
                        //player equips empty / kicks out another player
                    }
                }
                else if (blockData.blockObject.blockType == BlockType.Mast)
                {
                    //blockData.active = !blockData.active;
                }
                else if (blockData.blockObject.blockType == BlockType.Payload)
                {
                    if (blockData.itemsInBlock != null && blockData.itemsInBlock.Count == 1)
                    {
                        if (SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[0]].itemObject.activeAbility != null)
                        {
                            Vector3 feedbackPosition = blockGameObject.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                            SingletonManager.Instance.itemManager.ActivateActive(blockData.itemsInBlock[0], blockGameObject.transform.position, blockData.blockDirection, feedbackPosition);
                        }
                    }
                }
                break;

            case 1:
                // Check if the block is of type Cannon or Payload and the player is holding an item
                if ((blockData.blockObject.blockType == BlockType.Cannon || blockData.blockObject.blockType == BlockType.Payload)
                    && playerScript.equippedItem != null)
                {
                    SingletonManager.Instance.itemManager.PlaceItemOnBlock(playerScript.equippedItem, blockGameObject);
                    playerScript.equippedItem = null;
                }
                break;


            case 2:
                if (blockData.blockObject.blockType == BlockType.Cannon)
                {
                    if (playerBlockRelations.ContainsKey(player))
                    {
                        if (blockData.itemsInBlock.Count != 1 || blockData.itemsInBlock[0] == null || blockData.ammoCount == 0)
                        {
                            Debug.Log("shoot blanks");
                            break;
                        }

                        ItemData blockItemData = SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[0]];

                        if (blockItemData.itemObject.projectile.Count != 1)
                        {
                            Debug.Log("shoot blanks");
                            break;
                        }

                        Vector3 feedbackPosition = blockGameObject.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                        SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(feedbackPosition, 1f, blockItemData.itemObject.effectColor);

                        ProjectileData projectile = blockItemData.itemObject.projectile[0];

                        Vector3 blockPosition = blockGameObject.transform.position;
                        Vector3 selectorPosition = SingletonManager.Instance.cannonBehaviour.GetSelectorPosition();
                        Vector3Int selectorTilePosition = SingletonManager.Instance.cannonBehaviour.WorldToCell(selectorPosition);


                        float deltaX = blockPosition.x - selectorPosition.x;
                        float deltaY = (blockPosition.y - selectorPosition.y) * 0.5f;
                        float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);


                        AbilityData multiple = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Multiple);
                        int fireAmount = projectile.fireAmount;
                        float accuracy = distance * 1f / projectile.accuracy;
                        List<Vector3Int> additionalTilesList = null;

                        if (multiple != null)
                        {
                            fireAmount = fireAmount + Mathf.RoundToInt(multiple.value);
                            if (accuracy < 1.5f)
                            {
                                additionalTilesList = GetSurroundingTiles(selectorTilePosition, 2f);
                            }
                        }



                        List<Vector3Int> tilesList = GetSurroundingTiles(selectorTilePosition, accuracy);



                        for (int i = 0; i < fireAmount; i++)
                        {
                            if (i == 1 && additionalTilesList != null)
                            {
                                tilesList = additionalTilesList;
                            }
                            Vector3Int targetTile = tilesList[Random.Range(0, tilesList.Count)];
                            SingletonManager.Instance.cannonBehaviour.FireInTheHole(blockPosition, targetTile, projectile, SingletonManager.Instance.itemManager.itemDictionary[blockData.itemsInBlock[0]].itemObject.mass);
                            StartCoroutine(WhiteFlashCoroutine(blockGameObject.GetComponent<SpriteRenderer>()));
                        }

                        AbilityData extra = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Extra);

                        int updatedAmmoCount = projectile.ammoCount;
                        if (extra != null)
                        {
                            updatedAmmoCount = Mathf.RoundToInt(projectile.ammoCount * extra.value);
                        }

                        blockData.ammoCount -= 1;
                        SingletonManager.Instance.uiManager.ShowAmmoCount(blockGameObject, blockData.ammoCount, updatedAmmoCount);
                        if (blockData.ammoCount == 0)
                        {
                            SingletonManager.Instance.itemManager.RemoveItem(blockData.itemsInBlock[0]);
                        }

                    }
                }
                break;

            case 3:
                SingletonManager.Instance.blockManager.RotateBlock(blockGameObject, 1, false);
                break;

            default:

                break;
        }
    }


}
