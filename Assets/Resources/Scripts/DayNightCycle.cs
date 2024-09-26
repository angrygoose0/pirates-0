using UnityEngine;
using TMPro;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    public Light2D globalLight;
    public Light2D shipLight;
    public float dayDuration = 120f; // Duration of a full day in seconds

    public TextMeshProUGUI timerText; // Reference to the UI Text component that displays the timer
    public float elapsedTime = 0f;
    public float lightLevel = 0f;
    public float difficultyFactor = 0f;
    public float dampingConstant = 0.1f;


    void Start()
    {
    }
    void Update()
    {
        if (SingletonManager.Instance.gameStart.gameStarted)
        {
            elapsedTime += Time.deltaTime;
        }
        // Increment elapsed time


        // Update the timer text
        UpdateTimer();

        // Calculate the light level based on the elapsed time
        lightLevel = (Mathf.Sin((elapsedTime / dayDuration) * 2f * Mathf.PI) + 1f) / 2f;
        globalLight.intensity = lightLevel;

        shipLight.intensity = 1 - lightLevel;

        // Calculate the difficulty factor
        difficultyFactor = (1f - lightLevel) * elapsedTime * dampingConstant;
        //SingletonManager.Instance.creatureManager.maxGlobalMobCount = Mathf.RoundToInt(difficultyFactor);
    }

    void UpdateTimer()
    {
        int minutes = (int)(elapsedTime / 60);
        int seconds = (int)(elapsedTime % 60);
        int milliseconds = (int)((elapsedTime * 1000) % 1000);

        timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }
}
