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

    [Header("Estado do Jogo")]
    [SerializeField] private RoomNode playerCurrentRoom; // Sala atual do jogador
    [SerializeField] private bool allowRoomSelection = true;

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

        // Registra evento de seleção de custo
        if (costSelectionMenu != null)
        {
            costSelectionMenu.OnCostSelected += OnCostTypeSelected;
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
            Debug.Log($"Sala inicial definida: {playerCurrentRoom.logicalPosition}");
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
    /// Chamado quando uma sala é clicada (pelo RoomVisual).
    /// </summary>
    public void OnRoomClicked(RoomNode clickedRoom)
    {
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

        // Verifica se há caminho possível
        if (!HasPathToRoom(clickedRoom))
        {
            Debug.LogWarning($"Não há caminho para {clickedRoom.logicalPosition}!");
            return;
        }

        // Salva destino e abre menu de seleção
        selectedDestination = clickedRoom;
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
        Debug.Log($"Calculando caminho com custo: {selectedCostType}");

        // Calcula o caminho com o custo selecionado
        CalculateAndShowPath(selectedCostType);

        // Permite nova seleção
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
