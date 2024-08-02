using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public enum Ability
{
    LifeSteal,
    Haste,
    Nova, //light explosion
    Multiple, //multiple bullets fired
    Damage,
    // Add more states here as needed
}

[System.Serializable]
public class AbilityData
{
    public Ability ability;
    public int tier;
}

public class AbilityManager : MonoBehaviour
{
    public List<AbilityData> abilityList = new List<AbilityData>();
    public float lifeStealValue;
    public float hasteValue;




    public AbilityData GetAbilityData(Ability ability)
    {
        return abilityList.Find(a => a.ability == ability);
    }


    // Call this method to add an ability to the list while ensuring no duplicates
    public void AddOrUpdateAbility(Ability newAbility, int newTier)
    {
        bool abilityExists = false;

        for (int i = 0; i < abilityList.Count; i++)
        {
            if (abilityList[i].ability == newAbility)
            {
                abilityExists = true;
                abilityList[i].tier = newTier;
                break;
            }
        }

        // If the ability doesn't exist in the list, add it
        if (!abilityExists)
        {
            abilityList.Add(new AbilityData { ability = newAbility, tier = newTier });
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Example usage
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddOrUpdateAbility(Ability.LifeSteal, 3);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            AddOrUpdateAbility(Ability.Haste, 2);
        }
    }
}
