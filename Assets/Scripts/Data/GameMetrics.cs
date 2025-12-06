using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe que armazena as métricas finais do jogador.
/// Utiliza conceitos de Teoria dos Grafos para avaliar o desempenho.
/// </summary>
[System.Serializable]
public class GameMetrics
{
    [Header("Path Optimality")]
    public float pathOptimalityRatio;        // Custo A* / Custo do jogador (1.0 = perfeito)
    public float playerTotalCost;            // Custo total gasto pelo jogador
    public float optimalPathCost;            // Custo do caminho ótimo (A*)
    public string optimalityFeedback;        // Feedback textual

    [Header("Exploration")]
    public float explorationIndex;           // Salas visitadas / Total de salas
    public int roomsVisited;
    public int totalRooms;
    public string explorationProfile;        // "Explorador Complecionista", "Pragmático", etc.

    [Header("Backtracking")]
    public int backtrackingCost;             // Total de passos - Salas únicas visitadas
    public int totalSteps;
    public int uniqueRoomsVisited;
    public string backtrackingAnalysis;      // Análise do backtracking

    [Header("Risk Profile")]
    public float avgHealthCostPerStep;       // Média de vida gasta por passo
    public float avgSanityCostPerStep;       // Média de sanidade gasta por passo
    public float avgTimeCostPerStep;         // Média de tempo gasto por passo
    public string riskProfile;               // "O Mártir", "O Louco", "O Lento"

    [Header("Game Info")]
    public bool victory;                     // Vitória ou derrota
    public int traitCount;                   // Número de traits ganhos
    public List<string> traitsAcquired;      // Lista de traits adquiridos
    public List<EdgeConnectionData> costsApplied; // (NOVO) Lista detalhada de custos

    public GameMetrics()
    {
        traitsAcquired = new List<string>();
        costsApplied = new List<EdgeConnectionData>();
    }

    /// <summary>
    /// Gera feedback textual para Path Optimality.
    /// </summary>
    public void GenerateOptimalityFeedback()
    {
        if (pathOptimalityRatio >= 0.95f)
        {
            optimalityFeedback = "Perfeito! Você jogou como uma máquina.";
        }
        else if (pathOptimalityRatio >= 0.8f)
        {
            optimalityFeedback = "Excelente! Suas escolhas foram muito eficientes.";
        }
        else if (pathOptimalityRatio >= 0.6f)
        {
            optimalityFeedback = "Bom. Houve algumas rotas subótimas, mas no geral bem.";
        }
        else if (pathOptimalityRatio >= 0.4f)
        {
            optimalityFeedback = "Razoável. Muitas escolhas custosas foram feitas.";
        }
        else
        {
            optimalityFeedback = "Ineficiente. Você deu muitas voltas ou escolheu caminhos caros.";
        }
    }

    /// <summary>
    /// Gera perfil de exploração baseado no índice.
    /// </summary>
    public void GenerateExplorationProfile()
    {
        if (explorationIndex >= 0.9f)
        {
            explorationProfile = "Explorador Complecionista";
        }
        else if (explorationIndex >= 0.6f)
        {
            explorationProfile = "Aventureiro Curioso";
        }
        else if (explorationIndex >= 0.4f)
        {
            explorationProfile = "Pragmático";
        }
        else
        {
            explorationProfile = "Rushador";
        }
    }

    /// <summary>
    /// Gera análise de backtracking.
    /// </summary>
    public void GenerateBacktrackingAnalysis()
    {
        float backtrackRatio = uniqueRoomsVisited > 0 ? (float)backtrackingCost / uniqueRoomsVisited : 0;

        if (backtrackRatio <= 0.1f)
        {
            backtrackingAnalysis = "Caminho quase linear. Decisões muito confiantes.";
        }
        else if (backtrackRatio <= 0.3f)
        {
            backtrackingAnalysis = "Pouco backtracking. Bom senso de direção.";
        }
        else if (backtrackRatio <= 0.6f)
        {
            backtrackingAnalysis = "Backtracking moderado. Algumas indecisões ou becos sem saída.";
        }
        else
        {
            backtrackingAnalysis = "Alto backtracking. Muitas voltas ou dungeon com becos sem saída.";
        }
    }

    /// <summary>
    /// Gera perfil de risco baseado nas médias de custo.
    /// Foca em qual recurso o jogador GASTOU mais, não qual poupou.
    /// </summary>
    public void GenerateRiskProfile()
    {
        // Determina qual recurso foi mais gasto (proporcional ao range máximo)
        // Normaliza pelo range: Vida (1-5), Sanidade (1-8), Tempo (1-3)
        float healthRatio = avgHealthCostPerStep / 5f;   // Normalizado para 0-1
        float sanityRatio = avgSanityCostPerStep / 8f;   // Normalizado para 0-1
        float timeRatio = avgTimeCostPerStep / 3f;       // Normalizado para 0-1

        // Encontra o maior gasto proporcional
        float maxRatio = Mathf.Max(healthRatio, sanityRatio, timeRatio);
        float minRatio = Mathf.Min(healthRatio, sanityRatio, timeRatio);
        float variance = maxRatio - minRatio;

        // Se os gastos são muito próximos (variância < 15%), é equilibrado
        if (variance < 0.15f)
        {
            riskProfile = "O Equilibrado";
        }
        // Se gastou mais vida proporcionalmente
        else if (healthRatio >= sanityRatio && healthRatio >= timeRatio)
        {
            if (healthRatio > 0.6f)
                riskProfile = "O Mártir";      // Gastou MUITA vida
            else
                riskProfile = "O Imprudente";  // Gastou mais vida que os outros
        }
        // Se gastou mais sanidade proporcionalmente
        else if (sanityRatio >= healthRatio && sanityRatio >= timeRatio)
        {
            if (sanityRatio > 0.6f)
                riskProfile = "O Louco";       // Gastou MUITA sanidade
            else
                riskProfile = "O Temerário";   // Gastou mais sanidade que os outros
        }
        // Se gastou mais tempo proporcionalmente (raro devido ao baixo range)
        else
        {
            riskProfile = "O Hesitante";       // Demorou muito nas decisões
        }
    }

    /// <summary>
    /// Gera todos os feedbacks de uma vez.
    /// </summary>
    public void GenerateAllFeedback()
    {
        GenerateOptimalityFeedback();
        GenerateExplorationProfile();
        GenerateBacktrackingAnalysis();
        GenerateRiskProfile();
    }

    /// <summary>
    /// Retorna um resumo formatado das métricas.
    /// </summary>
    public string GetFormattedSummary()
    {
        return $@"
=== RESUMO DA PARTIDA ===

RESULTADO: {(victory ? "VITÓRIA" : "DERROTA")}

EFICIÊNCIA DE ROTA:
- Otimalidade: {pathOptimalityRatio:P0} ({optimalityFeedback})
- Custo do Jogador: {playerTotalCost:F1}
- Custo Ótimo (A*): {optimalPathCost:F1}

EXPLORAÇÃO:
- Salas Visitadas: {roomsVisited}/{totalRooms} ({explorationIndex:P0})
- Perfil: {explorationProfile}

NAVEGAÇÃO:
- Total de Passos: {totalSteps}
- Salas Únicas: {uniqueRoomsVisited}
- Backtracking: {backtrackingCost} passos ({backtrackingAnalysis})

PERFIL DE RISCO:
- {riskProfile}
- Vida/Passo: {avgHealthCostPerStep:F2}
- Sanidade/Passo: {avgSanityCostPerStep:F2}
- Tempo/Passo: {avgTimeCostPerStep:F2}

TRAITS ADQUIRIDOS: {traitCount}
{string.Join("\n", traitsAcquired)}
";
    }
}
