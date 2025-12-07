using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Tipos de mensagens do log com cores diferentes.
/// </summary>
public enum LogMessageType
{
    Neutral,    // Cinza - Movimento geral, exploração
    Info,       // Branco - Eventos importantes, seleção
    Danger,     // Vermelho - Dano, armadilhas, morte
    Gain,       // Verde - Recuperação de recursos
    Sanity      // Roxo - Sanidade, afflictions
}

/// <summary>
/// Gerenciador central do log de eventos com design Persona 5-inspired.
/// Singleton para acesso fácil de qualquer lugar.
/// </summary>
public class GameLog : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform logContent; // Content do ScrollRect
    [SerializeField] private GameObject logEntryPrefab; // Prefab com LogEntry.cs
    [SerializeField] private ScrollRect scrollRect; // Para auto-scroll

    [Header("Design Settings (Optional)")]
    [SerializeField] private Image logPanelBackground; // Fundo do painel de log
    [SerializeField] private Color panelBackgroundColor = new Color(0.04f, 0.04f, 0.04f, 0.9f);

    [Header("Settings")]
    [SerializeField] private int maxLogEntries = 50; // Máximo de mensagens antes de limpar antigas
    [SerializeField] private bool autoScroll = true; // Auto-scroll para baixo
    [SerializeField] private float messageLifetime = 60f; // Tempo antes de limpar mensagens antigas (0 = desabilitado)

    // Singleton
    private static GameLog instance;
    public static GameLog Instance => instance;

    // Lista de entradas ativas
    private Queue<GameObject> logEntries = new Queue<GameObject>();
    private Queue<float> entryTimestamps = new Queue<float>();

    void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Aplicar cor de fundo se configurado
        if (logPanelBackground != null)
        {
            logPanelBackground.color = panelBackgroundColor;
        }
    }

    void Start()
    {
        // Log system ready
    }

    void Update()
    {
        // Limpar mensagens antigas baseado no tempo (se habilitado)
        if (messageLifetime > 0)
        {
            CleanupOldEntries();
        }
    }

    /// <summary>
    /// Adiciona uma mensagem ao log.
    /// </summary>
    public void LogMessage(string message, LogMessageType type = LogMessageType.Neutral)
    {
        if (logEntryPrefab == null || logContent == null)
        {
            Debug.LogWarning($"[GameLog] Prefab ou Content não atribuído! Mensagem: {message}");
            return;
        }

        // Instancia o prefab
        GameObject entryObj = Instantiate(logEntryPrefab, logContent);
        LogEntry entry = entryObj.GetComponent<LogEntry>();

        if (entry != null)
        {
            entry.SetMessage(message, type);
        }
        else
        {
            Debug.LogError("[GameLog] LogEntry component não encontrado no prefab!");
        }

        // Adiciona à fila
        logEntries.Enqueue(entryObj);
        entryTimestamps.Enqueue(Time.time);

        // Remove entradas antigas se exceder o limite
        while (logEntries.Count > maxLogEntries)
        {
            RemoveOldestEntry();
        }

        // Auto-scroll para o final
        if (autoScroll && scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f; // 0 = bottom
        }
    }

    /// <summary>
    /// Remove a entrada mais antiga do log.
    /// </summary>
    private void RemoveOldestEntry()
    {
        if (logEntries.Count > 0)
        {
            GameObject oldEntry = logEntries.Dequeue();
            entryTimestamps.Dequeue();
            Destroy(oldEntry);
        }
    }

    /// <summary>
    /// Limpa mensagens antigas baseado no tempo.
    /// </summary>
    private void CleanupOldEntries()
    {
        while (entryTimestamps.Count > 0 && Time.time - entryTimestamps.Peek() > messageLifetime)
        {
            RemoveOldestEntry();
        }
    }

    // ═══ MÉTODOS DE CONVENIÊNCIA ═══

    /// <summary>
    /// Log de movimento entre salas.
    /// </summary>
    public void LogMovement(string fromRoom, string toRoom, float costApplied)
    {
        LogMessage($"> Movendo de {fromRoom} para {toRoom}... (Custo: {costApplied:F1})", LogMessageType.Neutral);
    }

    /// <summary>
    /// Log de dano recebido.
    /// </summary>
    public void LogDamage(float amount, string source = "desconhecida")
    {
        LogMessage($"> DANO! -{amount:F0} de {source}.", LogMessageType.Danger);
    }

    /// <summary>
    /// Log de affliction ganhada.
    /// </summary>
    public void LogAffliction(string afflictionName, string description)
    {
        LogMessage($"> AFFLICTION: {afflictionName} - {description}", LogMessageType.Sanity);
    }

    /// <summary>
    /// Log de recuperação de recursos.
    /// </summary>
    public void LogGain(string resourceName, float amount)
    {
        LogMessage($"> +{amount:F0} {resourceName} recuperado.", LogMessageType.Gain);
    }

    /// <summary>
    /// Log de perda de sanidade.
    /// </summary>
    public void LogSanityLoss(float amount, string reason = "")
    {
        string msg = $"> Sanidade -{amount:F0}";
        if (!string.IsNullOrEmpty(reason))
            msg += $" ({reason})";
        
        LogMessage(msg, LogMessageType.Sanity);
    }

    /// <summary>
    /// Log de seleção de sala.
    /// </summary>
    public void LogRoomSelection(string roomName)
    {
        LogMessage($"> Você selecionou: {roomName}", LogMessageType.Info);
    }

    /// <summary>
    /// Log de entrada em sala.
    /// </summary>
    public void LogRoomEnter(string roomName, string description = "")
    {
        string msg = $"> Entrando em {roomName}";
        if (!string.IsNullOrEmpty(description))
            msg += $" - {description}";
        
        LogMessage(msg, LogMessageType.Info);
    }

    /// <summary>
    /// Log de morte do jogador.
    /// </summary>
    public void LogPlayerDeath()
    {
        LogMessage("> ═══ VOCÊ MORREU ═══", LogMessageType.Danger);
    }

    /// <summary>
    /// Limpa todos os logs.
    /// </summary>
    public void ClearLog()
    {
        while (logEntries.Count > 0)
        {
            RemoveOldestEntry();
        }
    }

    // ═══ HELPERS ESTÁTICOS PARA CORES ═══

    /// <summary>
    /// Retorna a cor correspondente ao tipo de mensagem.
    /// </summary>
    public static Color GetColorForType(LogMessageType type)
    {
        return type switch
        {
            LogMessageType.Neutral => new Color(0.8f, 0.8f, 0.8f),    // #CCCCCC (cinza)
            LogMessageType.Info => Color.white,                       // #FFFFFF (branco)
            LogMessageType.Danger => new Color(1f, 0.3f, 0.3f),      // #FF4D4D (vermelho)
            LogMessageType.Gain => new Color(0.3f, 1f, 0.53f),       // #4DFF88 (verde)
            LogMessageType.Sanity => new Color(0.74f, 0.58f, 0.98f), // #BD93F9 (roxo)
            _ => Color.white
        };
    }

    /// <summary>
    /// Retorna a cor da borda correspondente ao tipo de mensagem.
    /// </summary>
    public static Color GetBorderColorForType(LogMessageType type)
    {
        return type switch
        {
            LogMessageType.Neutral => new Color(0.33f, 0.33f, 0.33f), // #555555
            LogMessageType.Info => Color.white,                       // #FFFFFF
            LogMessageType.Danger => new Color(0.83f, 0.1f, 0.13f),  // #D31920
            LogMessageType.Gain => new Color(0.3f, 1f, 0.53f),       // #4DFF88
            LogMessageType.Sanity => new Color(0.74f, 0.58f, 0.98f), // #BD93F9
            _ => Color.gray
        };
    }
}
