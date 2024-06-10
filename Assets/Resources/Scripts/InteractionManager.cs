using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public GameObject equippedCannon;

    public void InteractWithBlock(GameObject blockPrefab, int interaction) // 0=primary  1=secondary interaction
    {
        Debug.Log(interaction);
        blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
        BlockObject blockObject = blockScript.blockObject;

        if (blockScript != null && blockObject != null)
        {

            if (blockObject != null)
            {
                //Debug.Log("Interacted with block ID " + blockObject.id);
            }

        }

        if (blockObject.blockType == BlockType.Cannon)
        {
            if (equippedCannon != null)
            {
                // Destroy the currently equipped cannon if there is one
                Destroy(equippedCannon);
                equippedCannon = blockPrefab;
            }

            Debug.Log("Equipped cannon with ID " + blockObject.id);
            Debug.Log(blockPrefab); // Log the position of the GameObject
        }
        else if (blockObject.blockType == BlockType.Mast)
        {
            if (interaction == 0)
            {
                Debug.Log("turning mast to the right");
            }
            else if (interaction == 1)
            {
                Debug.Log("turning mast to the left");
            }



        }

    }
}

