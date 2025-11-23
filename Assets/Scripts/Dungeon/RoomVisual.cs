using UnityEngine;
using TMPro;
using System.Text;

public class RoomVisual : MonoBehaviour {
    public TextMeshPro roomInfoText;
    public SpriteRenderer mainSprite;

    private RoomNode roomData;
    private Color originalColor;
    private DungeonGenerator generator;
    

    public void Initialize(RoomNode room) {
        roomData = room;

        mainSprite = GetComponent<SpriteRenderer>();
        originalColor = mainSprite.color;
        generator = FindObjectOfType<DungeonGenerator>();
        // --- Configura o Texto (TextMeshPro) ---
        GameObject textGO = new GameObject("RoomInfo");
        textGO.transform.SetParent(transform);
        textGO.transform.localPosition = Vector3.zero;

        roomInfoText = textGO.AddComponent<TextMeshPro>();
        roomInfoText.text = $"{room.roomType}\n(Dist: {room.distanceFromStart})";
        roomInfoText.fontSize = 2; // Ajuste o tamanho conforme necessário
        roomInfoText.alignment = TextAlignmentOptions.Center;
        roomInfoText.sortingOrder = 2; // Na frente da sala
        roomInfoText.rectTransform.sizeDelta = new Vector2(room.roomRect.width, room.roomRect.height);
    }

    // Highlight de mouse-over
    void OnMouseEnter() {
        mainSprite.color = Color.cyan; // Feedback visual
    }

    void OnMouseExit() {
        mainSprite.color = originalColor; // Retorna à cor original
    }

    // Mostra info no click
    void OnMouseDown() {
        try {
            // ========================================
            // NOTIFICA O GAME CONTROLLER (Sistema de Produção)
            // ========================================
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.OnRoomClicked(roomData);
                // Se GameController está ativo, não executa PathTester
                return;
            }

            // ========================================
            // NOTIFICA O PATH TESTER (Sistema de Debug de Pathfinding)
            // Só executa se GameController não estiver presente
            // ========================================
            PathTester pathTester = FindObjectOfType<PathTester>();
            if (pathTester != null)
            {
                pathTester.OnRoomClicked(roomData);
            }

            // ========================================
            // FUNCIONALIDADE DE DEBUG ORIGINAL (Edge Costs)
            // ========================================
            if (generator == null) {
                Debug.LogWarning("Generator não encontrado!");
                return;
            }

            if (roomData == null) {
                Debug.LogWarning("RoomData é null!");
                return;
            }

            // Constrói uma string de log
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- SALA CLICADA ---");
            sb.AppendLine($"Posição Lógica: {roomData.logicalPosition}");
            sb.AppendLine($"Tipo: {roomData.roomType}");
            sb.AppendLine($"Distância do Spawn: {roomData.distanceFromStart}");
            
            if (roomData.connections == null) {
                sb.AppendLine("Conexões: NULL!");
            } else {
                sb.AppendLine($"--- Conexões de Saída ({roomData.connections.Count}) ---");

                // Itera pelas conexões e pede os custos ao gerador
                foreach (RoomNode neighbor in roomData.connections)
                {
                    if (neighbor == null) {
                        sb.AppendLine("-> Conexão NULL encontrada!");
                        continue;
                    }
                    
                    EdgeData cost = generator.GetEdgeCost(roomData, neighbor); // Pede o custo
                    if (cost != null)
                    {
                        sb.AppendLine($"-> Para {neighbor.logicalPosition}:");
                        sb.AppendLine($"    Vida: {cost.costHealth:F1} | San: {cost.costSanity:F1} | Tempo: {cost.costTime:F1}");
                    }
                    else
                    {
                        sb.AppendLine($"-> Para {neighbor.logicalPosition}: SEM CUSTO (aresta não encontrada)");
                    }
                }
            }
            
            Debug.Log(sb.ToString());
        }
        catch (System.Exception e) {
            Debug.LogError($"Erro no OnMouseDown: {e.Message}\n{e.StackTrace}");
        }
    }
}