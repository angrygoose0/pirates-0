using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class HealthBar : MonoBehaviour
{
    public Transform target; // The GameObject this UI element follows
    public Vector3 offset = new Vector3(0, -20, 0); // Offset in UI space
    private RectTransform healthBarRectTransform;
    private Canvas canvas;
    private List<Image> imagesList = new List<Image>();
    public float activeTime = 3f;
    public float fadeDuration = 3f; // Duration over which the images fade out
    private MMProgressBar hpBar;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Cache the RectTransform of the health bar
        healthBarRectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        hpBar = GetComponent<MMProgressBar>();

        imagesList.AddRange(GetComponentsInChildren<Image>());
        SetImagesAlpha(0f);
    }


    void LateUpdate()
    {
        if (target != null && canvas != null)
        {
            // Convert the world position of the target to screen space
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + offset);

            // Convert screen position to UI (Canvas) position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPoint);

            // Update the health bar's position
            healthBarRectTransform.localPosition = localPoint;
        }
    }

    public void Death()
    {
        Destroy(gameObject);
    }

    public void ModifyHealth(float impact)
    {
        hpBar.UpdateBar(impact, 0f, 1f);
        // If there's an existing fade coroutine, stop it
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // Reset the alpha of all images to 1 (fully visible)
        SetImagesAlpha(1f);

        // Start the fade-out coroutine
        fadeCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {

        // Wait for the active time before starting to fade out
        yield return new WaitForSeconds(activeTime);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            SetImagesAlpha(alpha);
            yield return null;
        }

        // Ensure the images are fully transparent at the end
        SetImagesAlpha(0f);

        // Print the deactive debug message
        Debug.Log("ModifyHealth deactive");
    }

    private void SetImagesAlpha(float alpha)
    {
        foreach (Image img in imagesList)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }
}
