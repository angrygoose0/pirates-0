using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonManager : MonoBehaviour
{
    private static SingletonManager _instance;
    public static SingletonManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SingletonManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SingletonManager");
                    _instance = singletonObject.AddComponent<SingletonManager>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    // Add references to other components you want to manage
    public ShipGenerator shipGenerator { get; private set; }
    public InteractionManager interactionManager { get; private set; }
    public WorldGenerator worldGenerator { get; private set; }
    public ShipMovement shipMovement { get; private set; }
    public Explosions explosions { get; private set; }
    public CannonBehaviour cannonBehaviour { get; private set; }
    public GoldManager goldManager { get; private set; }
    public ItemManager itemManager { get; private set; }
    public CreatureManager creatureManager { get; private set; }
    public StructureManager structureManager { get; private set; }
    public DayNightCycle dayNightCycle { get; private set; }
    public AbilityManager abilityManager { get; private set; }
    public UIManager uiManager { get; private set; }
    public FeedbackManager feedbackManager { get; private set; }
    public FishingLine fishingLine { get; private set; }
    public CameraBrain cameraBrain { get; private set; }
    public GameStart gameStart { get; private set; }
    public WaterShader waterShader { get; private set; }

    private void Awake()
    {
        // Singleton pattern logic
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize all the components attached to the GameObject
        shipGenerator = GetComponent<ShipGenerator>();
        interactionManager = GetComponent<InteractionManager>();
        worldGenerator = GetComponent<WorldGenerator>();
        shipMovement = GetComponent<ShipMovement>();
        explosions = GetComponent<Explosions>();
        cannonBehaviour = GetComponent<CannonBehaviour>();
        goldManager = GetComponent<GoldManager>();
        itemManager = GetComponent<ItemManager>();
        creatureManager = GetComponent<CreatureManager>();
        structureManager = GetComponent<StructureManager>();
        dayNightCycle = GetComponent<DayNightCycle>();
        abilityManager = GetComponent<AbilityManager>();
        uiManager = GetComponent<UIManager>();
        feedbackManager = GetComponent<FeedbackManager>();
        fishingLine = GetComponent<FishingLine>();
        cameraBrain = GetComponent<CameraBrain>();
        gameStart = GetComponent<GameStart>();
        waterShader = GetComponent<WaterShader>();

        // Debugging: Check if each component is missing
        if (shipGenerator == null) Debug.LogError("ShipGenerator script is missing!");
        if (interactionManager == null) Debug.LogError("InteractionManager script is missing!");
        if (worldGenerator == null) Debug.LogError("WorldGenerator script is missing!");
        if (shipMovement == null) Debug.LogError("ShipMovement script is missing!");
        if (explosions == null) Debug.LogError("Explosions script is missing!");
        if (cannonBehaviour == null) Debug.LogError("CannonBehaviour script is missing!");
        if (goldManager == null) Debug.LogError("GoldManager script is missing!");
        if (itemManager == null) Debug.LogError("ItemManager script is missing!");
        if (creatureManager == null) Debug.LogError("CreatureManager script is missing!");
        if (structureManager == null) Debug.LogError("StructureManager script is missing!");
        if (dayNightCycle == null) Debug.LogError("DayNightCycle script is missing!");
        if (abilityManager == null) Debug.LogError("AbilityManager script is missing!");
        if (uiManager == null) Debug.LogError("UIManager script is missing!");
        if (feedbackManager == null) Debug.LogError("FeedbackManager script is missing!");
        if (fishingLine == null) Debug.LogError("FishingLine script is missing!");
        if (cameraBrain == null) Debug.LogError("CameraBrain script is missing!");
        if (gameStart == null) Debug.LogError("GameStart script is missing!");
        if (waterShader == null) Debug.LogError("WaterShader script is missing!");
    }
}
