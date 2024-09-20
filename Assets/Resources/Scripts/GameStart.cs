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

    }

    public void StartGame()
    {
        gameStarted = true;
        startButton.SetActive(false);
    }


}
