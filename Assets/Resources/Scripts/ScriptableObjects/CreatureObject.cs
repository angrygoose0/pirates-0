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

[System.Serializable]
public class TentacleValue
{
    public List<float> segmentSizes;
    public float setDistance = 1.0f;
    public float acceleration;
    public float maxMoveSpeed;
    public float deceleration;

    public float pullStrength = 0.5f;
    public float wiggleFrequency = 2.0f;
    public float wiggleAmplitude = 0.5f;
    public bool endTarget;
}

[System.Serializable]
public class CreatureDrops
{
    public ItemObject droppedItemObject;
    public int dropWeight;
}

[CreateAssetMenu(fileName = "creatureObject", menuName = "Creature", order = 1)]
public class CreatureObject : ScriptableObject
{
    public float id;
    public float startingHealth;
    public float armor;
    public int populationValue;
    public int spawnWeight;
    public int minPackSpawn;
    public int maxPackSpawn;
    public List<CreatureValue> creatureValues;
    
    public List<CreatureDrops> creatureDrops;
    public int tentacles;

    public float damage;

    public float rotationSpeed;
    public float maxMoveSpeed;
    public float acceleration;
    public float deceleration;
    public int range;
    public float aggressionThreshold;
    public List<TentacleValue> tentacleList;
    public Vector2 goldDropRange;
    public float hostilityMultiplier;
    /* hostility
    how fast it goes up, 
    how much it goes down, depending on hp
    how much it goes down after attacking.
    */
}