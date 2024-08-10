using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject canvas;

    public GameObject ammoCountContainerPrefab;
    public GameObject ammoCountPrefab;
    public GameObject helpfulContainerPrefab;

    public Dictionary<GameObject, GameObject> blockToUIRelations = new Dictionary<GameObject, GameObject>();

    public Dictionary<GameObject, GameObject> blockToHelpfulUIDict = new Dictionary<GameObject, GameObject>();

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



    public void ToggleHelpfulUI(GameObject blockObject, List<string> helpfulUIList, bool updating)
    {
        GameObject uiObject;

        if (blockToHelpfulUIDict.TryGetValue(blockObject, out uiObject))
        {
            // If the blockObject is in the dictionary and the helpfulUIList is empty, destroy the UI object.
            if (helpfulUIList.Count == 0)
            {
                if (uiObject != null)
                {
                    Destroy(uiObject);
                    blockToHelpfulUIDict.Remove(blockObject);
                }
            }
            else if (updating)
            {
                // If the UI object already exists, update its text.
                TextMeshProUGUI textMesh = uiObject.GetComponent<TextMeshProUGUI>();
                if (textMesh != null)
                {
                    textMesh.text = string.Join("\n", helpfulUIList);
                }
            }
        }
        else
        {
            // If the blockObject is not in the dictionary and the helpfulUIList is not empty, instantiate a new UI object.
            if (helpfulUIList.Count > 0)
            {
                uiObject = Instantiate(helpfulContainerPrefab, blockObject.transform.position, Quaternion.identity, canvas.transform);
                blockToHelpfulUIDict[blockObject] = uiObject;

                // Set the text of the new UI object.
                TextMeshProUGUI textMesh = uiObject.GetComponent<TextMeshProUGUI>();
                if (textMesh != null)
                {
                    textMesh.text = string.Join("\n", helpfulUIList);
                }
            }
        }
    }

}
