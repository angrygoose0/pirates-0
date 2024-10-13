using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBrain : MonoBehaviour
{
    public CinemachineVirtualCamera editCamera;
    public CinemachineVirtualCamera playCamera;
    private CinemachineVirtualCamera currentCamera;


    public CinemachineVirtualCamera camera2;

    // List to store follow targets
    public List<Transform> followTargets = new List<Transform>();

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
            // Deactivate the previous camera by setting its priority to 0
            currentCamera.Priority = 0;
        }

        // Activate the new camera by setting its priority to a higher value
        newCamera.Priority = 10;

        // Update the current camera reference
        currentCamera = newCamera;
    }

    public void PlayCamera()
    {
        SwitchCamera(playCamera);
    }

    public void Camera2()
    {
        SwitchCamera(camera2);
        ChangeFollowTarget(2);
    }

    // Method to change the follow target based on an index
    public void ChangeFollowTarget(int index)
    {
        if (index >= 0 && index < followTargets.Count && currentCamera != null)
        {
            currentCamera.Follow = followTargets[index];
        }
        else
        {
            Debug.LogWarning("Invalid index or no camera available.");
        }
    }


}
