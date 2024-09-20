using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Ship"))
        {
            SingletonManager.Instance.creatureManager.AttackShip(gameObject, collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player")) // temporary end game for when there is just one player, and the game ends when a creature touches the player.
        {
            //SingletonManager.Instance.
        }
    }
}
