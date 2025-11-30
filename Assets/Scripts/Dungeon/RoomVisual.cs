using UnityEngine;
using TMPro;
using System.Text;

public class RoomVisual : MonoBehaviour {
    public TextMeshPro roomInfoText;
    public SpriteRenderer mainSprite;

    private RoomNode roomData;
    private Color originalColor;
    private DungeonGenerator generator;
    private RoomSpriteConfig spriteConfig;
    
    // Sistema de borda animada
    private GameObject borderObject;
    private SpriteRenderer borderSpriteRenderer;
    private bool isHovered = false;
    private bool isSelected = false;
    private float animationTimer = 0f;
    private int currentBorderFrame = 0;
    

    public void Initialize(RoomNode room) {
        roomData = room;

        mainSprite = GetComponent<SpriteRenderer>();
        originalColor = mainSprite.color;
        generator = FindObjectOfType<DungeonGenerator>();
        
        // --- Carrega a configuração de sprites ---
        LoadSpriteConfig();
        
        // --- Aplica o sprite apropriado baseado no tipo de sala ---
        ApplyRoomSprite(room.roomType);
        
        // --- Cria a borda animada ---
        CreateBorder(room.roomRect);
        
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

    /// <summary>
    /// Carrega a configuração de sprites do Resources
    /// </summary>
    private void LoadSpriteConfig()
    {
        if (spriteConfig == null)
        {
            spriteConfig = Resources.Load<RoomSpriteConfig>("RoomSpriteConfig");
            
            if (spriteConfig == null)
            {
                Debug.LogWarning("[RoomVisual] RoomSpriteConfig não encontrado em Resources! " +
                                "Crie um em: Assets/Resources/RoomSpriteConfig.asset");
            }
        }
    }

    /// <summary>
    /// Aplica o sprite correto baseado no tipo de sala
    /// </summary>
    private void ApplyRoomSprite(RoomType roomType)
    {
        if (spriteConfig == null || mainSprite == null)
            return;

        Sprite roomSprite = spriteConfig.GetSpriteForRoomType(roomType);
        
        if (roomSprite != null)
        {
            mainSprite.sprite = roomSprite;
            // Atualiza a cor original para a cor branca (para não alterar a cor do sprite)
            mainSprite.color = Color.white;
            originalColor = Color.white;
        }
        else
        {
            Debug.LogWarning($"[RoomVisual] Nenhum sprite encontrado para {roomType}");
        }
    }

    /// <summary>
    /// Cria o GameObject da borda animada
    /// </summary>
    private void CreateBorder(Rect roomRect)
    {
        if (spriteConfig == null || (spriteConfig.borderSprite1 == null && spriteConfig.borderSprite2 == null))
        {
            Debug.LogWarning("[RoomVisual] Sprites de borda não configurados no RoomSpriteConfig");
            return;
        }

        // Cria o GameObject da borda como filho desta sala
        borderObject = new GameObject("RoomBorder");
        borderObject.transform.SetParent(transform);
        borderObject.transform.localPosition = Vector3.zero;
        
        // Adiciona o SpriteRenderer
        borderSpriteRenderer = borderObject.AddComponent<SpriteRenderer>();
        borderSpriteRenderer.sprite = spriteConfig.borderSprite1; // Começa com o primeiro sprite
        borderSpriteRenderer.sortingOrder = 1; // Entre a sala (0) e o texto (2)
        
        // Aplica a escala maior que a sala
        float scaleMultiplier = spriteConfig.borderScaleMultiplier;
        borderObject.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1f);
        
        // Começa invisível (será mostrada no hover/seleção)
        borderSpriteRenderer.enabled = false;
    }

    /// <summary>
    /// Atualiza a animação da borda
    /// </summary>
    private void Update()
    {
        // Só anima se a borda estiver visível
        if (borderSpriteRenderer != null && borderSpriteRenderer.enabled && spriteConfig != null)
        {
            UpdateBorderAnimation();
        }
    }

    /// <summary>
    /// Controla a animação alternando entre os 2 sprites da borda
    /// </summary>
    private void UpdateBorderAnimation()
    {
        if (spriteConfig.borderSprite1 == null || spriteConfig.borderSprite2 == null)
            return;

        animationTimer += Time.deltaTime;
        
        // Calcula o intervalo entre frames baseado na velocidade
        float frameInterval = 1f / spriteConfig.borderAnimationSpeed;
        
        if (animationTimer >= frameInterval)
        {
            animationTimer = 0f;
            
            // Alterna entre os dois sprites
            currentBorderFrame = (currentBorderFrame + 1) % 2;
            borderSpriteRenderer.sprite = currentBorderFrame == 0 ? 
                spriteConfig.borderSprite1 : 
                spriteConfig.borderSprite2;
        }
    }

    /// <summary>
    /// Mostra ou esconde a borda
    /// </summary>
    private void UpdateBorderVisibility()
    {
        if (borderSpriteRenderer != null)
        {
            borderSpriteRenderer.enabled = isHovered || isSelected;
            
            // Reseta a animação quando a borda aparece
            if (borderSpriteRenderer.enabled)
            {
                animationTimer = 0f;
                currentBorderFrame = 0;
                if (spriteConfig != null && spriteConfig.borderSprite1 != null)
                {
                    borderSpriteRenderer.sprite = spriteConfig.borderSprite1;
                }
            }
        }
    }

    /// <summary>
    /// Define se esta sala está selecionada
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBorderVisibility();
    }

    // Highlight de mouse-over
    void OnMouseEnter() {
        isHovered = true;
        UpdateBorderVisibility();
        mainSprite.color = Color.cyan; // Feedback visual
    }

    void OnMouseExit() {
        isHovered = false;
        UpdateBorderVisibility();
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