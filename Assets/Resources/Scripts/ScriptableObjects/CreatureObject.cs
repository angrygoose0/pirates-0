using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class CreatureValue
{
    public string name;
    public float value;

    public CreatureValue(string name, float value)
    {
        this.name = name;
        this.value = value;
    }
}

[CreateAssetMenu(fileName = "Block", menuName = "Creature", order = 1)]
public class CreatureObject : ScriptableObject
{
    public float id;
    public float hp;
    public float armor;
    public int populationValue;
    public int spawnWeight;
    public int minPackSpawn;
    public int maxPackSpawn;
    public List<CreatureValue> creatureValues;
    public int tentacles;

    public float rotationSpeed;
    public float maxMoveSpeed;
    public float acceleration;
    public float deceleration;
    public int range;
    public float aggressionThreshold;
}