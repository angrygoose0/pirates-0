using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public enum Ability
{
    LifeSteal,
    Haste,
    Nova, // light explosion
    Multiple, // multiple bullets fired
    Might, // damage done
    Fragility, // damage received
    Health,
    Extra,
    Bleed,
    HealOrb,
    // Add more states here as needed
}

[System.Serializable]
public class AbilityData
{
    public Ability ability;
    public float value;
}

public class AbilityManager : MonoBehaviour
{
    public List<AbilityData> abilityList = new List<AbilityData>();

    public AbilityData GetAbilityData(Ability ability)
    {
        return abilityList.Find(a => a.ability == ability);
    }

    // Call this method to add an ability to the list while ensuring no duplicates and adding tiers if already exists
    // this replaces that ability instance with a new value for artifacts are attatched or removed.
    public void AddOrUpdateAbility(Ability newAbility, float newValue)
    {
        bool abilityExists = false;

        for (int i = 0; i < abilityList.Count; i++)
        {
            if (abilityList[i].ability == newAbility)
            {
                abilityExists = true;
                abilityList[i].value += newValue; // Add the tiers together
                break;
            }
        }

        // If the ability doesn't exist in the list, add it
        if (!abilityExists)
        {
            abilityList.Add(new AbilityData { ability = newAbility, value = newValue });
        }
    }
}
