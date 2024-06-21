using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    [SerializeField]
    private int goldCount;
    public TextMeshProUGUI goldText;

    void Start()
    {
        // Initialize the UI text with the current gold count
        UpdateGoldUI();
    }

    // Call this method whenever the gold count changes
    public void AddGold(int amount)
    {
        goldCount += amount;
        if (goldCount < 0)
        {
            goldCount = 0; // Ensure gold count does not go below zero
        }
        UpdateGoldUI();
    }

    void UpdateGoldUI()
    {
        goldText.text = goldCount.ToString();
    }
}

