using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainManager : MonoBehaviour
{
    // Reference to the main camera
    private Camera mainCamera;

    // Reference to the particle system
    public ParticleSystem rainParticleSystem;

    // Base scale factor to apply when camera orthographic size is 1
    public float baseScaleFactor = 0.15f;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera != null && rainParticleSystem != null)
        {
            // Get the top edge position in world space
            Vector3 topEdgePosition = mainCamera.transform.position + new Vector3(0, mainCamera.orthographicSize, 0);

            // Set the particle system position to the top edge with dynamic offset
            rainParticleSystem.transform.position = new Vector3(
                mainCamera.transform.position.x,  // Match camera's x position
                topEdgePosition.y, // Align with the top edge and apply dynamic offset
                rainParticleSystem.transform.position.z // Keep the original z position
            );

            // Adjust the particle system's scale to stay consistent with the camera's zoom level
            float scaleMultiplier = mainCamera.orthographicSize * baseScaleFactor;
            rainParticleSystem.transform.localScale = Vector3.one * scaleMultiplier;
        }
    }
}
