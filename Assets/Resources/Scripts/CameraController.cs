using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Reference to the Cinemachine Virtual Camera
    public float zoomSpeed = 1f; // Speed at which the camera zooms in and out
    public float minZoom = 2f; // Minimum orthographic size
    public float maxZoom = 10f; // Maximum orthographic size

    // Start is called before the first frame update
    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleZoom();
    }

    void HandleZoom()
    {
        if (virtualCamera != null)
        {
            // Get the current orthographic size of the camera
            float currentOrthoSize = virtualCamera.m_Lens.OrthographicSize;

            // Get the input from the scroll wheel (positive for zooming out, negative for zooming in)
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");

            // Calculate the new orthographic size based on the scroll input and zoom speed
            float newOrthoSize = currentOrthoSize - scrollInput * zoomSpeed;

            // Clamp the orthographic size to stay within the min and max zoom limits
            newOrthoSize = Mathf.Clamp(newOrthoSize, minZoom, maxZoom);

            // Set the new orthographic size
            virtualCamera.m_Lens.OrthographicSize = newOrthoSize;
        }
    }
}
