using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estrutura principal de save que contém todos os dados necessários para
/// salvar e carregar uma partida usando a estratégia Seed + Delta State.
/// </summary>
[System.Serializable]
public class GameSaveData
{
    public DungeonGenerationData dungeonData;
    public List<EdgeConnectionData> edgeConnections;
    public PlayerStateData playerState;
    public GameProgressData progressData;
    public string saveDate;
    public int saveVersion = 1;

    public GameSaveData()
    {
        saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// Parâmetros de geração do dungeon.
/// Permite recriar o mapa exato usando a mesma seed.
/// </summary>
[System.Serializable]
public class DungeonGenerationData
{
    public int seed;
    public int maxRooms;
    public float cycleChance;
    public bool directedGraph;

    // Room distribution chances
    public float treasureChanceInDeadEnd;
    public float treasureChanceInCorridor;
    public float campChance;
    public float eventChance;

    // Cost ranges
    public Vector2 healthCostRange;
    public Vector2 sanityCostRange;
    public Vector2 timeCostRange;

    public DungeonGenerationData() { }

    public DungeonGenerationData(DungeonGenerator generator)
    {
        // Usa actualSeed para salvar a seed real usada (importante quando randomSeed = 0)
        seed = generator.actualSeed;
        maxRooms = generator.maxRooms;
        cycleChance = generator.cycleChance;
        directedGraph = generator.directedGraph;

        treasureChanceInDeadEnd = generator.treasureChanceInDeadEnd;
        treasureChanceInCorridor = generator.treasureChanceInCorridor;
        campChance = generator.campChance;
        eventChance = generator.eventChance;

        // Note: Cost ranges are private in DungeonGenerator
        // Will need to be exposed or accessed via reflection
        healthCostRange = new Vector2(1, 5);
        sanityCostRange = new Vector2(1, 8);
        timeCostRange = new Vector2(1, 3);
    }
}

/// <summary>
/// Representa uma conexão (aresta) entre duas salas usando índices.
/// Não salvamos referências de RoomNode, mas sim índices na lista allRooms.
/// </summary>
[System.Serializable]
public class EdgeConnectionData
{
    public int fromRoomIndex;
    public int toRoomIndex;
    public float costHealth;
    public float costSanity;
    public float costTime;
    public string description;

    public EdgeConnectionData() { }

    public EdgeConnectionData(int fromIndex, int toIndex, EdgeData edgeData)
    {
        fromRoomIndex = fromIndex;
        toRoomIndex = toIndex;
        costHealth = edgeData.costHealth;
        costSanity = edgeData.costSanity;
        costTime = edgeData.costTime;
        description = edgeData.description;
    }

    public EdgeData ToEdgeData()
    {
        return new EdgeData(costHealth, costSanity, costTime, description);
    }
}

/// <summary>
/// Estado atual do jogador (posição, recursos, traits).
/// </summary>
[System.Serializable]
public class PlayerStateData
{
    public int currentRoomIndex;
    
    public float currentHealth;
    public float maxHealth;
    public float currentSanity;
    public float maxSanity;
    public float currentSupplies;
    public float maxSupplies;

    public List<SerializedTrait> activeTraits;

    public PlayerStateData() 
    {
        activeTraits = new List<SerializedTrait>();
    }

    public PlayerStateData(int roomIndex, PlayerStats stats)
    {
        currentRoomIndex = roomIndex;
        
        currentHealth = stats.currentHealth;
        maxHealth = stats.maxHealth;
        currentSanity = stats.currentSanity;
        maxSanity = stats.maxSanity;
        currentSupplies = stats.currentSupplies;
        maxSupplies = stats.maxSupplies;

        activeTraits = new List<SerializedTrait>();
        foreach (var trait in stats.activeTraits)
        {
            activeTraits.Add(new SerializedTrait(trait));
        }
    }
}

/// <summary>
/// Versão serializável de Trait para salvar em JSON.
/// </summary>
[System.Serializable]
public class SerializedTrait
{
    public string traitName;
    public string description;
    public bool isAffliction;
    public float healthCostMultiplier;
    public float sanityCostMultiplier;
    public float timeCostMultiplier;

    public SerializedTrait() { }

    public SerializedTrait(Trait trait)
    {
        traitName = trait.traitName;
        description = trait.description;
        isAffliction = trait.isAffliction;
        healthCostMultiplier = trait.healthCostMultiplier;
        sanityCostMultiplier = trait.sanityCostMultiplier;
        timeCostMultiplier = trait.timeCostMultiplier;
    }

    public Trait ToTrait()
    {
        return new Trait(
            traitName,
            description,
            healthCostMultiplier,
            sanityCostMultiplier,
            timeCostMultiplier,
            isAffliction
        );
    }
}

/// <summary>
/// Histórico de progresso do jogador na dungeon.
/// Usado para calcular métricas e reconstruir o caminho percorrido.
/// </summary>
[System.Serializable]
public class GameProgressData
{
    public List<int> visitedRoomIndices;  // Salas únicas visitadas
    public List<int> pathHistory;         // Caminho completo (pode ter repetições)
    public int totalSteps;                // Total de movimentos feitos

    // Histórico de custos aplicados (para análise de perfil de risco)
    public List<EdgeConnectionData> costsApplied;

    public GameProgressData()
    {
        visitedRoomIndices = new List<int>();
        pathHistory = new List<int>();
        costsApplied = new List<EdgeConnectionData>();
        totalSteps = 0;
    }
}
