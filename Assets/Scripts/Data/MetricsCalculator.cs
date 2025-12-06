using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calcula métricas de desempenho do jogador usando conceitos de Teoria dos Grafos.
/// </summary>
public static class MetricsCalculator
{
    /// <summary>
    /// Gera todas as métricas baseadas no progresso do jogador.
    /// </summary>
    /// <param name="generator">Gerador do dungeon</param>
    /// <param name="pathHistory">Histórico completo de salas visitadas (com repetições)</param>
    /// <param name="visitedRoomIndices">Salas únicas visitadas</param>
    /// <param name="costsApplied">Custos aplicados em cada movimento</param>
    /// <param name="playerStats">Stats do jogador</param>
    /// <param name="victory">Se o jogador venceu</param>
    /// <returns>GameMetrics completo</returns>
    public static GameMetrics GenerateMetrics(
        DungeonGenerator generator,
        List<int> pathHistory,
        HashSet<int> visitedRoomIndices,
        List<EdgeConnectionData> costsApplied,
        PlayerStats playerStats,
        bool victory)
    {
        GameMetrics metrics = new GameMetrics();
        metrics.victory = victory;
        metrics.costsApplied = new List<EdgeConnectionData>(costsApplied); // (NOVO) Copia a lista

        // A. Path Optimality Ratio
        CalculateOptimalityRatio(generator, pathHistory, costsApplied, metrics);

        // B. Exploration Index
        CalculateExplorationIndex(generator.allRooms.Count, visitedRoomIndices, metrics);

        // C. Backtracking Cost
        CalculateBacktrackingCost(pathHistory, visitedRoomIndices, metrics);

        // D. Risk Profile
        CalculateRiskProfile(costsApplied, pathHistory.Count, metrics);

        // Traits adquiridos
        metrics.traitCount = playerStats.activeTraits.Count;
        metrics.traitsAcquired = new List<string>();
        foreach (var trait in playerStats.activeTraits)
        {
            metrics.traitsAcquired.Add($"• {trait.traitName}: {trait.description}");
        }

        // Gera feedbacks textuais
        metrics.GenerateAllFeedback();

        Debug.Log($"[MetricsCalculator] Métricas geradas:");
        Debug.Log($"  - Otimalidade: {metrics.pathOptimalityRatio:P0}");
        Debug.Log($"  - Exploração: {metrics.explorationIndex:P0}");
        Debug.Log($"  - Backtracking: {metrics.backtrackingCost} passos");
        Debug.Log($"  - Perfil: {metrics.riskProfile}");

        return metrics;
    }

    /// <summary>
    /// A. Calcula o fator de otimização comparando caminho do jogador com A*.
    /// Ratio = Custo A* / Custo do Jogador
    /// 1.0 = perfeito, < 0.5 = muito ineficiente
    /// </summary>
    private static void CalculateOptimalityRatio(
        DungeonGenerator generator,
        List<int> pathHistory,
        List<EdgeConnectionData> costsApplied,
        GameMetrics metrics)
    {
        // Calcula custo total do jogador
        float totalHealthCost = 0;
        float totalSanityCost = 0;
        float totalTimeCost = 0;

        foreach (var cost in costsApplied)
        {
            totalHealthCost += cost.costHealth;
            totalSanityCost += cost.costSanity;
            totalTimeCost += cost.costTime;
        }

        metrics.playerTotalCost = totalHealthCost + totalSanityCost + totalTimeCost;

        // Calcula custo do caminho ótimo (Spawn -> Boss) usando A*
        // Usaremos uma média dos 3 tipos de custo para uma comparação justa
        if (pathHistory.Count >= 2)
        {
            RoomNode spawnRoom = generator.spawnRoom;
            RoomNode bossRoom = generator.bossRoom;

            // Calcula A* para os 3 tipos de custo e faz média
            List<RoomNode> pathHealth = AStarPathfinder.FindPath(
                generator.dungeonGraph, spawnRoom, bossRoom, CostType.Health);
            List<RoomNode> pathSanity = AStarPathfinder.FindPath(
                generator.dungeonGraph, spawnRoom, bossRoom, CostType.Sanity);
            List<RoomNode> pathTime = AStarPathfinder.FindPath(
                generator.dungeonGraph, spawnRoom, bossRoom, CostType.Time);

            float optimalHealthCost = AStarPathfinder.CalculatePathCost(
                generator.dungeonGraph, pathHealth, CostType.Health);
            float optimalSanityCost = AStarPathfinder.CalculatePathCost(
                generator.dungeonGraph, pathSanity, CostType.Sanity);
            float optimalTimeCost = AStarPathfinder.CalculatePathCost(
                generator.dungeonGraph, pathTime, CostType.Time);

            metrics.optimalPathCost = optimalHealthCost + optimalSanityCost + optimalTimeCost;

            // Evita divisão por zero
            if (metrics.playerTotalCost > 0)
            {
                metrics.pathOptimalityRatio = metrics.optimalPathCost / metrics.playerTotalCost;
                // Limita a 1.0 (não pode ser melhor que o ótimo)
                metrics.pathOptimalityRatio = Mathf.Min(metrics.pathOptimalityRatio, 1.0f);
            }
            else
            {
                metrics.pathOptimalityRatio = 1.0f;
            }
        }
        else
        {
            // Jogador não se moveu
            metrics.optimalPathCost = 0;
            metrics.pathOptimalityRatio = 1.0f;
        }
    }

    /// <summary>
    /// B. Calcula o índice de exploração.
    /// Índice = Salas Visitadas / Total de Salas
    /// </summary>
    private static void CalculateExplorationIndex(
        int totalRooms,
        HashSet<int> visitedRoomIndices,
        GameMetrics metrics)
    {
        metrics.totalRooms = totalRooms;
        metrics.roomsVisited = visitedRoomIndices.Count;

        if (totalRooms > 0)
        {
            metrics.explorationIndex = (float)visitedRoomIndices.Count / totalRooms;
        }
        else
        {
            metrics.explorationIndex = 0f;
        }
    }

    /// <summary>
    /// C. Calcula o custo de backtracking.
    /// Backtracking = Total de Passos - Salas Únicas Visitadas
    /// Indica quantas vezes o jogador voltou atrás ou fez ciclos.
    /// </summary>
    private static void CalculateBacktrackingCost(
        List<int> pathHistory,
        HashSet<int> visitedRoomIndices,
        GameMetrics metrics)
    {
        metrics.totalSteps = pathHistory.Count;
        metrics.uniqueRoomsVisited = visitedRoomIndices.Count;
        
        // Backtracking = passos totais - salas únicas
        // Se visitei 10 salas mas dei 15 passos, fiz 5 movimentos de backtracking
        metrics.backtrackingCost = Mathf.Max(0, metrics.totalSteps - metrics.uniqueRoomsVisited);
    }

    /// <summary>
    /// D. Calcula o perfil de risco analisando os custos médios.
    /// Determina qual recurso o jogador mais protegeu/gastou.
    /// </summary>
    private static void CalculateRiskProfile(
        List<EdgeConnectionData> costsApplied,
        int totalSteps,
        GameMetrics metrics)
    {
        if (totalSteps == 0 || costsApplied.Count == 0)
        {
            metrics.avgHealthCostPerStep = 0;
            metrics.avgSanityCostPerStep = 0;
            metrics.avgTimeCostPerStep = 0;
            return;
        }

        float totalHealth = 0;
        float totalSanity = 0;
        float totalTime = 0;

        foreach (var cost in costsApplied)
        {
            totalHealth += cost.costHealth;
            totalSanity += cost.costSanity;
            totalTime += cost.costTime;
        }

        // Calcula médias
        metrics.avgHealthCostPerStep = totalHealth / costsApplied.Count;
        metrics.avgSanityCostPerStep = totalSanity / costsApplied.Count;
        metrics.avgTimeCostPerStep = totalTime / costsApplied.Count;
    }

    /// <summary>
    /// Calcula estatísticas adicionais para debug/análise.
    /// </summary>
    public static string GetDetailedAnalysis(GameMetrics metrics, DungeonGenerator generator)
    {
        int leafRooms = 0;
        int corridorRooms = 0;
        int hubRooms = 0;

        foreach (var room in generator.allRooms)
        {
            int connections = room.connections.Count;
            if (connections == 1)
                leafRooms++;
            else if (connections == 2)
                corridorRooms++;
            else
                hubRooms++;
        }

        return $@"
=== ANÁLISE DETALHADA ===

ESTRUTURA DO GRAFO:
- Total de Salas: {generator.allRooms.Count}
- Folhas (1 conexão): {leafRooms}
- Corredores (2 conexões): {corridorRooms}
- Hubs (3+ conexões): {hubRooms}

CAMINHO DO JOGADOR:
- Eficiência: {metrics.pathOptimalityRatio:P0}
- Exploração: {metrics.explorationIndex:P0}
- Backtracking Rate: {(metrics.backtrackingCost / (float)metrics.totalSteps):P0}

CUSTOS MÉDIOS POR PASSO:
- Vida: {metrics.avgHealthCostPerStep:F2}
- Sanidade: {metrics.avgSanityCostPerStep:F2}
- Tempo: {metrics.avgTimeCostPerStep:F2}
";
    }
}
