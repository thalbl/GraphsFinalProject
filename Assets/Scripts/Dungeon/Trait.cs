using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de Traits (Traços) do jogador que modificam custos de movimento.
/// Traits podem ser positivos (buffs) ou negativos (afflictions).
/// </summary>
[System.Serializable]
public class Trait
{
    public string traitName;
    public string description;
    
    // Modificadores de custo (1.0 = sem modificação, 2.0 = dobra o custo, 0.5 = metade do custo)
    public float healthCostMultiplier = 1.0f;
    public float sanityCostMultiplier = 1.0f;
    public float timeCostMultiplier = 1.0f;
    
    // Flag para identificar se é uma affliction (trait negativo)
    public bool isAffliction = false;

    // Construtores
    public Trait() { }

    public Trait(string name, string desc, float healthMult = 1.0f, float sanityMult = 1.0f, float timeMult = 1.0f, bool affliction = false)
    {
        traitName = name;
        description = desc;
        healthCostMultiplier = healthMult;
        sanityCostMultiplier = sanityMult;
        timeCostMultiplier = timeMult;
        isAffliction = affliction;
    }

    /// <summary>
    /// Retorna uma affliction (trait negativo) aleatória de uma lista pré-definida.
    /// </summary>
    public static Trait GetRandomAffliction()
    {
        Trait[] afflictions = new Trait[]
        {
            new Trait(
                "Paranóico",
                "O medo constante corrói sua mente. Custos de Sanidade são dobrados.",
                healthMult: 1.0f,
                sanityMult: 2.0f,
                timeMult: 1.0f,
                affliction: true
            ),
            new Trait(
                "Imprudente",
                "Impaciência e desprezo pela própria segurança. Vida +50%, Tempo -20%.",
                healthMult: 1.5f,
                sanityMult: 1.0f,
                timeMult: 0.8f,
                affliction: true
            ),
            new Trait(
                "Hesitante",
                "Medo paralisante de tomar decisões erradas. Custos de Tempo dobrados.",
                healthMult: 1.0f,
                sanityMult: 1.0f,
                timeMult: 2.0f,
                affliction: true
            ),
            new Trait(
                "Frágil",
                "Corpo e mente fragilizados. Todos os custos aumentam 30%.",
                healthMult: 1.3f,
                sanityMult: 1.3f,
                timeMult: 1.3f,
                affliction: true
            ),
            new Trait(
                "Ganancioso",
                "Obsessão por saque. Sanidade +30%, Tempo -10%.",
                healthMult: 1.0f,
                sanityMult: 1.3f,
                timeMult: 0.9f,
                affliction: true
            ),
            new Trait(
                "Claustrofóbico",
                "Pânico em corredores apertados. Sanidade +50% durante movimento.",
                healthMult: 1.0f,
                sanityMult: 1.5f,
                timeMult: 1.2f,
                affliction: true
            )
        };

        int randomIndex = Random.Range(0, afflictions.Length);
        return afflictions[randomIndex];
    }

    /// <summary>
    /// Retorna um trait positivo (virtude) aleatório.
    /// </summary>
    public static Trait GetRandomVirtue()
    {
        Trait[] virtues = new Trait[]
        {
            new Trait(
                "Estoico",
                "Mente forte e resiliente. Custos de Sanidade reduzidos em 30%.",
                healthMult: 1.0f,
                sanityMult: 0.7f,
                timeMult: 1.0f,
                affliction: false
            ),
            new Trait(
                "Vigoroso",
                "Corpo resistente. Custos de Vida reduzidos em 30%.",
                healthMult: 0.7f,
                sanityMult: 1.0f,
                timeMult: 1.0f,
                affliction: false
            ),
            new Trait(
                "Ligeiro",
                "Veloz e ágil. Custos de Tempo reduzidos em 30%.",
                healthMult: 1.0f,
                sanityMult: 1.0f,
                timeMult: 0.7f,
                affliction: false
            ),
            new Trait(
                "Estrategista",
                "Mestre tático. Todos os custos reduzidos em 15%.",
                healthMult: 0.85f,
                sanityMult: 0.85f,
                timeMult: 0.85f,
                affliction: false
            )
        };

        int randomIndex = Random.Range(0, virtues.Length);
        return virtues[randomIndex];
    }

    /// <summary>
    /// Cria um trait positivo (buff) customizado.
    /// Exemplos futuros: "Resiliente", "Rápido", "Calmo"
    /// </summary>
    public static Trait CreatePositiveTrait(string name, string desc, float healthMult = 0.8f, float sanityMult = 0.8f, float timeMult = 0.8f)
    {
        return new Trait(name, desc, healthMult, sanityMult, timeMult, affliction: false);
    }
}

