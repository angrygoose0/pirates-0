using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using Cinemachine;


public class FeedbackManager : MonoBehaviour
{
    //ship damaged feedback values
    public MMF_Player shipDamagedFeedbackPlayer;
    private MMF_CinemachineImpulse cinemachineImpulseFeedback;
    public float shipDamagedCameraShakeMagnitude;

    void Start()
    {
        cinemachineImpulseFeedback = shipDamagedFeedbackPlayer.GetFeedbackOfType<MMF_CinemachineImpulse>();
    }


    public void ShipDamagedFeedback(float damageMagnitude)
    {
        float cameraShakeMagnitude = shipDamagedCameraShakeMagnitude * damageMagnitude;
        cinemachineImpulseFeedback.Velocity = new Vector3(cameraShakeMagnitude, cameraShakeMagnitude, cameraShakeMagnitude);
        shipDamagedFeedbackPlayer?.PlayFeedbacks();
    }


}
