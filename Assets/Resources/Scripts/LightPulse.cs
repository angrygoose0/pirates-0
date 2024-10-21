using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPulse : MonoBehaviour
{
    public float pulseSpeed = 1.0f; // Speed of the pulse
    public float minScale = 0.8f;   // Minimum scale
    public float maxScale = 1.2f;   // Maximum scale
    private Vector3 originalScale;  // To store the original scale of the object

    // Start is called before the first frame update
    void Start()
    {
        // Store the initial scale of the object
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new scale using a sine wave
        float scaleFactor = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f);

        // Apply the new scale to the object
        transform.localScale = originalScale * scaleFactor;
    }
}
