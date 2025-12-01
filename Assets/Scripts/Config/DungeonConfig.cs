using UnityEngine;

/// <summary>
/// ScriptableObject para armazenar configurações padrão da dungeon.
/// Pode ser criado no menu: Assets > Create > Dungeon Config
/// </summary>
[CreateAssetMenu(fileName = "NewDungeonConfig", menuName = "Dungeon/Config", order = 1)]
public class DungeonConfig : ScriptableObject
{
    [Header("Generation Settings")]
    [Range(5, 100)]
    public int defaultMaxRooms = 50;
    
    [Tooltip("Se 0, usa seed aleatório")]
    public int defaultSeed = 0;
    
    [Range(0f, 1f)]
    [Tooltip("Complexidade do grafo (0 = árvore simples, 1 = muito conectado)")]
    public float defaultComplexity = 0.3f;
    
    public bool defaultDirectedGraph = true;

    [Header("Cost Ranges")]
    public Vector2 healthCostRange = new Vector2(1, 5);
    public Vector2 sanityCostRange = new Vector2(1, 8);
    public Vector2 timeCostRange = new Vector2(1, 3);

    [Header("Room Distribution Chances")]
    [Range(0f, 1f)] public float treasureChanceInDeadEnd = 0.8f;
    [Range(0f, 1f)] public float treasureChanceInCorridor = 0.1f;
    [Range(0f, 1f)] public float campChance = 0.15f;
    [Range(0f, 1f)] public float eventChance = 0.1f;

    /// <summary>
    /// Aplica as configurações padrão ao GameSessionState
    /// </summary>
    public void ApplyToSessionState()
    {
        GameSessionState state = GameSessionState.Instance;
        state.maxRooms = defaultMaxRooms;
        state.randomSeed = defaultSeed;
        state.complexity = defaultComplexity;
        state.directedGraph = defaultDirectedGraph;
        
        state.healthCostRange = healthCostRange;
        state.sanityCostRange = sanityCostRange;
        state.timeCostRange = timeCostRange;
        
        state.treasureChanceInDeadEnd = treasureChanceInDeadEnd;
        state.treasureChanceInCorridor = treasureChanceInCorridor;
        state.campChance = campChance;
        state.eventChance = eventChance;
        
        Debug.Log($"[DungeonConfig] Applied '{name}' to GameSessionState");
    }
}
