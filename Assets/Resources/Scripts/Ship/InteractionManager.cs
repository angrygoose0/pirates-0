using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public GameObject equippedCannon;
    public CannonBehaviour cannonBehaviour;

    public void InteractWithBlock(GameObject blockPrefab, int interaction) // 0=primary 1=secondary interaction
    {
        Debug.Log(interaction);
        blockPrefabScript blockScript = blockPrefab.GetComponent<blockPrefabScript>();
        BlockObject blockObject = blockScript.blockObject;

        ItemObject itemObject = blockScript.itemObject;


        if (blockScript != null && blockObject != null)
        {
            if (blockObject.blockType == BlockType.Cannon)
            {
                if (interaction == 0)
                {
                    if (equippedCannon)
                    {
                        Debug.Log("a");
                        Vector3 blockPosition = blockPrefab.transform.position;
                        Debug.Log("b");
                        Vector3 selectorPosition = cannonBehaviour.GetSelectorPosition();
                        Debug.Log("c");
                        Vector3Int selectorTilePosition = cannonBehaviour.WorldToCell(selectorPosition);
                        Debug.Log("d");
                        cannonBehaviour.FireInTheHole(blockPosition, selectorTilePosition, itemObject);
                    }
                    else
                    {
                        equippedCannon = blockPrefab;
                        Debug.Log("Equipped cannon with ID " + blockObject.id);
                        Debug.Log(blockPrefab); // Log the position of the GameObject
                    }
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
