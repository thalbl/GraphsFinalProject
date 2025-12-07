using UnityEngine;

/// <summary>
/// Script de teste para verificar o sistema de destaque de arestas.
/// Adicione este componente em qualquer GameObject na cena.
/// </summary>
public class EdgeHighlightTester : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private int currentRoomIndex = 0;

    void Start()
    {
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        
        if (dungeonGenerator == null)
        {
            Debug.LogError("[EdgeHighlightTester] DungeonGenerator não encontrado!");
            return;
        }

        // Aguarda o dungeon ser gerado
        if (dungeonGenerator.OnDungeonGenerated != null)
        {
            dungeonGenerator.OnDungeonGenerated += OnDungeonReady;
        }
        else
        {
            Invoke(nameof(TestHighlight), 1f);
        }
    }

    void OnDungeonReady()
    {
        Debug.Log("[EdgeHighlightTester] Dungeon gerado! Pronto para testar.");
        TestHighlight();
    }

    void Update()
    {
        if (dungeonGenerator == null || dungeonGenerator.allRooms == null) return;

        // Pressione ESPAÇO para alternar entre salas
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CycleThroughRooms();
        }

        // Pressione R para resetar todas as arestas
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetHighlights();
        }

        // Pressione T para testar a sala spawn
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestHighlight();
        }
    }

    /// <summary>
    /// Testa destaque na sala spawn
    /// </summary>
    void TestHighlight()
    {
        if (dungeonGenerator == null || dungeonGenerator.spawnRoom == null)
        {
            Debug.LogWarning("[EdgeHighlightTester] DungeonGenerator ou spawnRoom é null!");
            return;
        }

        Debug.Log($"[EdgeHighlightTester] Destacando arestas da sala SPAWN ({dungeonGenerator.spawnRoom.logicalPosition})");
        Debug.Log($"[EdgeHighlightTester] Número de conexões: {dungeonGenerator.spawnRoom.connections.Count}");
        
        dungeonGenerator.HighlightPlayerAccessibleEdges(dungeonGenerator.spawnRoom);
        
        // Verifica se o visualizer existe
        if (dungeonGenerator.visualizer == null)
        {
            Debug.LogError("[EdgeHighlightTester] dungeonGenerator.visualizer é NULL!");
        }
    }

    /// <summary>
    /// Alterna entre diferentes salas para testar
    /// </summary>
    void CycleThroughRooms()
    {
        if (dungeonGenerator.allRooms == null || dungeonGenerator.allRooms.Count == 0)
        {
            Debug.LogWarning("[EdgeHighlightTester] Nenhuma sala disponível!");
            return;
        }

        currentRoomIndex = (currentRoomIndex + 1) % dungeonGenerator.allRooms.Count;
        RoomNode selectedRoom = dungeonGenerator.allRooms[currentRoomIndex];

        Debug.Log($"[EdgeHighlightTester] Sala {currentRoomIndex}: {selectedRoom.logicalPosition} " +
                  $"({selectedRoom.roomType}) - {selectedRoom.connections.Count} conexões");

        dungeonGenerator.HighlightPlayerAccessibleEdges(selectedRoom);
    }

    /// <summary>
    /// Reseta todas as arestas
    /// </summary>
    void ResetHighlights()
    {
        Debug.Log("[EdgeHighlightTester] Resetando todas as arestas para cor padrão");
        dungeonGenerator.ResetEdgeHighlights();
    }

    void OnGUI()
    {
        if (dungeonGenerator == null || dungeonGenerator.allRooms == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Box("=== EDGE HIGHLIGHT TESTER ===");
        
        GUILayout.Label($"Sala atual: {currentRoomIndex} / {dungeonGenerator.allRooms.Count - 1}");
        
        if (currentRoomIndex < dungeonGenerator.allRooms.Count)
        {
            RoomNode room = dungeonGenerator.allRooms[currentRoomIndex];
            GUILayout.Label($"Tipo: {room.roomType}");
            GUILayout.Label($"Posição: {room.logicalPosition}");
            GUILayout.Label($"Conexões: {room.connections.Count}");
        }

        GUILayout.Space(10);
        GUILayout.Label("Controles:");
        GUILayout.Label("ESPAÇO - Mudar de sala");
        GUILayout.Label("T - Testar sala Spawn");
        GUILayout.Label("R - Resetar arestas");
        
        GUILayout.EndArea();
    }
}
