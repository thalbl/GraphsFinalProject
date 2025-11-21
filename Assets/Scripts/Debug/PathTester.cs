using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour de debug para testar o sistema A* visualmente.
/// Permite clicar em salas para definir início/fim e alternar tipos de custo.
/// </summary>
public class PathTester : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DungeonGenerator dungeonGenerator;
    [SerializeField] private DungeonGraph dungeonGraph;

    [Header("Debug Info")]
    [SerializeField] private CostType currentCostType = CostType.Health;
    
    // Estado da seleção
    private RoomNode startRoom = null;
    private RoomNode endRoom = null;
    private List<RoomNode> currentPath = null;

    // Cores para visualização
    private Color startColor = Color.magenta;
    private Color endColor = Color.cyan;
    private Color pathColor = Color.cyan;

    // Cache das cores originais para restaurar
    private Dictionary<RoomNode, Color> originalColors = new Dictionary<RoomNode, Color>();

    void Start()
    {
        // Busca referências se não foram atribuídas no Inspector
        if (dungeonGenerator == null)
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        if (dungeonGraph == null)
            dungeonGraph = FindObjectOfType<DungeonGraph>();

        if (dungeonGenerator == null || dungeonGraph == null)
        {
            Debug.LogError("PathTester: DungeonGenerator ou DungeonGraph não encontrado na cena!");
            enabled = false;
            return;
        }

        // Se o dungeon já foi gerado, armazena cores originais
        if (dungeonGenerator.allRooms != null)
        {
            CacheOriginalColors();
        }
        else
        {
            // Se ainda não foi gerado, espera o evento
            dungeonGenerator.OnDungeonGenerated += CacheOriginalColors;
        }

        Debug.Log("PathTester ativado! Clique em salas para definir início/fim. Use teclas 1/2/3 para alternar custos.");
    }

    void Update()
    {
        // Input de teclado para alternar tipos de custo
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            ChangeCostType(CostType.Health);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            ChangeCostType(CostType.Sanity);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            ChangeCostType(CostType.Time);
        }

        // Tecla R para resetar
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSelection();
        }
    }

    /// <summary>
    /// Chamado pelo RoomVisual quando uma sala é clicada.
    /// </summary>
    public void OnRoomClicked(RoomNode room)
    {
        if (room == null) return;

        // Primeiro clique: Define início
        if (startRoom == null)
        {
            SetStartRoom(room);
        }
        // Segundo clique: Define fim e calcula rota
        else if (endRoom == null)
        {
            // Se clicar na mesma sala, ignora
            if (room == startRoom)
            {
                Debug.Log("PathTester: Clique em uma sala diferente para o destino!");
                return;
            }

            SetEndRoom(room);
            CalculateAndDisplayPath();
        }
        // Se já tem início e fim, reseta e começa de novo
        else
        {
            ResetSelection();
            SetStartRoom(room);
        }
    }

    /// <summary>
    /// Define a sala inicial.
    /// </summary>
    private void SetStartRoom(RoomNode room)
    {
        startRoom = room;
        
        // Pinta a sala de início de magenta
        SetRoomColor(room, startColor);
        
        Debug.Log($"PathTester: Sala inicial definida: {room.logicalPosition} ({room.roomType})");
    }

    /// <summary>
    /// Define a sala final.
    /// </summary>
    private void SetEndRoom(RoomNode room)
    {
        endRoom = room;
        Debug.Log($"PathTester: Sala final definida: {room.logicalPosition} ({room.roomType})");
    }

    /// <summary>
    /// Calcula o caminho usando A* e exibe visualmente.
    /// </summary>
    private void CalculateAndDisplayPath()
    {
        if (startRoom == null || endRoom == null)
        {
            Debug.LogWarning("PathTester: Start ou End não definidos!");
            return;
        }

        // Limpa visualização anterior se existir
        if (currentPath != null)
        {
            ClearPathVisualization();
        }

        // Calcula o caminho
        currentPath = AStarPathfinder.FindPath(dungeonGraph, startRoom, endRoom, currentCostType);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning($"PathTester: Nenhum caminho encontrado de {startRoom.logicalPosition} para {endRoom.logicalPosition}!");
            return;
        }

        // Calcula e exibe o custo total
        float totalCost = AStarPathfinder.CalculatePathCost(dungeonGraph, currentPath, currentCostType);

        Debug.Log($"═══════════════════════════════════════");
        Debug.Log($"CAMINHO ENCONTRADO!");
        Debug.Log($"Tipo de Custo: {currentCostType}");
        Debug.Log($"Início: {startRoom.logicalPosition} ({startRoom.roomType})");
        Debug.Log($"Fim: {endRoom.logicalPosition} ({endRoom.roomType})");
        Debug.Log($"Número de salas: {currentPath.Count}");
        Debug.Log($"Custo Total ({currentCostType}): {totalCost:F2}");
        Debug.Log($"───────────────────────────────────────");
        
        // Exibe o caminho detalhado
        System.Text.StringBuilder pathString = new System.Text.StringBuilder("Caminho: ");
        for (int i = 0; i < currentPath.Count; i++)
        {
            pathString.Append(currentPath[i].logicalPosition);
            if (i < currentPath.Count - 1)
            {
                pathString.Append(" → ");
                
                // Exibe custo de cada aresta
                EdgeData edgeData = dungeonGraph.GetEdgeData(currentPath[i], currentPath[i + 1]);
                if (edgeData != null)
                {
                    float stepCost = 0;
                    switch (currentCostType)
                    {
                        case CostType.Health: stepCost = edgeData.costHealth; break;
                        case CostType.Sanity: stepCost = edgeData.costSanity; break;
                        case CostType.Time: stepCost = edgeData.costTime; break;
                    }
                    pathString.Append($"[{stepCost:F1}] → ");
                }
            }
        }
        Debug.Log(pathString.ToString());
        Debug.Log($"═══════════════════════════════════════");

        // Visualiza o caminho
        VisualizePathInScene();
    }

    /// <summary>
    /// Pinta as salas do caminho de ciano.
    /// </summary>
    private void VisualizePathInScene()
    {
        if (currentPath == null) return;

        foreach (RoomNode room in currentPath)
        {
            // Mantém a cor especial do início
            if (room == startRoom)
            {
                SetRoomColor(room, startColor);
            }
            // Pinta o resto do caminho de ciano
            else
            {
                SetRoomColor(room, pathColor);
            }
        }
    }

    /// <summary>
    /// Limpa a visualização do caminho atual.
    /// </summary>
    private void ClearPathVisualization()
    {
        if (currentPath != null)
        {
            foreach (RoomNode room in currentPath)
            {
                RestoreRoomColor(room);
            }
        }
    }

    /// <summary>
    /// Altera o tipo de custo e recalcula o caminho se já houver seleção.
    /// </summary>
    private void ChangeCostType(CostType newCostType)
    {
        if (currentCostType == newCostType)
            return;

        currentCostType = newCostType;
        Debug.Log($"PathTester: Tipo de custo alterado para {currentCostType}");

        // Se já temos start e end, recalcula o caminho
        if (startRoom != null && endRoom != null)
        {
            CalculateAndDisplayPath();
        }
    }

    /// <summary>
    /// Reseta a seleção e limpa visualizações.
    /// </summary>
    private void ResetSelection()
    {
        // Restaura cores
        if (startRoom != null)
            RestoreRoomColor(startRoom);
        if (endRoom != null)
            RestoreRoomColor(endRoom);
        
        ClearPathVisualization();

        // Limpa estado
        startRoom = null;
        endRoom = null;
        currentPath = null;

        Debug.Log("PathTester: Seleção resetada. Clique em uma sala para começar.");
    }

    /// <summary>
    /// Armazena as cores originais de todas as salas.
    /// </summary>
    private void CacheOriginalColors()
    {
        originalColors.Clear();

        if (dungeonGenerator == null || dungeonGenerator.allRooms == null)
            return;

        foreach (RoomNode room in dungeonGenerator.allRooms)
        {
            if (room.visualInstance != null)
            {
                SpriteRenderer sr = room.visualInstance.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    originalColors[room] = sr.color;
                }
            }
        }
    }

    /// <summary>
    /// Muda a cor visual de uma sala.
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

        if (originalColors.TryGetValue(room, out Color originalColor))
        {
            SetRoomColor(room, originalColor);
        }
        else
        {
            // Se não tiver cor armazenada, pega do generator
            Color roomColor = dungeonGenerator.GetRoomColor(room);
            SetRoomColor(room, roomColor);
        }
    }

    /// <summary>
    /// Validação/Debug no Inspector.
    /// </summary>
    void OnValidate()
    {
        // Debug visual no inspector
        if (startRoom != null && endRoom != null && currentPath != null)
        {
            gameObject.name = $"PathTester [{currentCostType}] ({currentPath.Count} rooms)";
        }
        else
        {
            gameObject.name = $"PathTester [{currentCostType}]";
        }
    }
}
