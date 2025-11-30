using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Sistema que gerencia a narrativa emergente através de flavor texts contextuais.
/// Transforma eventos mecânicos em narrativa imersiva.
/// </summary>
public class NarrativeLogSystem : MonoBehaviour
{
    [Header("Flavor Text Database")]
    [SerializeField] private List<FlavorTextData> flavorTextDatabase = new List<FlavorTextData>();

    [Header("Settings")]
    [SerializeField] private bool enableNarrativeMode = true;
    [SerializeField] private float traitPhraseChance = 0.7f; // 70% de chance de usar frase de trait

    // Singleton
    private static NarrativeLogSystem instance;
    public static NarrativeLogSystem Instance => instance;

    // Cache do player stats
    private PlayerStats cachedPlayerStats;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        // Busca player stats
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            cachedPlayerStats = player.stats;
        }
    }

    /// <summary>
    /// Log quando o jogador seleciona/inspeciona uma sala.
    /// </summary>
    public void LogRoomSelection(RoomNode room)
    {
        if (!enableNarrativeMode || room == null)
        {
            EventLogger.LogRoomSelection($"{room.roomType} ({room.logicalPosition})");
            return;
        }

        var phrase = GetContextualPhrase(FlavorContext.RoomSelection, room.roomType);
        
        if (!string.IsNullOrEmpty(phrase.text))
        {
            GameLog.Instance?.LogMessage(phrase.text, phrase.type);
        }
        else
        {
            // Fallback para mensagem padrão
            EventLogger.LogRoomSelection($"{room.roomType} ({room.logicalPosition})");
        }
    }

    /// <summary>
    /// Log durante movimento entre salas.
    /// </summary>
    public void LogMovement(RoomNode fromRoom, RoomNode toRoom, float cost)
    {
        if (!enableNarrativeMode)
        {
            EventLogger.LogMovement(fromRoom.logicalPosition.ToString(), toRoom.logicalPosition.ToString(), cost);
            return;
        }

        var phrase = GetContextualPhrase(FlavorContext.Movement, toRoom.roomType);
        
        if (!string.IsNullOrEmpty(phrase.text))
        {
            GameLog.Instance?.LogMessage(phrase.text, phrase.type);
        }
        else
        {
            // Fallback
            EventLogger.LogMovement(fromRoom.logicalPosition.ToString(), toRoom.logicalPosition.ToString(), cost);
        }
    }

    /// <summary>
    /// Log ao entrar em uma sala.
    /// </summary>
    public void LogRoomEnter(RoomNode room, string description = "")
    {
        if (!enableNarrativeMode || room == null)
        {
            EventLogger.LogInfo($"Entrando em {room.roomType}: {room.logicalPosition}");
            return;
        }

        var phrase = GetContextualPhrase(FlavorContext.RoomEnter, room.roomType);
        
        if (!string.IsNullOrEmpty(phrase.text))
        {
            GameLog.Instance?.LogMessage(phrase.text, phrase.type);
        }
        else if (!string.IsNullOrEmpty(description))
        {
            EventLogger.LogInfo($"> {description}");
        }
        else
        {
            EventLogger.LogInfo($"Chegou em {room.roomType}");
        }
    }

    /// <summary>
    /// Log ao receber dano.
    /// </summary>
    public void LogDamage(float amount, string source)
    {
        if (!enableNarrativeMode)
        {
            EventLogger.LogDamage(amount, source);
            return;
        }

        var phrase = GetContextualPhrase(FlavorContext.Damage, null);
        
        if (!string.IsNullOrEmpty(phrase.text))
        {
            // Substitui {amount} e {source} na frase
            string finalPhrase = phrase.text
                .Replace("{amount}", amount.ToString("F0"))
                .Replace("{source}", source);
            
            GameLog.Instance?.LogMessage(finalPhrase, phrase.type);
        }
        else
        {
            EventLogger.LogDamage(amount, source);
        }
    }

    /// <summary>
    /// Busca uma frase contextual aplicável.
    /// </summary>
    private (string text, LogMessageType type) GetContextualPhrase(FlavorContext context, RoomType? roomType)
    {
        if (flavorTextDatabase == null || flavorTextDatabase.Count == 0)
            return ("", LogMessageType.Neutral);

        FlavorCondition currentCondition = GetPlayerCondition();
        List<Trait> playerTraits = cachedPlayerStats?.activeTraits ?? new List<Trait>();

        // Filtra frases aplicáveis
        var applicablePhrases = flavorTextDatabase
            .Where(data => data.IsApplicable(context, roomType, currentCondition, playerTraits))
            .ToList();

        if (applicablePhrases.Count == 0)
            return ("", LogMessageType.Neutral);

        // Prioridade 1: Frases de Trait (mais raras e interessantes)
        var traitPhrases = applicablePhrases.Where(p => !string.IsNullOrEmpty(p.requiredTraitName)).ToList();
        if (traitPhrases.Count > 0 && Random.value < traitPhraseChance)
        {
            var selected = traitPhrases[Random.Range(0, traitPhrases.Count)];
            return (selected.GetRandomPhrase(), GetMessageType(selected, context));
        }

        // Prioridade 2: Frases de Condição Crítica
        var conditionPhrases = applicablePhrases.Where(p => p.condition != FlavorCondition.None).ToList();
        if (conditionPhrases.Count > 0)
        {
            var selected = conditionPhrases[Random.Range(0, conditionPhrases.Count)];
            return (selected.GetRandomPhrase(), GetMessageType(selected, context));
        }

        // Prioridade 3: Frases Padrão
        var selectedDefault = applicablePhrases[Random.Range(0, applicablePhrases.Count)];
        return (selectedDefault.GetRandomPhrase(), GetMessageType(selectedDefault, context));
    }

    /// <summary>
    /// Determina a condição atual do jogador.
    /// </summary>
    private FlavorCondition GetPlayerCondition()
    {
        if (cachedPlayerStats == null)
            return FlavorCondition.None;

        float healthPercent = cachedPlayerStats.currentHealth / cachedPlayerStats.maxHealth;
        float sanityPercent = cachedPlayerStats.currentSanity / cachedPlayerStats.maxSanity;

        // Prioriza condições críticas
        if (healthPercent < 0.3f) return FlavorCondition.LowHealth;
        if (sanityPercent < 0.3f) return FlavorCondition.LowSanity;
        if (healthPercent > 0.8f) return FlavorCondition.HighHealth;
        if (sanityPercent > 0.8f) return FlavorCondition.HighSanity;

        return FlavorCondition.None;
    }

    /// <summary>
    /// Determina o tipo de mensagem baseado no FlavorTextData e contexto.
    /// </summary>
    private LogMessageType GetMessageType(FlavorTextData data, FlavorContext context)
    {
        if (data.overrideMessageType)
            return data.messageTypeOverride;

        // Tipo padrão baseado no contexto
        return context switch
        {
            FlavorContext.RoomSelection => LogMessageType.Info,
            FlavorContext.Movement => LogMessageType.Neutral,
            FlavorContext.RoomEnter => LogMessageType.Info,
            FlavorContext.Damage => LogMessageType.Danger,
            FlavorContext.Healing => LogMessageType.Gain,
            FlavorContext.Affliction => LogMessageType.Sanity,
            FlavorContext.Death => LogMessageType.Danger,
            _ => LogMessageType.Neutral
        };
    }

    /// <summary>
    /// Adiciona flavor texts programaticamente (para testes).
    /// </summary>
    public void AddFlavorText(FlavorTextData data)
    {
        if (!flavorTextDatabase.Contains(data))
        {
            flavorTextDatabase.Add(data);
        }
    }
}
