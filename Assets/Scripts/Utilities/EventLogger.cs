using UnityEngine;

/// <summary>
/// Utility estática para logging conveniente de eventos de qualquer lugar do código.
/// Wrapper para GameLog.Instance.
/// </summary>
public static class EventLogger
{
    /// <summary>
    /// Log de mensagem neutra (cinza).
    /// </summary>
    public static void LogNeutral(string message)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMessage(message, LogMessageType.Neutral);
        else
            Debug.LogWarning($"[EventLogger] GameLog não disponível: {message}");
    }

    /// <summary>
    /// Log de informação importante (branco).
    /// </summary>
    public static void LogInfo(string message)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMessage(message, LogMessageType.Info);
        else
            Debug.LogWarning($"[EventLogger] GameLog não disponível: {message}");
    }

    /// <summary>
    /// Log de perigo/dano (vermelho).
    /// </summary>
    public static void LogDanger(string message)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMessage(message, LogMessageType.Danger);
        else
            Debug.LogWarning($"[EventLogger] GameLog não disponível: {message}");
    }

    /// <summary>
    /// Log de ganho/recuperação (verde).
    /// </summary>
    public static void LogGain(string message)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMessage(message, LogMessageType.Gain);
        else
            Debug.LogWarning($"[EventLogger] GameLog não disponível: {message}");
    }

    /// <summary>
    /// Log de sanidade/affliction (roxo).
    /// </summary>
    public static void LogSanity(string message)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMessage(message, LogMessageType.Sanity);
        else
            Debug.LogWarning($"[EventLogger] GameLog não disponível: {message}");
    }

    // ═══ MÉTODOS ESPECÍFICOS ═══

    /// <summary>
    /// Log de movimento entre salas.
    /// </summary>
    public static void LogMovement(string fromRoom, string toRoom, float cost)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMovement(fromRoom, toRoom, cost);
    }

    /// <summary>
    /// Log de dano recebido.
    /// </summary>
    public static void LogDamage(float amount, string source = "desconhecida")
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogDamage(amount, source);
    }

    /// <summary>
    /// Log de affliction ganhada.
    /// </summary>
    public static void LogAffliction(string afflictionName, string description)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogAffliction(afflictionName, description);
    }

    /// <summary>
    /// Log de virtue (trait positivo) ganhada.
    /// </summary>
    public static void LogVirtue(string virtueName, string description)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogMessage($"VIRTUE: {virtueName} - {description}", LogMessageType.Gain);
        else
            Debug.Log($"[EventLogger] GameLog não disponível: VIRTUE - {virtueName}");
    }

    /// <summary>
    /// Log de seleção de sala.
    /// </summary>
    public static void LogRoomSelection(string roomName)
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogRoomSelection(roomName);
    }

    /// <summary>
    /// Log de morte do jogador.
    /// </summary>
    public static void LogPlayerDeath()
    {
        if (GameLog.Instance != null)
            GameLog.Instance.LogPlayerDeath();
    }
}
