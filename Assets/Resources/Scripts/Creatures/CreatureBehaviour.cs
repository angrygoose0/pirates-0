using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviour : MonoBehaviour
{

    public CreatureManager creatureManager;
    public void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag("Ship"))
        {
            creatureManager.AttackShip(gameObject);
        }
    }
}
