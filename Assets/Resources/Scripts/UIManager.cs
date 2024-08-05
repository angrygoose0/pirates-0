using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject canvas;

    public GameObject ammoCountContainerPrefab;
    public GameObject ammoCountPrefab;

    public Dictionary<GameObject, GameObject> blockToUIRelations = new Dictionary<GameObject, GameObject>();

    public void ShowAmmoCount(GameObject blockObject, int currentAmmoCount, int maxAmmoCount)
    {
        GameObject ammoCountContainer;

        // Check if the block already has a UI container
        if (!blockToUIRelations.TryGetValue(blockObject, out ammoCountContainer) || ammoCountContainer == null)
        {
            // If the container doesn't exist and currentAmmoCount is 0, just return
            if (currentAmmoCount == 0)
            {
                return;
            }

            // Instantiate a new container and add it to the dictionary
            ammoCountContainer = Instantiate(ammoCountContainerPrefab, blockObject.transform.position, Quaternion.identity, canvas.transform);
            blockToUIRelations[blockObject] = ammoCountContainer;
        }

        // If currentAmmoCount is 0, remove the container and dictionary entry
        if (currentAmmoCount == 0)
        {
            Destroy(ammoCountContainer);
            blockToUIRelations.Remove(blockObject);
            return;
        }

        // Clear previous ammo indicators if any
        foreach (Transform child in ammoCountContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new ammo indicators
        for (int i = 1; i <= maxAmmoCount; i++)
        {
            GameObject ammoCount = Instantiate(ammoCountPrefab, ammoCountContainer.transform);
            CanvasGroup canvasGroup = ammoCount.GetComponent<CanvasGroup>();
            if (i <= currentAmmoCount)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = 0.2f;
            }
        }
    }
}
