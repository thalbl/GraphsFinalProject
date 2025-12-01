using UnityEngine;

/// <summary>
/// Singleton que persiste as configurações da dungeon entre recargas de cena.
/// Inspirado no sistema de save state do Persona 5.
/// </summary>
public class GameSessionState : MonoBehaviour
{
    private static GameSessionState _instance;
    
    public static GameSessionState Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameSessionState");
                _instance = go.AddComponent<GameSessionState>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Dungeon Generation Settings")]
    public int maxRooms = 50;
    public int randomSeed = 2020;
    public float complexity = 0.3f; // 0-1, mapeia para cycleChance
    public bool directedGraph = true;

    [Header("Cost Ranges")]
    public Vector2 healthCostRange = new Vector2(1, 5);
    public Vector2 sanityCostRange = new Vector2(1, 8);
    public Vector2 timeCostRange = new Vector2(1, 3);

    [Header("Room Distribution")]
    [Range(0f, 1f)] public float treasureChanceInDeadEnd = 0.8f;
    [Range(0f, 1f)] public float treasureChanceInCorridor = 0.1f;
    [Range(0f, 1f)] public float campChance = 0.15f;
    [Range(0f, 1f)] public float eventChance = 0.1f;

    private void Awake()
    {
        // Garante que só existe uma instância
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Atualiza todas as configurações de uma vez
    /// </summary>
    public void UpdateSettings(int rooms, int seed, float complexityValue, bool isDirected)
    {
        maxRooms = rooms;
        randomSeed = seed;
        complexity = Mathf.Clamp01(complexityValue);
        directedGraph = isDirected;
        
        Debug.Log($"[GameSessionState] Settings Updated: Rooms={rooms}, Seed={seed}, Complexity={complexity:F2}, Directed={isDirected}");
    }

    /// <summary>
    /// Aplica as configurações armazenadas no DungeonGenerator
    /// </summary>
    public void ApplyToDungeonGenerator(DungeonGenerator generator)
    {
        if (generator == null)
        {
            Debug.LogError("[GameSessionState] DungeonGenerator is null!");
            return;
        }

        generator.maxRooms = maxRooms;
        generator.randomSeed = randomSeed;
        generator.cycleChance = complexity;
        generator.directedGraph = directedGraph;
        
        // Aplica também as distribuições de sala
        generator.treasureChanceInDeadEnd = treasureChanceInDeadEnd;
        generator.treasureChanceInCorridor = treasureChanceInCorridor;
        generator.campChance = campChance;
        generator.eventChance = eventChance;

        Debug.Log($"[GameSessionState] Applied: rooms={maxRooms}, seed={randomSeed}, complexity={complexity:F2}, directed={directedGraph}");
    }

    /// <summary>
    /// Reseta para valores padrão
    /// </summary>
    public void ResetToDefaults()
    {
        maxRooms = 15;
        randomSeed = 12345;
        complexity = 0.3f;
        directedGraph = true;
        
        Debug.Log("[GameSessionState] Reset to default values");
    }
}
