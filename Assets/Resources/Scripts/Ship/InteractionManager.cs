using UnityEngine;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    public GameObject equippedCannon;
    public CannonBehaviour cannonBehaviour;

    private Dictionary<GameObject, GameObject> playerBlockRelations = new Dictionary<GameObject, GameObject>(); // adds two instances where block/players are swapped key/value to make it more efficient when searching.


    public void InteractWithBlock(GameObject blockPrefab, int interaction, GameObject player) // 0=primary 1=secondary interaction
    {
        PlayerBehaviour playerScript = player.GetComponent<PlayerBehaviour>();
        GameObject equippedItem = null;


        equippedItem = playerScript.equippedItem;

        ItemObject equippedItemObject = null;
        if (equippedItem != null)
        {
            ItemScript equippedItemScript = equippedItem.GetComponent<ItemScript>();
            equippedItemObject = equippedItemScript.itemObject;
        }

        Debug.Log(interaction);

        blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
        BlockObject blockObject = blockScript.blockObject;
        GameObject blockItem = null;


        blockItem = blockScript.itemPrefabObject;

        ItemObject blockItemObject = null;
        if (blockItem != null)
        {
            ItemScript blockItemScript = blockItem.GetComponent<ItemScript>();
            blockItemObject = blockItemScript.itemObject;
        }

        if (blockObject.blockType == BlockType.Cannon)
        {
            Debug.Log("cannoninteraction");
            if (interaction == 0)
            {
                Debug.Log("interaction 0");
                if (blockScript.player == player)
                {
                    blockScript.player = null; //player exits cannon
                }
                else
                {
                    blockScript.player = player; //player equips empty / kicks out another player
                }
            }
            else if (interaction == 1)
            {
                Debug.Log("interaction 1");
                blockScript.itemPrefabObject = equippedItem;
                playerScript.equippedItem = blockItem;
            }

            else if (interaction == 2) //fire
            {
                Debug.Log("interaction 2");
                Vector3 blockPosition = blockPrefab.transform.position;
                Vector3 selectorPosition = cannonBehaviour.GetSelectorPosition();
                Vector3Int selectorTilePosition = cannonBehaviour.WorldToCell(selectorPosition);
                cannonBehaviour.FireInTheHole(blockPosition, selectorTilePosition, blockItemObject);
            }
        }

        else if (blockObject.blockType == BlockType.Mast)
        {
            if (interaction == 0)
            {
                Debug.Log("turning mast clockwise 45");
                blockScript.blockDirection = RotateVector(blockScript.blockDirection, 45);
            }
            else if (interaction == 1)
            {
                Debug.Log("turning mast anti-clockwise 45");
                blockScript.blockDirection = RotateVector(blockScript.blockDirection, -45);
            }
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
