using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public bool gameStarted = false;
    public bool trailer;
    public GameObject startButton;
    public ProjectileData projectileData;
    public Transform trailerCameraAnchor;
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

        if (!trailer)
        {
            SingletonManager.Instance.shipMovement.currentVelocity = new Vector2(0.5f, 0.5f);
        }

        if (trailer)
        {
            StartCoroutine(PlayTrailerSequence());
        }
    }

    IEnumerator PlayTrailerSequence()
    {
        SingletonManager.Instance.cameraBrain.ChangeFollowTarget(0);
        yield return new WaitForSeconds(2f);
        CreatureData creatureData = SingletonManager.Instance.creatureManager.Trailer1();
        // First action: Wait for 2 seconds before doing something

        float accuracy = 1 / projectileData.accuracy;
        List<Vector3Int> tilesList = SingletonManager.Instance.interactionManager.GetSurroundingTiles(creatureData.currentTilePosition, accuracy);

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < 10; i++)
        {
            for (int o = 0; o < projectileData.fireAmount; o++)
            {
                Vector3Int targetTile = tilesList[Random.Range(0, tilesList.Count)];
                SingletonManager.Instance.cannonBehaviour.FireInTheHole(Vector3.zero, targetTile, projectileData, 2f);
            }

            // Wait for 0.01 seconds before the next iteration
            yield return new WaitForSeconds(0.1f);
        }
        // Second action: Wait for 5 seconds and then do something else
        yield return new WaitForSeconds(2f);

        SingletonManager.Instance.cameraBrain.Camera2();

    }




    //public void FireInTheHole(Vector3 startCoordinate, Vector3Int endTileCoordinate, ProjectileData projectile, float mass)

    public void EndGame()
    {
        gameStarted = false;
        startButton.SetActive(true);
    }


}
