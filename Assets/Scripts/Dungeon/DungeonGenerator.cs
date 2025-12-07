using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Orquestrador principal da geração de dungeon.
/// Coordena os módulos especializados para criar o dungeon completo.
/// </summary>
public class DungeonGenerator : MonoBehaviour {

    public System.Action OnDungeonGenerated;

    [Header("Generation Settings")]
    public int maxRooms = 15;
    [Tooltip("Seed para geração determinística. Se 0, usa seed aleatório baseado no tempo.")]
    public int randomSeed = 0;
    [HideInInspector] public int actualSeed; // Seed realmente usada (útil quando randomSeed = 0)
    public bool generateOnStart = true;

    [Header("Room Sizes")]
    public Vector2 spawnRoomSize = new Vector2(12, 12);
    public Vector2 bossRoomSize = new Vector2(16, 16);
    public Vector2 normalRoomSize = new Vector2(8, 8);
    public float minGapBetweenRooms = 2f;

    [Header("Visual Settings")]
    [Tooltip("Material base para arestas (será instanciado para cada linha)")]
    public Material edgeMaterial;
    
    public Color spawnColor = Color.green;
    public Color bossColor = Color.red;
    public Color combatColor = Color.white;
    public Color treasureColor = Color.yellow;
    public Color campColor = Color.blue;
    public Color eventColor = Color.magenta;

    [Header("Cost Settings")]
    [SerializeField] private Vector2 healthCostRange = new Vector2(1, 5);
    [SerializeField] private Vector2 sanityCostRange = new Vector2(1, 8);
    [SerializeField] private Vector2 timeCostRange = new Vector2(1, 3);
    public bool directedGraph = false;

    [Header("Graph Complexity")]
    [Range(0f, 1f)]
    public float cycleChance = 0.4f;

    [Header("Room Distribution Chances")]
    [Range(0f, 1f)] public float treasureChanceInDeadEnd = 0.8f;
    [Range(0f, 1f)] public float treasureChanceInCorridor = 0.1f;
    [Range(0f, 1f)] public float campChance = 0.15f;
    [Range(0f, 1f)] public float eventChance = 0.1f;

    [Header("System References")]
    public CameraController mainCamera;
    public DungeonGraph dungeonGraph;

    // Dados gerados (públicos para acesso externo)
    [HideInInspector] public List<RoomNode> allRooms;
    [HideInInspector] public RoomNode spawnRoom;
    [HideInInspector] public RoomNode bossRoom;

    // Dados privados
    private Dictionary<Vector2Int, RoomNode> occupiedPositions;
    private GameObject dungeonContainer;
    private Dictionary<(RoomNode, RoomNode), EdgeData> edgeCosts;
    private System.Random prng;

    // Módulos (visualizer é público para permitir destacar arestas)
    private DungeonGraphBuilder graphBuilder;
    private RoomTypeAssigner typeAssigner;
    private DungeonLayoutManager layoutManager;
    private EdgeCostGenerator costGenerator;
    public DungeonVisualizer visualizer { get; private set; }

    void Start() {
        if (generateOnStart) GenerateDungeon();
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon() {
        // Determina a seed a usar (0 = aleatório baseado no tempo)
        if (randomSeed == 0)
        {
            actualSeed = Environment.TickCount;
            Debug.Log($"[DungeonGenerator] Seed aleatória gerada: {actualSeed}");
        }
        else
        {
            actualSeed = randomSeed;
        }
        
        // Inicializa gerador de números aleatórios com a seed determinada
        prng = new System.Random(actualSeed);

        // Limpa dungeon anterior
        if (dungeonContainer != null) {
            // Limpa materiais instanciados antes de destruir
            if (visualizer != null)
            {
                visualizer.Cleanup();
            }
            
            if (Application.isPlaying)
                Destroy(dungeonContainer);
            else
                DestroyImmediate(dungeonContainer);
        }

        dungeonContainer = new GameObject("Dungeon");

        // Inicializa DungeonGraph se disponível
        if (dungeonGraph != null) dungeonGraph.Initialize();

        // === PIPELINE DE GERAÇÃO MODULAR ===

        // 1. GRAFO: Estrutura base (DFS) + Ciclos
        graphBuilder = new DungeonGraphBuilder(prng, maxRooms, cycleChance);
        spawnRoom = graphBuilder.GenerateRoomGraph(out occupiedPositions, out allRooms);
        graphBuilder.AddCycles(allRooms, occupiedPositions);
        graphBuilder.CalculateDistancesFromStart(spawnRoom, allRooms);

        // 2. TIPOS: Atribuição inteligente de tipos de sala
        typeAssigner = new RoomTypeAssigner(
            prng,
            treasureChanceInDeadEnd,
            treasureChanceInCorridor,
            campChance,
            eventChance
        );
        bossRoom = typeAssigner.AssignRoomTypes(allRooms, spawnRoom);

        // 3. LAYOUT: Posicionamento físico e naturalização
        layoutManager = new DungeonLayoutManager(
            spawnRoomSize,
            bossRoomSize,
            normalRoomSize,
            minGapBetweenRooms
        );
        layoutManager.ApplyPhysicalLayout(allRooms);
        layoutManager.NaturalizeLayout(allRooms, spawnRoom);

        // 4. CUSTOS: Gera custos de arestas
        costGenerator = new EdgeCostGenerator(
            prng,
            healthCostRange,
            sanityCostRange,
            timeCostRange,
            directedGraph
        );
        edgeCosts = costGenerator.GenerateEdgeCosts(allRooms, dungeonGraph);

        // 5. VISUALIZAÇÃO: Cria representação visual
        visualizer = new DungeonVisualizer(
            dungeonContainer,
            edgeMaterial,
            directedGraph,
            spawnColor, bossColor, combatColor,
            treasureColor, campColor, eventColor
        );
        visualizer.CreateVisualRepresentation(allRooms);

        Debug.Log($"[DungeonGenerator] Dungeon gerado: {allRooms.Count} salas.");

        OnDungeonGenerated?.Invoke();
    }

    /// <summary>
    /// API pública para obter custo de uma aresta
    /// </summary>
    public EdgeData GetEdgeCost(RoomNode from, RoomNode to)
    {
        if (edgeCosts != null && edgeCosts.TryGetValue((from, to), out EdgeData data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// API pública para obter cor de uma sala
    /// </summary>
    public Color GetRoomColor(RoomNode room)
    {
        return room.roomType switch
        {
            RoomType.Spawn => spawnColor,
            RoomType.Boss => bossColor,
            RoomType.Treasure => treasureColor,
            RoomType.Camp => campColor,
            RoomType.Event => eventColor,
            _ => combatColor
        };
    }

    /// <summary>
    /// Destaca as arestas acessíveis a partir da sala atual do jogador
    /// </summary>
    public void HighlightPlayerAccessibleEdges(RoomNode playerCurrentRoom)
    {
        if (visualizer != null)
        {
            visualizer.HighlightAccessibleEdges(playerCurrentRoom, allRooms);
        }
    }

    /// <summary>
    /// Reseta todas as arestas para cor padrão
    /// </summary>
    public void ResetEdgeHighlights()
    {
        if (visualizer != null)
        {
            visualizer.ResetAllEdgeColors();
        }
    }

    public EdgeData GetEdgeCost(RoomNode from, RoomNode to)
    {
        if (edgeCosts.TryGetValue((from, to), out EdgeData data))
        {
            return data;
        }
        return null; // Não existe aresta lógica
    }
    
}