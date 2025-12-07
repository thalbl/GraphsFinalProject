using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gerencia o sistema de GPS que permite ao jogador revelar o melhor caminho
/// até uma sala escolhida ao custo de Sanidade.
/// </summary>
public class PathfinderGPS : MonoBehaviour
{
    [Header("Referências de Sistema")]
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private DungeonGraph dungeonGraph;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CostSelectionMenu costSelectionMenu;
    [SerializeField] private GameController gameController;

    [Header("Configuração do GPS")]
    [SerializeField] private float sanityCost = 20f; // Custo em sanidade para usar o GPS
    [SerializeField] private Color gpsPathColor = new Color(0.2f, 0.6f, 1f); // Azul para arestas
    [SerializeField] private Color gpsRoomBorderColor = new Color(0.3f, 0.7f, 1f, 1f); // Azul para bordas das salas

    // Instância estática para acesso global
    private static PathfinderGPS instance;
    public static PathfinderGPS Instance => instance;

    // Estado interno
    private bool isWaitingForRoomSelection = false;
    private bool isWaitingForCostTypeSelection = false;
    private RoomNode selectedDestination = null;
    private List<RoomNode> currentGPSPath = null;
    private CostType selectedCostType;

    // Referências visuais
    private Dictionary<(RoomNode, RoomNode), Material> gpsEdgeMaterials = new Dictionary<(RoomNode, RoomNode), Material>();
    private List<RoomVisual> highlightedRoomVisuals = new List<RoomVisual>();

    private void Awake()
    {
        // Define instância singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("[GPS] Múltiplas instâncias de PathfinderGPS detectadas!");
        }

        // Auto-detecta componentes se não foram setados
        if (dungeonGenerator == null)
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGraph == null)
            dungeonGraph = FindObjectOfType<DungeonGraph>();

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        if (costSelectionMenu == null)
            costSelectionMenu = FindObjectOfType<CostSelectionMenu>();

        if (gameController == null)
            gameController = FindObjectOfType<GameController>();
    }

    private void Start()
    {
        // Registra eventos
        if (costSelectionMenu != null)
        {
            costSelectionMenu.OnCostSelected += OnGPSCostTypeSelected;
            costSelectionMenu.OnCancelled += OnGPSSelectionCancelled;
        }

        if (playerController != null)
        {
            RegisterPlayerEvents();
        }
    }

    /// <summary>
    /// Define a referência do PlayerController (chamado pelo GameController após instanciar o player)
    /// </summary>
    public void SetPlayerController(PlayerController player)
    {
        if (playerController != null)
        {
            // Remove eventos do player anterior
            playerController.stats.OnStatsChanged -= OnStatsChangedHandler;
        }

        playerController = player;

        if (playerController != null)
        {
            RegisterPlayerEvents();
            Debug.Log("[GPS] PlayerController configurado com sucesso");
        }
    }

    /// <summary>
    /// Registra eventos do player
    /// </summary>
    private void RegisterPlayerEvents()
    {
        if (playerController != null)
        {
            // Limpa GPS quando o jogador se move
            playerController.stats.OnStatsChanged += OnStatsChangedHandler;
        }
    }

    /// <summary>
    /// Handler para mudanças de stats do player
    /// </summary>
    private void OnStatsChangedHandler(float h, float s, float su)
    {
        CheckIfPlayerMovedAndClearGPS();
    }


    /// <summary>
    /// Inicia o processo de usar o GPS
    /// </summary>
    public void ActivateGPS()
    {
        if (playerController == null)
        {
            Debug.LogError("[GPS] PlayerController não encontrado!");
            return;
        }

        // Verifica se o jogador tem sanidade suficiente
        if (playerController.stats.currentSanity < sanityCost)
        {
            Debug.LogWarning($"[GPS] Sanidade insuficiente! Requer {sanityCost} SP, possui {playerController.stats.currentSanity:F1} SP");
            EventLogger.LogDanger($"Sanidade insuficiente para usar o GPS! Requer {sanityCost:F0} SP");
            return;
        }

        // Se já existe um caminho GPS ativo, limpa ele
        if (currentGPSPath != null)
        {
            ClearGPSPath();
        }

        // Inicia modo de seleção de sala
        isWaitingForRoomSelection = true;
        Debug.Log("[GPS] Modo de seleção de sala ativado. Clique em uma sala de destino.");
        EventLogger.LogInfo($"GPS ativado. Clique em uma sala de destino. (Custo: {sanityCost:F0} SP)");
    }

    /// <summary>
    /// Cancela o processo de GPS
    /// </summary>
    public void CancelGPS()
    {
        isWaitingForRoomSelection = false;
        isWaitingForCostTypeSelection = false;
        selectedDestination = null;
        ClearGPSPath();
        
        Debug.Log("[GPS] GPS cancelado pelo jogador");
        EventLogger.LogInfo("GPS cancelado");
    }

    /// <summary>
    /// Chamado quando o jogador clica em uma sala enquanto o GPS está ativo
    /// </summary>
    public void OnRoomSelectedForGPS(RoomNode room)
    {
        if (!isWaitingForRoomSelection)
            return;

        if (room == null)
        {
            Debug.LogWarning("[GPS] Sala selecionada é null!");
            return;
        }

        RoomNode currentRoom = playerController.GetCurrentRoom();
        
        // Não permite selecionar a sala atual
        if (room == currentRoom)
        {
            Debug.Log("[GPS] Você já está nesta sala!");
            EventLogger.LogInfo("Você já está nesta sala!");
            return;
        }

        // Salva destino e abre menu de seleção de tipo de custo
        selectedDestination = room;
        isWaitingForRoomSelection = false;
        isWaitingForCostTypeSelection = true;

        Debug.Log($"[GPS] Destino selecionado: {room.logicalPosition}. Abrindo menu de tipo de custo...");
        
        // Abre o menu de seleção de custo
        if (costSelectionMenu != null)
        {
            costSelectionMenu.ShowMenu(selectedDestination, currentRoom, dungeonGraph);
        }
        else
        {
            Debug.LogError("[GPS] CostSelectionMenu não encontrado!");
        }
    }

    /// <summary>
    /// Chamado quando o tipo de custo é selecionado no menu
    /// </summary>
    private void OnGPSCostTypeSelected(CostType costType)
    {
        if (!isWaitingForCostTypeSelection)
            return;

        selectedCostType = costType;
        isWaitingForCostTypeSelection = false;

        Debug.Log($"[GPS] Tipo de custo selecionado: {costType}");

        // Calcula e visualiza o caminho
        CalculateAndVisualizeGPSPath();
    }

    /// <summary>
    /// Chamado quando a seleção é cancelada
    /// </summary>
    private void OnGPSSelectionCancelled()
    {
        if (isWaitingForCostTypeSelection)
        {
            // Cancela o GPS
            CancelGPS();
        }
    }

    /// <summary>
    /// Calcula o caminho usando A* e visualiza
    /// </summary>
    private void CalculateAndVisualizeGPSPath()
    {
        if (playerController == null || selectedDestination == null || dungeonGraph == null)
        {
            Debug.LogError("[GPS] Dados inválidos para calcular caminho!");
            return;
        }

        RoomNode currentRoom = playerController.GetCurrentRoom();

        // Deduz sanidade
        playerController.stats.currentSanity -= sanityCost;
        playerController.stats.NotifyStatsChanged();
        
        Debug.Log($"[GPS] {sanityCost:F0} SP deduzidos. Sanidade atual: {playerController.stats.currentSanity:F1}");
        EventLogger.LogSanity($"GPS utilizado. -{sanityCost:F0} SP");

        // Calcula caminho usando A*
        currentGPSPath = AStarPathfinder.FindPath(
            dungeonGraph,
            currentRoom,
            selectedDestination,
            selectedCostType
        );

        if (currentGPSPath == null || currentGPSPath.Count == 0)
        {
            Debug.LogWarning("[GPS] Nenhum caminho encontrado!");
            EventLogger.LogDanger("GPS: Nenhum caminho encontrado para o destino!");
            selectedDestination = null;
            return;
        }

        // Calcula custo total
        float totalCost = AStarPathfinder.CalculatePathCost(dungeonGraph, currentGPSPath, selectedCostType);

        // Log do resultado
        Debug.Log($"═══════════════════════════════════════");
        Debug.Log($"[GPS] CAMINHO CALCULADO!");
        Debug.Log($"[GPS] Tipo: {selectedCostType}");
        Debug.Log($"[GPS] De: {currentRoom.logicalPosition} → Para: {selectedDestination.logicalPosition}");
        Debug.Log($"[GPS] Salas: {currentGPSPath.Count} | Custo: {totalCost:F2}");
        Debug.Log($"═══════════════════════════════════════");

        string costLabel = selectedCostType switch
        {
            CostType.Health => "Vida",
            CostType.Sanity => "Sanidade",
            CostType.Time => "Tempo",
            _ => "Desconhecido"
        };

        EventLogger.LogInfo($"GPS: Caminho encontrado! {currentGPSPath.Count} salas, custo total: {totalCost:F1} {costLabel}");

        // Visualiza o caminho
        VisualizeGPSPath();
    }

    /// <summary>
    /// Visualiza o caminho do GPS pintando arestas e destacando salas
    /// </summary>
    private void VisualizeGPSPath()
    {
        if (currentGPSPath == null || currentGPSPath.Count < 2)
            return;

        // Pinta as arestas do caminho
        for (int i = 0; i < currentGPSPath.Count - 1; i++)
        {
            RoomNode from = currentGPSPath[i];
            RoomNode to = currentGPSPath[i + 1];

            HighlightEdge(from, to);
        }

        // Destaca as salas do caminho (exceto a sala atual)
        for (int i = 1; i < currentGPSPath.Count; i++)
        {
            RoomNode room = currentGPSPath[i];
            HighlightRoom(room);
        }

        Debug.Log($"[GPS] Caminho visualizado com {currentGPSPath.Count} salas");
    }

    /// <summary>
    /// Destaca uma aresta do caminho GPS
    /// </summary>
    private void HighlightEdge(RoomNode from, RoomNode to)
    {
        if (dungeonGenerator == null || dungeonGenerator.visualizer == null)
            return;

        // Acessa o dicionário de LineRenderers do visualizer
        var edgeRenderers = dungeonGenerator.visualizer.GetEdgeRenderers();
        
        if (edgeRenderers != null && edgeRenderers.TryGetValue((from, to), out LineRenderer lr))
        {
            if (lr != null && lr.material != null)
            {
                // Cria uma cópia do material se ainda não foi criada
                if (!gpsEdgeMaterials.ContainsKey((from, to)))
                {
                    Material gpsMaterial = new Material(lr.material);
                    gpsEdgeMaterials[(from, to)] = gpsMaterial;
                }

                lr.material = gpsEdgeMaterials[(from, to)];
                lr.material.color = gpsPathColor;
                lr.startWidth = 0.25f; // Mais grosso
                lr.endWidth = 0.25f;

                Debug.Log($"[GPS] Aresta destacada: {from.logicalPosition} → {to.logicalPosition}");
            }
        }
    }

    /// <summary>
    /// Destaca uma sala do caminho GPS
    /// </summary>
    private void HighlightRoom(RoomNode room)
    {
        if (room.visualInstance == null)
            return;

        RoomVisual roomVisual = room.visualInstance.GetComponent<RoomVisual>();
        if (roomVisual != null)
        {
            roomVisual.SetGPSHighlight(true);
            highlightedRoomVisuals.Add(roomVisual);
        }
    }

    /// <summary>
    /// Limpa a visualização do caminho GPS
    /// </summary>
    public void ClearGPSPath()
    {
        if (currentGPSPath == null)
            return;

        // Restaura arestas
        if (dungeonGenerator != null && dungeonGenerator.visualizer != null)
        {
            var edgeRenderers = dungeonGenerator.visualizer.GetEdgeRenderers();
            
            for (int i = 0; i < currentGPSPath.Count - 1; i++)
            {
                RoomNode from = currentGPSPath[i];
                RoomNode to = currentGPSPath[i + 1];

                if (edgeRenderers != null && edgeRenderers.TryGetValue((from, to), out LineRenderer lr))
                {
                    if (lr != null && lr.material != null)
                    {
                        // Restaura cor padrão
                        lr.material.color = new Color(0.8f, 0.8f, 0.8f);
                        lr.startWidth = 0.15f;
                        lr.endWidth = 0.15f;
                    }
                }
            }
        }

        // Remove highlight das salas
        foreach (RoomVisual roomVisual in highlightedRoomVisuals)
        {
            if (roomVisual != null)
            {
                roomVisual.SetGPSHighlight(false);
            }
        }
        highlightedRoomVisuals.Clear();

        // Limpa materiais GPS
        foreach (var material in gpsEdgeMaterials.Values)
        {
            if (material != null)
                Destroy(material);
        }
        gpsEdgeMaterials.Clear();

        currentGPSPath = null;
        selectedDestination = null;

        Debug.Log("[GPS] Caminho GPS limpo");
    }

    /// <summary>
    /// Verifica se o jogador se moveu e limpa o GPS
    /// </summary>
    private void CheckIfPlayerMovedAndClearGPS()
    {
        if (currentGPSPath != null && currentGPSPath.Count > 0)
        {
            RoomNode currentRoom = playerController?.GetCurrentRoom();
            if (currentRoom != null && currentRoom != currentGPSPath[0])
            {
                // Jogador se moveu, limpa o GPS
                ClearGPSPath();
                EventLogger.LogNeutral("GPS: Caminho apagado após movimento");
            }
        }
    }

    // Propriedades públicas
    public bool IsWaitingForRoomSelection => isWaitingForRoomSelection;
    public bool CanUseGPS => playerController != null && playerController.stats.currentSanity >= sanityCost;
    public float SanityCost => sanityCost;
    public Color GPSRoomBorderColor => gpsRoomBorderColor; // Cor da borda configurável

    private void OnDestroy()
    {
        // Limpa materiais
        ClearGPSPath();

        // Remove eventos
        if (costSelectionMenu != null)
        {
            costSelectionMenu.OnCostSelected -= OnGPSCostTypeSelected;
            costSelectionMenu.OnCancelled -= OnGPSSelectionCancelled;
        }

        if (playerController != null)
        {
            playerController.stats.OnStatsChanged -= OnStatsChangedHandler;
        }
    }
}
