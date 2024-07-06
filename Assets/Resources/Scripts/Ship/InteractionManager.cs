using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public Dictionary<GameObject, GameObject> playerBlockRelations = new Dictionary<GameObject, GameObject>(); // adds two instances where block/players are swapped key/value to make it more efficient when searching.
    public GameObject equippedCannon;
    public CannonBehaviour cannonBehaviour;

    public GameObject playerOne; //temporary player variable

    void Update()
    {
        if (playerBlockRelations.ContainsKey(playerOne))
        {
            cannonBehaviour.cannonSelector();
        }

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
        if (equippedItem != null)
        {
            equippedItemScript = equippedItem.GetComponent<ItemScript>();
            equippedItemObject = equippedItemScript.itemObject;
        }


        blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
        BlockObject blockObject = blockScript.blockObject;
        GameObject blockItem = null;

        blockItem = blockScript.itemPrefabObject;

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
                    blockScript.blockDirection = RotateVector(blockScript.blockDirection, 45);
                }
                break;

            case 1:
                if (blockObject.blockType == BlockType.Cannon)
                {
                    if (equippedItemScript != null)
                    {
                        equippedItemScript.SetItemVisibility(false);
                        equippedItemScript.NewParent(blockPrefab);
                    }


                    blockScript.itemPrefabObject = equippedItem;

                    if (blockItemScript != null)
                    {
                        blockItemScript.SetItemVisibility(true);
                        blockItemScript.NewParent(player);
                    }

                    playerScript.equippedItem = blockItem;
                }
                else if (blockObject.blockType == BlockType.Mast)
                {
                    blockScript.blockDirection = RotateVector(blockScript.blockDirection, -45);
                }
                break;

            case 2:
                if (blockObject.blockType == BlockType.Cannon)
                {

                    if (playerBlockRelations.ContainsKey(player))
                    {
                        Vector3 blockPosition = blockPrefab.transform.position;
                        Vector3 selectorPosition = cannonBehaviour.GetSelectorPosition();
                        Vector3Int selectorTilePosition = cannonBehaviour.WorldToCell(selectorPosition);
                        cannonBehaviour.FireInTheHole(blockPosition, selectorTilePosition, blockItemObject);
                    }
                    else
                    {
                        Debug.Log("shoot blanks");
                    }
                }
                break;

            default:

                break;
        }
    }

    private Vector2 RotateVector(Vector2 originalVector, float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angleRadians);
        float sin = Mathf.Sin(angleRadians);
        return new Vector2(
            cos * originalVector.x - sin * originalVector.y,
            sin * originalVector.x + cos * originalVector.y
        );
    }
}
