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
        roomInfoText.fontSize = 2;
        roomInfoText.alignment = TextAlignmentOptions.Center;
        roomInfoText.sortingOrder = 2;
        roomInfoText.rectTransform.sizeDelta = new Vector2(room.roomRect.width, room.roomRect.height);
    }

    private void LoadSpriteConfig()
    {
        if (spriteConfig == null)
        {
            spriteConfig = Resources.Load<RoomSpriteConfig>("RoomSpriteConfig");
            
            if (spriteConfig == null)
            {
                Debug.LogWarning("[RoomVisual] RoomSpriteConfig não encontrado em Resources!");
            }
        }
    }

    private void ApplyRoomSprite(RoomType roomType)
    {
        if (spriteConfig == null || mainSprite == null)
            return;

        Sprite roomSprite = spriteConfig.GetSpriteForRoomType(roomType);
        
        if (roomSprite != null)
        {
            mainSprite.sprite = roomSprite;
            mainSprite.color = Color.white;
            originalColor = Color.white;
        }
    }

    private void CreateBorder(Rect roomRect)
    {
        if (spriteConfig == null || (spriteConfig.borderSprite1 == null && spriteConfig.borderSprite2 == null))
        {
            return;
        }

        borderObject = new GameObject("RoomBorder");
        borderObject.transform.SetParent(transform);
        borderObject.transform.localPosition = Vector3.zero;
        
        borderSpriteRenderer = borderObject.AddComponent<SpriteRenderer>();
        borderSpriteRenderer.sprite = spriteConfig.borderSprite1;
        borderSpriteRenderer.sortingOrder = 1;
        
        float scaleMultiplier = spriteConfig.borderScaleMultiplier;
        borderObject.transform.localScale = new Vector3(scaleMultiplier, scaleMultiplier, 1f);
        
        borderSpriteRenderer.enabled = false;
    }

    private void Update()
    {
        if (borderSpriteRenderer != null && borderSpriteRenderer.enabled && spriteConfig != null)
        {
            UpdateBorderAnimation();
        }
    }

    private void UpdateBorderAnimation()
    {
        if (spriteConfig.borderSprite1 == null || spriteConfig.borderSprite2 == null)
            return;

        animationTimer += Time.deltaTime;
        float frameInterval = 1f / spriteConfig.borderAnimationSpeed;
        
        if (animationTimer >= frameInterval)
        {
            animationTimer = 0f;
            currentBorderFrame = (currentBorderFrame + 1) % 2;
            borderSpriteRenderer.sprite = currentBorderFrame == 0 ? 
                spriteConfig.borderSprite1 : 
                spriteConfig.borderSprite2;
        }
    }

    private void UpdateBorderVisibility()
    {
        if (borderSpriteRenderer != null)
        {
            borderSpriteRenderer.enabled = isHovered || isSelected;
            
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

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBorderVisibility();
    }

    // Mouse hover - apenas borda, sem mudança de cor
    void OnMouseEnter() {
        isHovered = true;
        UpdateBorderVisibility();
    }

    void OnMouseExit() {
        isHovered = false;
        UpdateBorderVisibility();
    }

    void OnMouseDown() {
        try {
            GameController gameController = FindObjectOfType<GameController>();
            if (gameController != null)
            {
                gameController.OnRoomClicked(roomData);
                return;
            }

            PathTester pathTester = FindObjectOfType<PathTester>();
            if (pathTester != null)
            {
                pathTester.OnRoomClicked(roomData);
            }

            if (generator == null) {
                Debug.LogWarning("Generator não encontrado!");
                return;
            }

            if (roomData == null) {
                Debug.LogWarning("RoomData é null!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"--- SALA CLICADA ---");
            sb.AppendLine($"Posição Lógica: {roomData.logicalPosition}");
            sb.AppendLine($"Tipo: {roomData.roomType}");
            sb.AppendLine($"Distância do Spawn: {roomData.distanceFromStart}");
            
            if (roomData.connections == null) {
                sb.AppendLine("Conexões: NULL!");
            } else {
                sb.AppendLine($"--- Conexões de Saída ({roomData.connections.Count}) ---");

                foreach (RoomNode neighbor in roomData.connections)
                {
                    if (neighbor == null) {
                        sb.AppendLine("-> Conexão NULL encontrada!");
                        continue;
                    }
                    
                    EdgeData cost = generator.GetEdgeCost(roomData, neighbor);
                    if (cost != null)
                    {
                        sb.AppendLine($"-> Para {neighbor.logicalPosition}:");
                        sb.AppendLine($"    Vida: {cost.costHealth:F1} | San: {cost.costSanity:F1} | Tempo: {cost.costTime:F1}");
                    }
                    else
                    {
                        sb.AppendLine($"-> Para {neighbor.logicalPosition}: SEM CUSTO");
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