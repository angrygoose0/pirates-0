using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public bool gameStarted = false;
    public GameObject startButton;
    void Awake()
    {
        gameStarted = false;
        startButton.SetActive(true);

    }

    public void StartGame()
    {
        gameStarted = true;
        startButton.SetActive(false);

        SingletonManager.Instance.cameraBrain.PlayCamera();
    }

    public void EndGame()
    {
        gameStarted = false;
        startButton.SetActive(true);
    }


}
