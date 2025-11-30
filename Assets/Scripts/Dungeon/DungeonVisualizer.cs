using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Módulo responsável pela representação visual do dungeon.
/// Fase 7: Cria sprites, cores, linhas de conexão e componentes visuais.
/// </summary>
public class DungeonVisualizer
{
    private GameObject dungeonContainer;
    private Material baseMaterial; // Material base que será instanciado
    private bool directedGraph;

    // Cores de arestas
    private Color defaultEdgeColor = new Color(0.8f, 0.8f, 0.8f); // Cinza claro
    private Color accessibleEdgeColor = new Color(1f, 0.3f, 0.5f); // Rosa/vermelho

    // Cores de salas
    private Color spawnColor;
    private Color bossColor;
    private Color combatColor;
    private Color treasureColor;
    private Color campColor;
    private Color eventColor;

    // Dicionário para acessar LineRenderers por conexão
    private Dictionary<(RoomNode, RoomNode), LineRenderer> edgeRenderers = new Dictionary<(RoomNode, RoomNode), LineRenderer>();

    public DungeonVisualizer(
        GameObject container,
        Material edgeMaterial,
        bool directed,
        Color spawn, Color boss, Color combat, Color treasure, Color camp, Color evt)
    {
        this.dungeonContainer = container;
        this.baseMaterial = edgeMaterial;
        this.directedGraph = directed;
        this.spawnColor = spawn;
        this.bossColor = boss;
        this.combatColor = combat;
        this.treasureColor = treasure;
        this.campColor = camp;
        this.eventColor = evt;
    }

    /// <summary>
    /// Cria a representação visual de todas as salas e conexões
    /// </summary>
    public void CreateVisualRepresentation(List<RoomNode> allRooms)
    {
        // Cria os GameObjects das salas
        foreach (RoomNode room in allRooms)
        {
            GameObject roomGO = new GameObject($"Room_{room.logicalPosition}");
            roomGO.transform.SetParent(dungeonContainer.transform);
            roomGO.transform.position = room.GetWorldPosition();

            SpriteRenderer sr = roomGO.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRoomSprite(room.roomRect.size);
            sr.color = GetRoomColor(room);
            sr.sortingOrder = 1;

            BoxCollider2D collider = roomGO.AddComponent<BoxCollider2D>();
            collider.size = room.roomRect.size;

            room.visualInstance = roomGO;

            RoomVisual roomVisual = roomGO.AddComponent<RoomVisual>();
            roomVisual.Initialize(room);
        }

        // Cria as linhas de conexão
        foreach (RoomNode room in allRooms)
        {
            room.connectionLines = new LineRenderer[room.connections.Count];

            for (int i = 0; i < room.connections.Count; i++)
            {
                RoomNode connectedRoom = room.connections[i];

                // Evita duplicatas visuais
                bool shouldDraw = room.GetHashCode() <= connectedRoom.GetHashCode();

                if (shouldDraw)
                {
                    GameObject connectionGO = new GameObject($"Connection_{room.logicalPosition}->{connectedRoom.logicalPosition}");
                    connectionGO.transform.SetParent(dungeonContainer.transform);

                    LineRenderer lr = connectionGO.AddComponent<LineRenderer>();
                    lr.positionCount = 2;

                    // Offset para grafos direcionados
                    Vector3 offset = Vector3.zero;
                    if (directedGraph && connectedRoom.connections.Contains(room))
                    {
                        Vector3 dir = (connectedRoom.GetWorldPosition() - room.GetWorldPosition()).normalized;
                        Vector3 perp = new Vector3(-dir.y, dir.x, 0) * 0.2f;
                        offset = perp;
                    }

                    lr.SetPosition(0, room.GetWorldPosition() + offset);
                    lr.SetPosition(1, connectedRoom.GetWorldPosition() + offset);
                    lr.startWidth = 0.15f;
                    lr.endWidth = 0.15f;
                    lr.sortingOrder = 0;
                    
                    // Cria uma instância do material para esta linha
                    lr.material = UnityEngine.Object.Instantiate(baseMaterial);
                    lr.material.color = defaultEdgeColor; // Define cor padrão

                    room.connectionLines[i] = lr;
                    
                    // Armazena referência para poder mudar cor depois
                    edgeRenderers[(room, connectedRoom)] = lr;
                    edgeRenderers[(connectedRoom, room)] = lr; // Ambas direções apontam para mesma linha
                }
            }
        }

        Debug.Log($"[Visualizer] {allRooms.Count} salas visualizadas");
    }

    /// <summary>
    /// Destaca as arestas acessíveis a partir de uma sala específica
    /// </summary>
    public void HighlightAccessibleEdges(RoomNode currentRoom, List<RoomNode> allRooms)
    {
        Debug.Log($"[DungeonVisualizer] HighlightAccessibleEdges chamado. Sala: {currentRoom?.logicalPosition}");
        Debug.Log($"[DungeonVisualizer] Total de arestas no dicionário: {edgeRenderers.Count}");
        
        // Primeiro, reseta todas as arestas para cor padrão
        int resetCount = 0;
        foreach (var kvp in edgeRenderers)
        {
            if (kvp.Value != null && kvp.Value.material != null)
            {
                kvp.Value.material.color = defaultEdgeColor; // Muda apenas a cor
                resetCount++;
            }
        }
        Debug.Log($"[DungeonVisualizer] {resetCount} arestas resetadas para cor padrão");

        // Destaca apenas as conexões da sala atual mudando a cor
        if (currentRoom != null)
        {
            Debug.Log($"[DungeonVisualizer] Sala tem {currentRoom.connections.Count} conexões");
            int highlightCount = 0;
            
            foreach (RoomNode neighbor in currentRoom.connections)
            {
                Debug.Log($"[DungeonVisualizer] Tentando destacar aresta para {neighbor.logicalPosition}");
                
                if (edgeRenderers.TryGetValue((currentRoom, neighbor), out LineRenderer lr))
                {
                    if (lr != null && lr.material != null)
                    {
                        lr.material.color = accessibleEdgeColor; // Muda cor para rosa
                        highlightCount++;
                        Debug.Log($"[DungeonVisualizer] ✓ Aresta destacada para {neighbor.logicalPosition}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DungeonVisualizer] LineRenderer ou Material é null para {neighbor.logicalPosition}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DungeonVisualizer] Aresta não encontrada no dicionário: ({currentRoom.logicalPosition}, {neighbor.logicalPosition})");
                }
            }
            
            Debug.Log($"[DungeonVisualizer] Total de arestas destacadas: {highlightCount}");
        }
    }

    /// <summary>
    /// Reseta todas as arestas para cor padrão
    /// </summary>
    public void ResetAllEdgeColors()
    {
        foreach (var kvp in edgeRenderers)
        {
            if (kvp.Value != null && kvp.Value.material != null)
            {
                kvp.Value.material.color = defaultEdgeColor;
            }
        }
    }

    /// <summary>
    /// Limpa os materiais instanciados quando o dungeon é destruído
    /// </summary>
    public void Cleanup()
    {
        foreach (var kvp in edgeRenderers)
        {
            if (kvp.Value != null && kvp.Value.material != null)
            {
                UnityEngine.Object.Destroy(kvp.Value.material);
            }
        }
        edgeRenderers.Clear();
    }

    /// <summary>
    /// Cria um sprite processual para uma sala
    /// </summary>
    private Sprite CreateRoomSprite(Vector2 size)
    {
        Texture2D texture = new Texture2D((int)size.x, (int)size.y);
        Color[] colors = new Color[texture.width * texture.height];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 1f);
    }

    /// <summary>
    /// Retorna a cor apropriada para o tipo de sala
    /// </summary>
    private Color GetRoomColor(RoomNode room)
    {
        return room.roomType switch
        {
            RoomType.Spawn => spawnColor,
            RoomType.Boss => bossColor,
            RoomType.Treasure => treasureColor,
            RoomType.Camp => campColor,
            RoomType.Event => eventColor,
            _ => combatColor
        };
    }
}
