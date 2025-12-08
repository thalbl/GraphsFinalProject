using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class que gerencia os recursos do jogador (Vida, Sanidade, Suprimentos)
/// e o sistema de traços (traits) que modificam custos de movimento.
/// </summary>
[System.Serializable]
public class PlayerStats
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Sanity")]
    public float maxSanity = 100f;
    public float currentSanity = 100f;

    [Header("Supplies")]
    public float maxSupplies = 10f;
    public float currentSupplies = 10f;

    [Header("Traits")]
    public List<Trait> activeTraits = new List<Trait>();

    [Header("Sanity Recovery on Affliction")]
    public float sanityRecoveryAmount = 20f; // Quanto de sanidade recupera ao ganhar affliction

    // Eventos para notificar UI
    public event Action<float, float, float> OnStatsChanged; // (health, sanity, supplies)
    public event Action<Trait> OnTraitGained;
    public event Action OnPlayerDied;

    /// <summary>
    /// Inicializa os stats com valores máximos.
    /// </summary>
    public void Initialize()
    {
        currentHealth = maxHealth;
        currentSanity = maxSanity;
        currentSupplies = maxSupplies;
        activeTraits.Clear();
        
        NotifyStatsChanged();
    }

    /// <summary>
    /// Aplica o custo de movimento baseado na EdgeData e nos traits ativos.
    /// Retorna o custo real aplicado (após modificadores de traits).
    /// </summary>
    /// <param name="baseCost">Custo base da aresta (sem modificadores)</param>
    /// <returns>EdgeData com os custos reais aplicados</returns>
    public EdgeData ApplyCost(EdgeData baseCost)
    {
        if (baseCost == null)
        {
            Debug.LogWarning("ApplyCost chamado com EdgeData null!");
            return new EdgeData(0, 0, 0);
        }

        // Calcula custos com modificadores de traits
        float finalHealthCost = baseCost.costHealth;
        float finalSanityCost = baseCost.costSanity;
        float finalTimeCost = baseCost.costTime;

        // Aplica modificadores de cada trait ativo
        foreach (Trait trait in activeTraits)
        {
            finalHealthCost *= trait.healthCostMultiplier;
            finalSanityCost *= trait.sanityCostMultiplier;
            finalTimeCost *= trait.timeCostMultiplier;

            Debug.Log($"Trait '{trait.traitName}' aplicado - Multiplicadores: H×{trait.healthCostMultiplier}, S×{trait.sanityCostMultiplier}, T×{trait.timeCostMultiplier}");
        }

        // Deduz os recursos (nunca ficam negativos)
        currentHealth = Mathf.Max(0, currentHealth - finalHealthCost);
        currentSanity = Mathf.Max(0, currentSanity - finalSanityCost);
        currentSupplies = Mathf.Max(0, currentSupplies - finalTimeCost); // Tempo consome suprimentos

        Debug.Log($"Custos aplicados - Vida: -{finalHealthCost:F1}, Sanidade: -{finalSanityCost:F1}, Suprimentos: -{finalTimeCost:F1}");
        Debug.Log($"Recursos atuais - Vida: {currentHealth:F1}/{maxHealth}, Sanidade: {currentSanity:F1}/{maxSanity}, Suprimentos: {currentSupplies:F1}/{maxSupplies}");

        // Verifica condições especiais
        CheckSanity();
        
        // Notifica mudanças
        NotifyStatsChanged();

        // Retorna os custos reais aplicados
        return new EdgeData(finalHealthCost, finalSanityCost, finalTimeCost, baseCost.description);
    }

    /// <summary>
    /// Verifica se a sanidade caiu abaixo de zero e aplica trait (50% affliction, 50% virtue).
    /// </summary>
    private void CheckSanity()
    {
        if (currentSanity <= 0)
        {
            Debug.LogWarning("Sanidade chegou a zero! A mente está em colapso...");
            EventLogger.LogSanity("Sua sanidade chegou a zero!");
            
            // 50/50 chance de affliction ou virtue
            bool isAffliction = UnityEngine.Random.value < 0.5f;
            
            if (isAffliction)
            {
                Debug.Log("A escuridão toma conta... Affliction ganhada!");
                GainRandomAffliction();
            }
            else
            {
                Debug.Log("A clareza surge do caos... Virtue ganhada!");
                GainRandomVirtue();
            }
            
            // Recupera sanidade completa após desenvolver o trait
            currentSanity = maxSanity;
            Debug.Log($"Sanidade restaurada para {currentSanity} (máximo)");
            EventLogger.LogGain($"A mente se adapta. Sanidade restaurada para {maxSanity:F0}");
        }
    }

    /// <summary>
    /// Adiciona um trait negativo aleatório ao jogador.
    /// </summary>
    public void GainRandomAffliction()
    {
        Trait affliction = Trait.GetRandomAffliction();
        
        // Evita duplicatas (opcional - pode ser permitido ter múltiplos do mesmo)
        if (!activeTraits.Exists(t => t.traitName == affliction.traitName))
        {
            activeTraits.Add(affliction);
            Debug.Log($"<color=red>AFFLICTION GANHADA: {affliction.traitName}</color> - {affliction.description}");
            
            // Log no UI
            EventLogger.LogAffliction(affliction.traitName, affliction.description);
            
            OnTraitGained?.Invoke(affliction);
        }
        else
        {
            Debug.Log($"Affliction '{affliction.traitName}' já ativa, não adicionada novamente.");
        }
    }

    /// <summary>
    /// Adiciona um trait positivo (virtude) aleatório ao jogador.
    /// </summary>
    public void GainRandomVirtue()
    {
        Trait virtue = Trait.GetRandomVirtue();
        
        // Evita duplicatas
        if (!activeTraits.Exists(t => t.traitName == virtue.traitName))
        {
            activeTraits.Add(virtue);
            Debug.Log($"<color=green>VIRTUE GANHADA: {virtue.traitName}</color> - {virtue.description}");
            
            // Log no UI
            EventLogger.LogVirtue(virtue.traitName, virtue.description);
            
            OnTraitGained?.Invoke(virtue);
        }
        else
        {
            Debug.Log($"Virtue '{virtue.traitName}' já ativa, não adicionada novamente.");
        }
    }

    /// <summary>
    /// Adiciona um trait específico (para eventos futuros, buffs, etc).
    /// </summary>
    public void GainTrait(Trait trait)
    {
        if (!activeTraits.Exists(t => t.traitName == trait.traitName))
        {
            activeTraits.Add(trait);
            Debug.Log($"Trait ganho: {trait.traitName} - {trait.description}");
            
            OnTraitGained?.Invoke(trait);
        }
    }

    /// <summary>
    /// Verifica se o jogador está morto (vida <= 0).
    /// </summary>
    public bool IsDead()
    {
        bool dead = currentHealth <= 0;
        
        if (dead && OnPlayerDied != null)
        {
            Debug.LogError("JOGADOR MORREU!");
            EventLogger.LogPlayerDeath();
            OnPlayerDied?.Invoke();
        }
        
        return dead;
    }

    /// <summary>
    /// Adiciona recursos (para tesouros, descanso, etc).
    /// </summary>
    public void AddResources(float health = 0, float sanity = 0, float supplies = 0)
    {
        currentHealth = Mathf.Min(currentHealth + health, maxHealth);
        currentSanity = Mathf.Min(currentSanity + sanity, maxSanity);
        currentSupplies = Mathf.Min(currentSupplies + supplies, maxSupplies);

        Debug.Log($"Recursos adicionados - Vida: +{health}, Sanidade: +{sanity}, Suprimentos: +{supplies}");
        
        NotifyStatsChanged();
    }

    /// <summary>
    /// Notifica listeners sobre mudanças nos stats.
    /// </summary>
    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke(currentHealth, currentSanity, currentSupplies);
    }
}
