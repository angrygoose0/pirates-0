using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBrain : MonoBehaviour
{
    public CinemachineVirtualCamera editCamera;
    public CinemachineVirtualCamera playCamera;
    private CinemachineVirtualCamera currentCamera;

    void Start()
    {
        ResetAllCameraProperties();
        SwitchCamera(editCamera);
    }
    public void ResetAllCameraProperties()
    {
        CinemachineVirtualCamera[] allCameras = FindObjectsOfType<CinemachineVirtualCamera>();

        // Loop through each camera and set its priority to 0
        foreach (CinemachineVirtualCamera cam in allCameras)
        {
            cam.Priority = 0;
        }
    }
    private void SwitchCamera(CinemachineVirtualCamera newCamera)
    {
        if (currentCamera != null)
        {
            // Set the previous camera's priority to a lower value to deactivate it
            currentCamera.Priority = 0;
        }

        // Set the new camera's priority to a higher value to activate it
        newCamera.Priority = 10;

        // Update the current camera reference
        currentCamera = newCamera;
    }

    public void PlayCamera()
    {
        SwitchCamera(playCamera);
    }
}
