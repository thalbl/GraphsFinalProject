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
                "Ferido",
                "Um ferimento grave dificulta sua jornada. Custos de Vida aumentam 50%.",
                healthMult: 1.5f,
                sanityMult: 1.0f,
                timeMult: 1.0f,
                affliction: true
            ),
            new Trait(
                "Exausto",
                "A fadiga toma conta. Movimentos custam o dobro de tempo.",
                healthMult: 1.0f,
                sanityMult: 1.0f,
                timeMult: 2.0f,
                affliction: true
            ),
            new Trait(
                "Agorafóbico",
                "Espaços abertos causam pânico. Sanidade cai 80% mais rápido.",
                healthMult: 1.0f,
                sanityMult: 1.8f,
                timeMult: 1.2f,
                affliction: true
            ),
            new Trait(
                "Claustrofóbico",
                "A sensação de estar preso é insuportável. Todos os custos aumentam 30%.",
                healthMult: 1.3f,
                sanityMult: 1.3f,
                timeMult: 1.3f,
                affliction: true
            )
        };

        int randomIndex = Random.Range(0, afflictions.Length);
        return afflictions[randomIndex];
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

