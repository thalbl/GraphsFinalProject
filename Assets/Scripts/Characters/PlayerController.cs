using System.Collections;
using UnityEngine;

/// <summary>
/// Controla o estado e movimento do jogador através das salas da dungeon.
/// Gerencia o ciclo: Idle -> Decisão -> Movimento -> Evento -> Idle
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Referências do Sistema")]
    [SerializeField] private DungeonGraph dungeonGraph;
    [SerializeField] private GameController gameController;

    [Header("Animator")]
    [Tooltip("Animator para controlar animações Idle e Run")]
    [SerializeField] private Animator animator;
    [Tooltip("Nome do parâmetro bool no Animator (true = correndo, false = idle)")]
    [SerializeField] private string runParameterName = "IsRunning";

    [Header("Player Stats")]
    public PlayerStats stats = new PlayerStats();

    [Header("Movement Settings")]
    [SerializeField] private float movementDuration = 0.5f; // Tempo de animação de movimento

    [Header("Visual Offset")]
    [Tooltip("Ajuste de posição para centralizar o sprite com as salas")]
    [SerializeField] private Vector2 visualOffset = Vector2.zero;

    [Header("Estado Atual")]
    public RoomNode currentRoom;
    private bool isMoving = false;
    private bool isDead = false;

    // Componentes
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Auto-detecta Animator se não setado
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Auto-detecta referências se não setadas
        if (dungeonGraph == null)
            dungeonGraph = FindObjectOfType<DungeonGraph>();
        
        if (gameController == null)
            gameController = FindObjectOfType<GameController>();
    }

    /// <summary>
    /// Atualiza o estado da animação no Animator.
    /// </summary>
    private void SetRunningAnimation(bool running)
    {
        if (animator != null && !string.IsNullOrEmpty(runParameterName))
        {
            animator.SetBool(runParameterName, running);
        }
    }

    /// <summary>
    /// Retorna a posição de uma sala com o offset visual aplicado.
    /// </summary>
    private Vector3 GetPositionWithOffset(RoomNode room)
    {
        Vector3 position = room.GetWorldPosition();
        position.x += visualOffset.x;
        position.y += visualOffset.y;
        return position;
    }

    /// <summary>
    /// Inicializa o jogador na sala de spawn.
    /// Chamado pelo GameController após a geração da dungeon.
    /// </summary>
    public void Initialize(RoomNode startNode)
    {
        if (startNode == null)
        {
            Debug.LogError("PlayerController.Initialize chamado com RoomNode null!");
            return;
        }

        currentRoom = startNode;
        
        // Posiciona o player fisicamente na sala (com offset visual)
        transform.position = GetPositionWithOffset(startNode);
        
        // Inicializa stats
        stats.Initialize();
        
        // Registra evento de morte
        stats.OnPlayerDied += OnPlayerDied;
        
        isDead = false;
        isMoving = false;

        Debug.Log($"Player inicializado na sala {startNode.logicalPosition} (Tipo: {startNode.roomType})");
        Debug.Log($"Posição física: {transform.position}");
    }

    /// <summary>
    /// Inicia o movimento para uma sala alvo.
    /// Este é o método principal que orquestra todo o fluxo de ação.
    /// </summary>
    public void MoveTo(RoomNode targetRoom, CostType costType = CostType.Health)
    {
        // Validações
        if (isDead)
        {
            Debug.LogWarning("Player morto não pode se mover!");
            return;
        }

        if (isMoving)
        {
            Debug.LogWarning("Player já está em movimento!");
            return;
        }

        if (targetRoom == null)
        {
            Debug.LogError("MoveTo chamado com targetRoom null!");
            return;
        }

        if (targetRoom == currentRoom)
        {
            Debug.LogWarning("Player já está nesta sala!");
            return;
        }

        // Verifica se a sala alvo está conectada
        if (!currentRoom.connections.Contains(targetRoom))
        {
            Debug.LogWarning($"Sala {targetRoom.logicalPosition} não está conectada à sala atual!");
            return;
        }

        // Inicia corrotina de movimento
        StartCoroutine(MovementSequence(targetRoom, costType));
    }

    /// <summary>
    /// Sequência completa de movimento:
    /// 1. Aplica custos
    /// 2. Anima movimento
    /// 3. Atualiza sala atual
    /// 4. Dispara eventos da sala
    /// </summary>
    private IEnumerator MovementSequence(RoomNode targetRoom, CostType costType)
    {
        isMoving = true;

        Debug.Log($"─── INICIANDO MOVIMENTO ───");
        Debug.Log($"De: {currentRoom.logicalPosition} ({currentRoom.roomType})");
        Debug.Log($"Para: {targetRoom.logicalPosition} ({targetRoom.roomType})");

        // ═══ FASE 1: CÁLCULO E APLICAÇÃO DE CUSTOS ═══
        EdgeData baseCost = dungeonGraph.GetEdgeData(currentRoom, targetRoom);
        
        if (baseCost == null)
        {
            Debug.LogError($"Não foi possível obter EdgeData entre {currentRoom.logicalPosition} e {targetRoom.logicalPosition}!");
            isMoving = false;
            yield break;
        }

        Debug.Log($"Custo base - Vida: {baseCost.costHealth}, Sanidade: {baseCost.costSanity}, Tempo: {baseCost.costTime}");

        // Aplica custos (com modificadores de traits)
        EdgeData actualCost = stats.ApplyCost(baseCost);
        
        // Log de movimento no UI
        float totalCost = actualCost.costHealth + actualCost.costSanity + actualCost.costTime;
        EventLogger.LogMovement(currentRoom.logicalPosition.ToString(), targetRoom.logicalPosition.ToString(), totalCost);

        // ═══ FASE 2: CHECK DE MORTE ═══
        if (stats.IsDead())
        {
            Debug.LogError("<color=red>PLAYER MORREU DURANTE O MOVIMENTO!</color>");
            isMoving = false;
            isDead = true;
            yield break;
        }

        // ═══ FASE 3: ANIMAÇÃO DE MOVIMENTO ═══
        SetRunningAnimation(true);  // Inicia animação de corrida
        
        Vector3 startPosition = transform.position;
        Vector3 endPosition = GetPositionWithOffset(targetRoom);
        float elapsed = 0f;

        Debug.Log($"Animando movimento de {startPosition} para {endPosition}...");

        while (elapsed < movementDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / movementDuration;
            
            // Lerp suave com ease-in-out
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
            
            yield return null;
        }

        // Garante posição final exata
        transform.position = endPosition;
        
        SetRunningAnimation(false);  // Volta para animação de Idle

        // ═══ FASE 4: ATUALIZAÇÃO DE ESTADO ═══
        RoomNode previousRoom = currentRoom;
        currentRoom = targetRoom;

        Debug.Log($"<color=cyan>Chegou na sala {currentRoom.logicalPosition}!</color>");
        EventLogger.LogInfo($"Chegou em {currentRoom.roomType}: {currentRoom.logicalPosition}");

        // Notifica GameController sobre a mudança de sala
        if (gameController != null)
        {
            gameController.MovePlayerToRoom(currentRoom);
            
            // Registra movimento para progresso/métricas
            gameController.RecordMovement(previousRoom, currentRoom, actualCost);
        }

        // ═══ FASE 5: EVENTOS DA SALA ═══
        TriggerRoomEvent(currentRoom);

        // Retorna ao estado Idle
        isMoving = false;
        Debug.Log($"─── MOVIMENTO COMPLETO ───\n");
    }

    /// <summary>
    /// Dispara a lógica de evento baseada no tipo de sala.
    /// </summary>
    private void TriggerRoomEvent(RoomNode room)
    {
        // Evita triggerar evento em salas já visitadas (opcional - pode ser alterado)
        // Por enquanto, eventos disparam sempre

        switch (room.roomType)
        {
            case RoomType.Spawn:
                Debug.Log("Sala de Spawn - Nenhum evento.");
                break;

            case RoomType.Combat:
                OnCombatRoom(room);
                break;

            case RoomType.Treasure:
                OnTreasureRoom(room);
                break;

            case RoomType.Camp:
                OnCampRoom(room);
                break;

            case RoomType.Boss:
                OnBossRoom(room);
                break;

            case RoomType.Event:
                OnEventRoom(room);
                break;
        }
    }

    /// <summary>
    /// Evento: Sala de Combate
    /// TODO: Integrar com sistema de combate futuro
    /// </summary>
    private void OnCombatRoom(RoomNode room)
    {
        Debug.Log($"<color=red>[COMBATE] COMBATE INICIADO NA SALA {room.logicalPosition}!</color>");
        // TODO: Parar movimento, iniciar combate
        // Por enquanto, apenas log
    }

    /// <summary>
    /// Evento: Sala de Tesouro
    /// Adiciona recursos ao jogador
    /// </summary>
    private void OnTreasureRoom(RoomNode room)
    {
        Debug.Log($"<color=yellow>[TESOURO] TESOURO ENCONTRADO NA SALA {room.logicalPosition}!</color>");
        EventLogger.LogInfo($"Bau encontrado na sala {room.logicalPosition}!");
        
        // Recompensas aleatórias (reduzidas para balanceamento)
        float healthGain = Random.Range(2f, 5f);
        float sanityGain = Random.Range(2f, 5f);
        float suppliesGain = Random.Range(1f, 3f);

        stats.AddResources(healthGain, sanityGain, suppliesGain);
        
        Debug.Log($"Ganhou: +{healthGain:F0} Vida, +{sanityGain:F0} Sanidade, +{suppliesGain:F0} Suprimentos");
        EventLogger.LogGain($"+{healthGain:F0} Vida, +{sanityGain:F0} Sanidade, +{suppliesGain:F0} Suprimentos");
    }

    /// <summary>
    /// Evento: Sala de Acampamento - Local de descanso
    /// Restaura vida e sanidade do jogador
    /// </summary>
    private void OnCampRoom(RoomNode room)
    {
        Debug.Log($"<color=cyan>[CAMP] ACAMPAMENTO NA SALA {room.logicalPosition}!</color>");
        Debug.Log("Descansando e recuperando forcas...");
        
        // Valores de recuperação (balanceados)
        float healthRestore = 20f;
        float sanityRestore = 20f;
        
        // Recupera vida e sanidade usando AddResources
        stats.AddResources(healthRestore, sanityRestore, 0f);
        
        Debug.Log($"<color=green>+{healthRestore} HP restaurado! HP atual: {stats.currentHealth:F1}/{stats.maxHealth}</color>");
        Debug.Log($"<color=cyan>+{sanityRestore} Sanidade restaurada! Sanidade atual: {stats.currentSanity:F1}/{stats.maxSanity}</color>");
        
        EventLogger.LogGain($"Descansou no acampamento. +{healthRestore:F0} HP, +{sanityRestore:F0} Sanidade");
    }

    /// <summary>
    /// Evento: Sala do Boss
    /// Termina o jogo com vitória após um delay
    /// </summary>
    private void OnBossRoom(RoomNode room)
    {
        Debug.Log($"<color=red>[BOSS] BOSS ENCONTRADO NA SALA {room.logicalPosition}!</color>");
        EventLogger.LogInfo("Voce chegou na sala do Boss!");
        
        // Mensagens dramáticas
        EventLogger.LogGain("A dungeon está completa!");
        EventLogger.LogInfo("Você sobreviveu à escuridão...");
        
        Debug.Log("════════════════════════════════════");
        Debug.Log("       === VITORIA! ===              ");
        Debug.Log("  Você completou a dungeon!         ");
        Debug.Log("════════════════════════════════════");
        
        // Inicia coroutine para delay antes de mostrar tela de vitória
        StartCoroutine(VictorySequence());
    }

    /// <summary>
    /// Coroutine que espera um tempo antes de exibir a tela de vitória.
    /// Permite que o jogador leia as mensagens de entrada na sala do Boss.
    /// </summary>
    private IEnumerator VictorySequence()
    {
        // Espera 3 segundos para o jogador ler as mensagens
        yield return new WaitForSeconds(3f);
        
        EventLogger.LogGain("=== VITORIA ===");
        
        // Termina o jogo com vitória
        if (gameController != null)
        {
            gameController.OnGameEnd(true);
        }
        else
        {
            Debug.LogError("GameController não encontrado para finalizar o jogo!");
        }
    }

    /// <summary>
    /// Evento: Sala de Evento Aleatório
    /// TODO: Criar eventos procedurais
    /// </summary>
    private void OnEventRoom(RoomNode room)
    {
        Debug.Log($"<color=magenta>[EVENTO] EVENTO ALEATORIO NA SALA {room.logicalPosition}!</color>");
        Debug.Log("Sistema de eventos (não implementado ainda)");
        // TODO: Criar sistema de eventos aleatórios
    }

    /// <summary>
    /// Callback quando o jogador morre.
    /// </summary>
    private void OnPlayerDied()
    {
        isDead = true;
        isMoving = false;
        
        Debug.LogError("═══════════════════════════════════");
        Debug.LogError("      GAME OVER - VOCÊ MORREU      ");
        Debug.LogError("═══════════════════════════════════");

        // O GameController escutará o evento stats.OnPlayerDied
        // e disparará a UI de Game Over
    }

    // ═══ MÉTODOS PÚBLICOS PARA DEBUG/INSPEÇÃO ═══

    public bool IsMoving() => isMoving;
    public bool IsDead() => isDead;
    public RoomNode GetCurrentRoom() => currentRoom;

    void OnDestroy()
    {
        // Limpa eventos para evitar memory leaks
        if (stats != null)
        {
            stats.OnPlayerDied -= OnPlayerDied;
        }
    }
}
