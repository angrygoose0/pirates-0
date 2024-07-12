using UnityEngine;


public class DayNightCycle : MonoBehaviour
{
    public UnityEngine.Rendering.Universal.Light2D globalLight;
    public float dayDuration = 120f; // Duration of a full day in seconds

    private float currentTime = 0f;

    void Update()
    {
        // Increment time
        currentTime += Time.deltaTime;

        // Calculate the time of day (0 to 1)
        float timeOfDay = (currentTime % dayDuration) / dayDuration;

        // Set light intensity and color based on time of day
        if (timeOfDay < 0.25f) // Dawn
        {
            globalLight.intensity = Mathf.Lerp(0.1f, 1f, timeOfDay / 0.25f);
        }
        else if (timeOfDay < 0.5f) // Day
        {
            globalLight.intensity = Mathf.Lerp(1f, 1.2f, (timeOfDay - 0.25f) / 0.25f);
        }
        else if (timeOfDay < 0.75f) // Dusk
        {
            globalLight.intensity = Mathf.Lerp(1.2f, 0.5f, (timeOfDay - 0.5f) / 0.25f);
        }
        else // Night
        {
            globalLight.intensity = Mathf.Lerp(0.5f, 0.1f, (timeOfDay - 0.75f) / 0.25f);
        }
    }
}
