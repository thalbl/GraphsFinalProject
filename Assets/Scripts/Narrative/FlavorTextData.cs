using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contexto onde o flavor text é usado.
/// </summary>
public enum FlavorContext
{
    RoomSelection,    // Ao selecionar/inspecionar sala
    Movement,         // Durante travessia
    RoomEnter,        // Ao entrar na sala
    Damage,           // Ao receber dano
    Healing,          // Ao recuperar recursos
    Affliction,       // Ao ganhar affliction
    Death             // Ao morrer
}

/// <summary>
/// Condição do jogador que influencia o texto.
/// </summary>
public enum FlavorCondition
{
    None,           // Sem condição específica
    LowHealth,      // Vida < 30%
    LowSanity,      // Sanidade < 30%
    HighHealth,     // Vida > 80%
    HighSanity      // Sanidade > 80%
}

/// <summary>
/// ScriptableObject que armazena frases narrativas contextuais.
/// </summary>
[CreateAssetMenu(fileName = "FlavorTextData", menuName = "Dungeon/Flavor Text Data")]
public class FlavorTextData : ScriptableObject
{
    [Header("Context")]
    public FlavorContext context;
    public RoomType roomType = RoomType.Spawn; // Tipo de sala (opcional)
    public bool anyRoomType = true; // Se true, ignora roomType

    [Header("Player Condition")]
    public FlavorCondition condition = FlavorCondition.None;
    
    [Header("Trait Requirement (Optional)")]
    public string requiredTraitName = ""; // Se vazio, não requer trait

    [Header("Phrases")]
    [TextArea(2, 4)]
    public List<string> phrases = new List<string>();

    [Header("Message Type Override")]
    public bool overrideMessageType = false;
    public LogMessageType messageTypeOverride = LogMessageType.Neutral;

    /// <summary>
    /// Retorna uma frase aleatória desta lista.
    /// </summary>
    public string GetRandomPhrase()
    {
        if (phrases == null || phrases.Count == 0)
            return "";

        return phrases[Random.Range(0, phrases.Count)];
    }

    /// <summary>
    /// Verifica se este FlavorText é aplicável dado o contexto atual.
    /// </summary>
    public bool IsApplicable(FlavorContext checkContext, RoomType? checkRoomType, 
                            FlavorCondition checkCondition, List<Trait> playerTraits)
    {
        // Verifica contexto
        if (context != checkContext)
            return false;

        // Verifica tipo de sala (se relevante)
        if (!anyRoomType && checkRoomType.HasValue && roomType != checkRoomType.Value)
            return false;

        // Verifica condição do player
        if (condition != FlavorCondition.None && condition != checkCondition)
            return false;

        // Verifica trait necessário
        if (!string.IsNullOrEmpty(requiredTraitName))
        {
            if (playerTraits == null || !playerTraits.Any(t => t.traitName == requiredTraitName))
                return false;
        }

        return true;
    }
}
