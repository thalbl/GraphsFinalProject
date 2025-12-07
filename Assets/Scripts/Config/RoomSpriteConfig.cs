using UnityEngine;

/// <summary>
/// ScriptableObject que armazena os sprites para cada tipo de sala.
/// Crie uma instância em: Assets > Create > Dungeon > Room Sprite Config
/// </summary>
[CreateAssetMenu(fileName = "RoomSpriteConfig", menuName = "Dungeon/Room Sprite Config", order = 2)]
public class RoomSpriteConfig : ScriptableObject
{
    [Header("Room Background Sprites")]
    [Tooltip("Sprite para a sala de Spawn (ponto inicial)")]
    public Sprite spawnRoomSprite;
    
    [Tooltip("Sprite para a sala Boss (confronto final)")]
    public Sprite bossRoomSprite;
    
    [Tooltip("Sprite para salas de Combat (batalhas)")]
    public Sprite combatRoomSprite;
    
    [Tooltip("Sprite para salas de Treasure (tesouro)")]
    public Sprite treasureRoomSprite;
    
    [Tooltip("Sprite para salas de Camp (descanso)")]
    public Sprite campRoomSprite;
    
    [Tooltip("Sprite para salas de Event (eventos especiais)")]
    public Sprite eventRoomSprite;

    [Header("Fallback")]
    [Tooltip("Sprite padrão caso algum sprite específico não seja atribuído")]
    public Sprite defaultRoomSprite;

    [Header("Border Animation (Hover/Selection)")]
    [Tooltip("Primeiro sprite da animação de borda")]
    public Sprite borderSprite1;
    
    [Tooltip("Segundo sprite da animação de borda")]
    public Sprite borderSprite2;
    
    [Tooltip("Velocidade da animação da borda (frames por segundo)")]
    [Range(1f, 30f)]
    public float borderAnimationSpeed = 4f;
    
    [Tooltip("Quanto maior a borda é em relação à sala (1.1 = 10% maior)")]
    [Range(1.0f, 2.0f)]
    public float borderScaleMultiplier = 1.15f;

    [Header("GPS Border Sprites (Opcional)")]
    [Tooltip("Sprite de borda para caminho GPS (frame 1). Se vazio, usa borderSprite1")]
    public Sprite gpsBorderSprite1;
    
    [Tooltip("Sprite de borda para caminho GPS (frame 2). Se vazio, usa borderSprite2")]
    public Sprite gpsBorderSprite2;

    /// <summary>
    /// Retorna o sprite apropriado baseado no tipo de sala
    /// </summary>
    public Sprite GetSpriteForRoomType(RoomType roomType)
    {
        Sprite sprite = null;

        switch (roomType)
        {
            case RoomType.Spawn:
                sprite = spawnRoomSprite;
                break;
            case RoomType.Boss:
                sprite = bossRoomSprite;
                break;
            case RoomType.Combat:
                sprite = combatRoomSprite;
                break;
            case RoomType.Treasure:
                sprite = treasureRoomSprite;
                break;
            case RoomType.Camp:
                sprite = campRoomSprite;
                break;
            case RoomType.Event:
                sprite = eventRoomSprite;
                break;
            default:
                Debug.LogWarning($"Tipo de sala desconhecido: {roomType}");
                break;
        }

        // Se o sprite específico não foi atribuído, usa o sprite padrão
        if (sprite == null)
        {
            sprite = defaultRoomSprite;
            if (sprite == null)
            {
                Debug.LogWarning($"Nenhum sprite configurado para {roomType} e nenhum sprite padrão definido!");
            }
        }

        return sprite;
    }

    /// <summary>
    /// Verifica se todos os sprites foram atribuídos
    /// </summary>
    public bool AreAllSpritesAssigned()
    {
        return spawnRoomSprite != null &&
               bossRoomSprite != null &&
               combatRoomSprite != null &&
               treasureRoomSprite != null &&
               campRoomSprite != null &&
               eventRoomSprite != null;
    }

    /// <summary>
    /// Retorna uma lista de tipos de sala que não têm sprite atribuído
    /// </summary>
    public string GetMissingSprites()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        if (spawnRoomSprite == null) sb.AppendLine("- Spawn Room");
        if (bossRoomSprite == null) sb.AppendLine("- Boss Room");
        if (combatRoomSprite == null) sb.AppendLine("- Combat Room");
        if (treasureRoomSprite == null) sb.AppendLine("- Treasure Room");
        if (campRoomSprite == null) sb.AppendLine("- Camp Room");
        if (eventRoomSprite == null) sb.AppendLine("- Event Room");

        return sb.Length > 0 ? sb.ToString() : "Todos os sprites foram atribuídos!";
    }
}
