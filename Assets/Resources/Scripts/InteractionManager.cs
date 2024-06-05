using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public GameObject equippedCannon;

    public void InteractWithBlock(GameObject blockPrefab)
    {
        blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
        BlockObject blockObject = blockScript.blockObject;

        if (blockScript != null && blockObject != null)
        {

            if (blockObject != null)
            {
                //Debug.Log("Interacted with block ID " + blockObject.id);
            }

        }

        if (blockObject.isCannon)
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
    }
}

