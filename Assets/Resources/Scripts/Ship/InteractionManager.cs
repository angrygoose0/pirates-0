using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //sometimes when a player interacts, the blockPrefab shouldnt have to be defined, as the player might already be equipped to a block, so it should check the dictionary to see what block the player is attatched to first.
    {

        PlayerBehaviour playerScript = player.GetComponent<PlayerBehaviour>();

        GameObject blockPrefab = null;

        if (playerBlockRelations.ContainsKey(player))
        {
            blockPrefab = playerBlockRelations[player];
            // if exists, continue to use the block the player is equipped rn

        }
        else
        {
            blockPrefab = playerScript.selectedBlockPrefab;
            //use the blockprefab the player is looking at rn. because the player hasnt equipped a block.
        }


        GameObject equippedItem = null;
        equippedItem = playerScript.equippedItem;
        ItemScript equippedItemScript = null;

        ItemObject equippedItemObject = null;
        if (playerScript.equippedItem != null)
        {
            equippedItemScript = equippedItem.GetComponent<ItemScript>();
            equippedItemObject = equippedItemScript.itemObject;
        }


        blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
        BlockObject blockObject = blockScript.blockObject;
        GameObject blockItem = null;

        if (blockScript.itemPrefabObject != null && blockScript.itemPrefabObject.Count == 1)
        {
            blockItem = blockScript.itemPrefabObject[0];
        }


        ItemScript blockItemScript = null;
        ItemObject blockItemObject = null;
        if (blockItem != null)
        {
            blockItemScript = blockItem.GetComponent<ItemScript>();
            blockItemObject = blockItemScript.itemObject;
        }

        switch (interaction)
        {
            case 0:
                if (blockObject.blockType == BlockType.Cannon)
                {

                    //the player is equipped to a block, so he is leaving.
                    if (playerBlockRelations.ContainsKey(player))
                    {
                        playerBlockRelations.Remove(player);
                        playerBlockRelations.Remove(blockPrefab);
                        //player exits cannon
                    }
                    else //the player isn't equipped to a block, so he's entering
                    {
                        playerBlockRelations[player] = blockPrefab; // the player shouldn't have another instance with the same key

                        if (playerBlockRelations.ContainsKey(blockPrefab))
                        {
                            GameObject existingPlayer = playerBlockRelations[blockPrefab];
                            playerBlockRelations.Remove(existingPlayer);
                        }

                        playerBlockRelations[blockPrefab] = player;


                        //player equips empty / kicks out another player
                    }
                }
                else if (blockObject.blockType == BlockType.Mast)
                {
                    blockScript.active = !blockScript.active;
                }
                else if (blockObject.blockType == BlockType.Payload)
                {
                    if (blockItemObject != null)
                    {
                        if (blockItemObject.activeAbility != null)
                        {
                            float multiplier = 1f;
                            Vector3 newBlockPosition = blockScript.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                            SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(newBlockPosition, multiplier);
                            blockItemScript.ActivateActive(blockPrefab.transform.position, blockScript.blockDirection.ToVector2());
                        }
                    }
                }
                break;

            case 1:

                if (blockObject.blockType == BlockType.Cannon)
                {

                    if (equippedItemObject.projectileData == null || equippedItemScript == null || equippedItemObject.projectileData.Count != 1) // so not ammo
                    {
                        break;
                    }
                    ProjectileData projectile = equippedItemObject.projectileData[0];

                    equippedItemScript.SetCollider(false);
                    equippedItemScript.NewParent(blockPrefab);
                    equippedItemScript.itemPickupable = false;
                    if (blockScript.itemPrefabObject.Count == 0)
                    {
                        blockScript.itemPrefabObject.Add(equippedItem);
                    }
                    else
                    {
                        Destroy(blockScript.itemPrefabObject[0]);
                        blockScript.itemPrefabObject[0] = equippedItem;
                    }
                    playerScript.equippedItem = null;

                    AbilityData extra = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Extra);

                    int updatedAmmoCount = projectile.ammoCount;
                    if (extra != null)
                    {
                        updatedAmmoCount = Mathf.RoundToInt(projectile.ammoCount * extra.value);
                    }

                    blockScript.ammoCount = updatedAmmoCount;

                    SingletonManager.Instance.uiManager.ShowAmmoCount(blockPrefab, blockScript.ammoCount, updatedAmmoCount);


                }

                if (blockObject.blockType == BlockType.Payload)
                {

                    if (equippedItemScript == null || equippedItemObject.projectileData.Count > 0) // so is ammo
                    {
                        break;
                    }

                    equippedItemScript.SetCollider(false);
                    equippedItemScript.NewParent(blockPrefab);
                    equippedItemScript.onPayload = true;
                    equippedItemScript.itemPickupable = false;

                    if (equippedItemScript.itemObject.spawningItem != null)
                    {
                        equippedItemScript.spriteRenderer.sprite = equippedItemScript.itemObject.spawningItem.itemSprite;
                        Color newColor = equippedItemScript.spriteRenderer.color;
                        newColor.a = 0.5f;
                        equippedItemScript.spriteRenderer.color = newColor;
                    }

                    blockScript.itemPrefabObject.Add(equippedItem);
                    blockScript.blockLight.intensity = 1.5f;
                    playerScript.equippedItem = null;

                    float multiplier = 1f;
                    Vector3 newBlockPosition = blockScript.transform.position + new Vector3(0.0f, 0.125f, 0.0f);
                    SingletonManager.Instance.feedbackManager.ArtifactPlaceFeedback(newBlockPosition, multiplier);

                    SingletonManager.Instance.shipGenerator.UpdateBlockEffects();

                    if (equippedItemObject.affectsCannons)
                    {
                        SingletonManager.Instance.shipGenerator.MakeTrailEffects(blockScript.transform);
                    }


                }
                break;

            case 2:
                if (blockObject.blockType == BlockType.Cannon)
                {

                    if (playerBlockRelations.ContainsKey(player))
                    {
                        ProjectileData projectile = null;
                        if (blockItemObject.projectileData != null)
                        {
                            if (blockItemObject.projectileData.Count > 0)
                            {
                                projectile = blockItemObject.projectileData[0];
                            }

                            // Use projectile here
                        }
                        if (blockItemObject == null && blockScript.ammoCount == 0 && projectile != null)
                        {
                            Debug.Log("shoot blanks");
                            break;
                        }

                        Vector3 blockPosition = blockPrefab.transform.position;
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
                            SingletonManager.Instance.cannonBehaviour.FireInTheHole(blockPosition, targetTile, projectile, blockItemObject.mass);
                            StartCoroutine(WhiteFlashCoroutine(blockPrefab.GetComponent<SpriteRenderer>()));


                        }

                        AbilityData extra = SingletonManager.Instance.abilityManager.GetAbilityData(Ability.Extra);

                        int updatedAmmoCount = projectile.ammoCount;
                        if (extra != null)
                        {
                            updatedAmmoCount = Mathf.RoundToInt(projectile.ammoCount * extra.value);
                        }

                        blockScript.ammoCount -= 1;
                        SingletonManager.Instance.uiManager.ShowAmmoCount(blockPrefab, blockScript.ammoCount, updatedAmmoCount);
                        if (blockScript.ammoCount == 0)
                        {
                            blockScript.itemPrefabObject.Clear();
                            Destroy(blockItem);
                        }

                    }
                }
                break;

            case 3:
                blockScript.RotateBlock(1, false);
                Debug.Log(blockScript.blockDirection);
                break;

            default:

                break;
        }
    }


}
