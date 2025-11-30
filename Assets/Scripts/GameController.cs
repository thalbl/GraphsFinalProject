using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controlador principal do gameplay que gerencia interação com salas e seleção de custos.
/// Orquestra o fluxo: click em sala → menu de custo → pathfinding → visualização.
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("Referências de Sistema")]
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private DungeonGraph dungeonGraph;
    [SerializeField] private CostSelectionMenu costSelectionMenu;
    [SerializeField] private GameOverUI gameOverUI;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab; // Prefab do jogador (sprite simples)
    private PlayerController playerController; // Instância do player

    [Header("Estado do Jogo")]
    [SerializeField] private RoomNode playerCurrentRoom; // Sala atual do jogador
    [SerializeField] private bool allowRoomSelection = true;
    private bool isGameOver = false;

    [Header("Visualização de Caminho")]
    [SerializeField] private Color pathColor = Color.cyan;
    [SerializeField] private Color startRoomColor = Color.magenta;

    // Estado interno
    private RoomNode selectedDestination;
    private List<RoomNode> currentPath;
    private Dictionary<RoomNode, Color> originalRoomColors = new Dictionary<RoomNode, Color>();

    void Start()
    {
        // Auto-detecta componentes se não foram setados
        if (dungeonGenerator == null)
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGraph == null)
            dungeonGraph = FindObjectOfType<DungeonGraph>();

        if (costSelectionMenu == null)
            costSelectionMenu = FindObjectOfType<CostSelectionMenu>();

        if (gameOverUI == null)
            gameOverUI = FindObjectOfType<GameOverUI>();

        // Registra evento de seleção de custo
        if (costSelectionMenu != null)
        {
            costSelectionMenu.OnCostSelected += OnCostTypeSelected;
            costSelectionMenu.OnCancelled += OnSelectionCancelled;
        }

        // Cacheia cores originais das salas
        if (dungeonGenerator != null)
        {
            // Sempre registra o evento para quando o dungeon for gerado/regenerado
            dungeonGenerator.OnDungeonGenerated += OnDungeonGenerated;
            
            // Se o dungeon já foi gerado, processa agora
            if (dungeonGenerator.allRooms != null && dungeonGenerator.allRooms.Count > 0)
            {
                OnDungeonGenerated();
            }
        }

        Debug.Log("GameController inicializado!");
    }

    /// <summary>
    /// Chamado quando o dungeon é gerado ou regenerado.
    /// </summary>
    private void OnDungeonGenerated()
    {
        // Cacheia cores  originais
        CacheOriginalColors();
        
        // Define sala inicial como a spawn room
        if (dungeonGenerator != null && dungeonGenerator.spawnRoom != null)
        {
            playerCurrentRoom = dungeonGenerator.spawnRoom;
            HighlightCurrentRoom();
            
            // ═══ DESTACA ARESTAS ACESSÍVEIS DA SALA INICIAL ═══
            dungeonGenerator.HighlightPlayerAccessibleEdges(playerCurrentRoom);
            
            Debug.Log($"Sala inicial definida: {playerCurrentRoom.logicalPosition}");

            // ═══ CORREÇÃO: Pequeno delay para garantir sincronização ═══
            StartCoroutine(InstantiatePlayerWithDelay());
        }
        else
        {
            Debug.LogError("Spawn room não encontrada!");
        }

        // Debug: verifica se o grafo foi populado
        if (dungeonGraph != null)
        {
            var allNodes = dungeonGraph.GetAllNodes();
            Debug.Log($"DungeonGraph contém {allNodes.Count} nós");
            
            // Testa se há arestas
            if (allNodes.Count > 0 && allNodes[0].connections != null)
            {
                Debug.Log($"Primeira sala tem {allNodes[0].connections.Count} conexões");
            }
        }
        else
        {
            Debug.LogError("DungeonGraph é null!");
        }
    }

    /// <summary>
    /// Instancia o player com um pequeno delay para garantir sincronização.
    /// </summary>
    private System.Collections.IEnumerator InstantiatePlayerWithDelay()
    {
        // Espera um frame para garantir que o CameraController terminou seu setup
        yield return null;
        
        InstantiatePlayer(dungeonGenerator.spawnRoom);
    }

    /// <summary>
    /// Instancia o player na sala de spawn.
    /// </summary>
    private void InstantiatePlayer(RoomNode spawnRoom)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab não atribuído no GameController!");
            Debug.LogWarning("Crie um GameObject com SpriteRenderer e atribua no Inspector.");
            return;
        }

        // Destroi player anterior se existir (para regeneração)
        if (playerController != null)
        {
            Destroy(playerController.gameObject);
        }

        // Instancia o prefab
        GameObject playerGO = Instantiate(playerPrefab, spawnRoom.GetWorldPosition(), Quaternion.identity);
        playerGO.name = "Player";

        // Pega o componente PlayerController
        playerController = playerGO.GetComponent<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogError("Player Prefab não tem componente PlayerController!");
            return;
        }

        // Inicializa o player
        playerController.Initialize(spawnRoom);

        // Registra evento de morte
        playerController.stats.OnPlayerDied += OnPlayerDied;

        Debug.Log("═══ PLAYER INSTANCIADO COM SUCESSO ═══");

        // ═══ CORREÇÃO: USA O CAMERA CONTROLLER PARA CENTRALIZAR ═══
        CenterCameraOnPlayerUsingCameraController();
    }

    /// <summary>
    /// Centraliza a câmera no player usando o CameraController (em vez de mover diretamente)
    /// </summary>
    private void CenterCameraOnPlayerUsingCameraController()
    {
        if (playerController == null) return;

        // Encontra o CameraController
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            // Foca na sala atual do player
            cameraController.FocusOnRoom(playerCurrentRoom, 0.5f);
            Debug.Log($"📷 CameraController focando na sala do player: {playerCurrentRoom.logicalPosition}");
        }
        else
        {
            // Fallback: método antigo se não encontrar CameraController
            Debug.LogWarning("CameraController não encontrado, usando fallback...");
            CenterCameraOnPlayer();
        }
    }

    /// <summary>
    /// Centraliza a câmera na posição do player (fallback)
    /// </summary>
    private void CenterCameraOnPlayer()
    {
        if (playerController == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 playerPos = playerController.transform.position;
            Vector3 newCameraPos = new Vector3(playerPos.x, playerPos.y, mainCamera.transform.position.z);
            mainCamera.transform.position = newCameraPos;
            
            Debug.Log($"📷 Câmera centralizada no player (fallback): {newCameraPos}");
        }
        else
        {
            Debug.LogWarning("Camera.main não encontrada!");
        }
    }

    /// <summary>
    /// Callback quando o jogador morre.
    /// </summary>
    private void OnPlayerDied()
    {
        isGameOver = true;
        allowRoomSelection = false;
        
        Debug.LogError("════════════════════════════════════");
        Debug.LogError("   GAME OVER - GameController      ");
        Debug.LogError("════════════════════════════════════");

        // IMPORTANTE: Ativa UI ANTES de pausar!
        // Isso permite que as coroutines iniciem corretamente
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverUI não encontrada! Adicione o componente GameOverUI na cena.");
        }

        // Pausa o jogo DEPOIS da UI estar ativa
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Chamado quando uma sala é clicada (pelo RoomVisual).
    /// </summary>
    public void OnRoomClicked(RoomNode clickedRoom)
    {
        if (isGameOver)
        {
            Debug.Log("Game Over - seleção desabilitada.");
            return;
        }

        if (!allowRoomSelection)
        {
            Debug.Log("Seleção de sala desabilitada no momento.");
            return;
        }

        if (clickedRoom == null)
        {
            Debug.LogWarning("Sala clicada é null!");
            return;
        }

        // Debug: mostra estado atual
        Debug.Log($"─── Click em sala {clickedRoom.logicalPosition} ───");
        Debug.Log($"Sala atual do jogador: {(playerCurrentRoom != null ? playerCurrentRoom.logicalPosition.ToString() : "NULL")}");
        Debug.Log($"DungeonGraph: {(dungeonGraph != null ? "OK" : "NULL")}");
        
        if (playerCurrentRoom != null)
        {
            Debug.Log($"Sala atual tem {playerCurrentRoom.connections?.Count ?? 0} conexões");
        }

        // Ignora se clicar na sala atual
        if (clickedRoom == playerCurrentRoom)
        {
            Debug.Log("Você já está nesta sala!");
            return;
        }

        // ═══ VALIDAÇÃO: APENAS SALAS ADJACENTES POR ENQUANTO ═══
        if (!playerCurrentRoom.connections.Contains(clickedRoom))
        {
            Debug.LogWarning($"Sala {clickedRoom.logicalPosition} não está adjacente! Clique em uma sala conectada.");
            return;
        }

        // Salva destino e abre menu de seleção
        selectedDestination = clickedRoom;
        
        // Log de seleção de sala usando sistema narrativo
        if (NarrativeLogSystem.Instance != null)
        {
            NarrativeLogSystem.Instance.LogRoomSelection(clickedRoom);
        }
        else
        {
            // Fallback se NarrativeLogSystem não estiver na cena
            EventLogger.LogRoomSelection($"{clickedRoom.roomType} ({clickedRoom.logicalPosition})");
        }
        
        OpenCostSelectionMenu();
    }

    /// <summary>
    /// Abre o menu de seleção de custo.
    /// </summary>
    private void OpenCostSelectionMenu()
    {
        if (costSelectionMenu == null)
        {
            Debug.LogError("CostSelectionMenu não encontrado!");
            return;
        }

        allowRoomSelection = false;
        costSelectionMenu.ShowMenu(selectedDestination, playerCurrentRoom, dungeonGraph);
    }

    /// <summary>
    /// Chamado quando o jogador seleciona um tipo de custo no menu.
    /// </summary>
    private void OnCostTypeSelected(CostType selectedCostType)
    {
        Debug.Log($"Tipo de custo selecionado: {selectedCostType}");

        // ═══ DELEGA MOVIMENTO AO PLAYERCONTROLLER ═══
        if (playerController != null && selectedDestination != null)
        {
            // Limpa visualização anterior
            ClearCurrentPath();

            // Inicia movimento do player
            playerController.MoveTo(selectedDestination, selectedCostType);

            // Nota: O PlayerController chamará MovePlayerToRoom() quando completar
        }
        else
        {
            Debug.LogError("PlayerController ou selectedDestination é null!");
        }

        // Permite nova seleção após movimento
        allowRoomSelection = true;
    }

    /// <summary>
    /// Chamado quando o jogador cancela a seleção de sala.
    /// </summary>
    private void OnSelectionCancelled()
    {
        Debug.Log("Seleção cancelada pelo jogador");

        // Limpa destino selecionado
        selectedDestination = null;

        // Re-habilita seleção de sala
        allowRoomSelection = true;
    }

    /// <summary>
    /// Calcula e mostra o caminho usando A*.
    /// </summary>
    private void CalculateAndShowPath(CostType costType)
    {
        if (playerCurrentRoom == null || selectedDestination == null || dungeonGraph == null)
        {
            Debug.LogError("Dados inválidos para calcular caminho!");
            return;
        }

        // Limpa caminho anterior
        ClearCurrentPath();

        // Calcula novo caminho
        currentPath = AStarPathfinder.FindPath(
            dungeonGraph,
            playerCurrentRoom,
            selectedDestination,
            costType
        );

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("Nenhum caminho encontrado!");
            return;
        }

        // Calcula custo total
        float totalCost = AStarPathfinder.CalculatePathCost(dungeonGraph, currentPath, costType);

        // Log do resultado
        Debug.Log($"═══════════════════════════════════════");
        Debug.Log($"CAMINHO CALCULADO!");
        Debug.Log($"Tipo de Custo: {costType}");
        Debug.Log($"De: {playerCurrentRoom.logicalPosition} ({playerCurrentRoom.roomType})");
        Debug.Log($"Para: {selectedDestination.logicalPosition} ({selectedDestination.roomType})");
        Debug.Log($"Salas no caminho: {currentPath.Count}");
        Debug.Log($"Custo Total: {totalCost:F2}");
        Debug.Log($"═══════════════════════════════════════");

        // Visualiza o caminho
        VisualizePath();
    }

    /// <summary>
    /// Visualiza o caminho pintando as salas.
    /// </summary>
    private void VisualizePath()
    {
        if (currentPath == null) return;

        foreach (RoomNode room in currentPath)
        {
            if (room == playerCurrentRoom)
            {
                // Mantém cor da sala atual
                SetRoomColor(room, startRoomColor);
            }
            else
            {
                // Pinta caminho
                SetRoomColor(room, pathColor);
            }
        }
    }

    /// <summary>
    /// Limpa a visualização do caminho atual.
    /// </summary>
    private void ClearCurrentPath()
    {
        if (currentPath != null)
        {
            foreach (RoomNode room in currentPath)
            {
                RestoreRoomColor(room);
            }
            currentPath = null;
        }

        // Re-destaca sala atual
        if (playerCurrentRoom != null)
        {
            HighlightCurrentRoom();
        }
    }

    /// <summary>
    /// Move o jogador para uma sala (para uso futuro).
    /// </summary>
    public void MovePlayerToRoom(RoomNode room)
    {
        if (room == null) return;

        // Limpa path anterior
        ClearCurrentPath();

        // Atualiza sala atual
        playerCurrentRoom = room;
        
        // Destaca nova sala
        HighlightCurrentRoom();
        
        // ═══ DESTACA ARESTAS ACESSÍVEIS DA NOVA POSIÇÃO ═══
        if (dungeonGenerator != null)
        {
            dungeonGenerator.HighlightPlayerAccessibleEdges(playerCurrentRoom);
        }

        Debug.Log($"Jogador movido para {room.logicalPosition}");
    }

    /// <summary>
    /// Destaca a sala atual do jogador.
    /// </summary>
    private void HighlightCurrentRoom()
    {
        if (playerCurrentRoom != null)
        {
            SetRoomColor(playerCurrentRoom, startRoomColor);
        }
    }

    /// <summary>
    /// Verifica se há caminho para uma sala.
    /// </summary>
    private bool HasPathToRoom(RoomNode destination)
    {
        if (playerCurrentRoom == null || destination == null || dungeonGraph == null)
            return false;

        // Tenta achar caminho com qualquer custo (usa Health como padrão para teste)
        var testPath = AStarPathfinder.FindPath(
            dungeonGraph,
            playerCurrentRoom,
            destination,
            CostType.Health
        );

        return testPath != null && testPath.Count > 0;
    }

    /// <summary>
    /// Cacheia as cores originais de todas as salas.
    /// </summary>
    private void CacheOriginalColors()
    {
        originalRoomColors.Clear();

        if (dungeonGenerator == null || dungeonGenerator.allRooms == null)
            return;

        foreach (RoomNode room in dungeonGenerator.allRooms)
        {
            if (room.visualInstance != null)
            {
                SpriteRenderer sr = room.visualInstance.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    originalRoomColors[room] = sr.color;
                }
            }
        }

        Debug.Log($"Cores de {originalRoomColors.Count} salas cacheadas.");
    }

    /// <summary>
    /// Define a cor de uma sala.
    /// </summary>
    private void SetRoomColor(RoomNode room, Color color)
    {
        if (room == null || room.visualInstance == null)
            return;

        SpriteRenderer sr = room.visualInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }
    }

    /// <summary>
    /// Restaura a cor original de uma sala.
    /// </summary>
    private void RestoreRoomColor(RoomNode room)
    {
        if (room == null || room.visualInstance == null)
            return;

        if (originalRoomColors.TryGetValue(room, out Color originalColor))
        {
            SetRoomColor(room, originalColor);
        }
        else if (dungeonGenerator != null)
        {
            // Fallback: pega cor do generator
            Color roomColor = dungeonGenerator.GetRoomColor(room);
            SetRoomColor(room, roomColor);
        }
    }

    // Métodos públicos para controle externo
    public void SetPlayerStartRoom(RoomNode room)
    {
        playerCurrentRoom = room;
        HighlightCurrentRoom();
    }

    public RoomNode GetPlayerCurrentRoom() => playerCurrentRoom;
    public List<RoomNode> GetCurrentPath() => currentPath;
}
