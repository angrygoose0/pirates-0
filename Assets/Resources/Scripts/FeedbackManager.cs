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
    public MMF_Player explosionFeedbackPlayer;
    public MMF_Player artifactPlaceFeedbackPlayer;
    public Transform explosionParticle;

    public float shipDamagedCameraShakeMagnitude;


    public void ShipDamagedFeedback(float damageMagnitude)
    {
        float cameraShakeMagnitude = shipDamagedCameraShakeMagnitude * damageMagnitude;
        MMF_CinemachineImpulse cinemachineImpulseFeedback = shipDamagedFeedbackPlayer.GetFeedbackOfType<MMF_CinemachineImpulse>();
        cinemachineImpulseFeedback.Velocity = new Vector3(cameraShakeMagnitude, cameraShakeMagnitude, cameraShakeMagnitude);
        shipDamagedFeedbackPlayer?.PlayFeedbacks();
    }

    public void ArtifactPlaceFeedback(Vector3 position, float multiplier)
    {
        Debug.Log("playing artifact");
        MMF_ParticlesInstantiation artifactPlaceParticlesFeedback = artifactPlaceFeedbackPlayer.GetFeedbackOfType<MMF_ParticlesInstantiation>();
        artifactPlaceParticlesFeedback.TargetWorldPosition = position;

        artifactPlaceFeedbackPlayer?.PlayFeedbacks();
    }

    public void ExplosionFeedback(Vector3 position, float shakeMagnitude)
    {
        MMF_ParticlesInstantiation explosionParticlesFeedback = explosionFeedbackPlayer.GetFeedbackOfType<MMF_ParticlesInstantiation>();
        explosionParticlesFeedback.TargetWorldPosition = position;

        Debug.Log(explosionParticlesFeedback.ParticlesPrefab.transform);
        Vector3 explosionSize = new Vector3(2f, 1f, 1f);
        ScaleTransformAndChildren(explosionParticle, explosionSize);

        float cameraShakeMagnitude = shakeMagnitude * 1.5f;
        MMF_CinemachineImpulse cinemachineImpulseFeedback = explosionFeedbackPlayer.GetFeedbackOfType<MMF_CinemachineImpulse>();
        cinemachineImpulseFeedback.Velocity = new Vector3(cameraShakeMagnitude, cameraShakeMagnitude, cameraShakeMagnitude);

        explosionFeedbackPlayer?.PlayFeedbacks();



    }

    public void ScaleTransformAndChildren(Transform parent, Vector3 newScale)
    {
        parent.localScale = newScale;

        foreach (Transform child in parent)
        {
            ScaleTransformAndChildren(child, newScale);
        }
    }


}
