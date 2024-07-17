using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DamageFlash : MonoBehaviour
{
    public Color flashColor = Color.white;
    public float flashTime = 0.25f;

    public void Flash(GameObject gameObject)
    {
        Material material = null;

        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            material = spriteRenderer.material;
        }
        else
        {
            TilemapRenderer tilemapRenderer = gameObject.GetComponent<TilemapRenderer>();
            if (tilemapRenderer != null)
            {
                material = tilemapRenderer.material;
            }
        }

        if (material != null)
        {
            material.SetColor("_FlashColor", flashColor);
            StartCoroutine(FlashCoroutine(material));
        }
        else
        {
            Debug.LogWarning("GameObject does not have a SpriteRenderer or TilemapRenderer component.");
        }
    }

    private IEnumerator FlashCoroutine(Material material)
    {
        float currentFlashAmount;
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            currentFlashAmount = Mathf.Lerp(2f, 0f, elapsedTime / flashTime);
            material.SetFloat("_FlashAmount", currentFlashAmount);
            yield return null;
        }
        material.SetFloat("_FlashAmount", 0f);
    }
}
