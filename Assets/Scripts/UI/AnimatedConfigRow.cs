using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Anima as Config Rows do Debug Menu com hover effects.
/// Esquema de cores: Branco → Cinza claro + borda vermelha.
/// </summary>
public class AnimatedConfigRow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image leftBorderImage; // Opcional: borda lateral esquerda

    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.02f;
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private float slideAmount = -10f; // Pixels para deslizar no hover

    [Header("Colors")]
    [ColorUsage(false, false)]
    [SerializeField] private Color idleBackgroundColor = Color.white;
    
    [ColorUsage(false, false)]
    [SerializeField] private Color hoverBackgroundColor = new Color(0.94f, 0.94f, 0.94f, 1f); // #F0F0F0
    
    [ColorUsage(false, false)]
    [SerializeField] private Color borderColor = new Color(0.82f, 0.1f, 0.13f, 1f); // #D31920

    // Estado
    private bool isHovered = false;
    private Vector3 targetScale = Vector3.one;
    private Vector3 basePosition;
    private RectTransform rectTransform;
    private Vector3 targetPosition;
    private float targetBorderAlpha = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Auto-detecta background se não foi setado
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    void Start()
    {
        // Salva posição base
        basePosition = rectTransform.anchoredPosition;
        targetPosition = basePosition;

        // Configura cor inicial
        if (backgroundImage != null)
        {
            backgroundImage.color = idleBackgroundColor;
        }

        // Inicializa borda como transparente
        if (leftBorderImage != null)
        {
            Color borderInitial = borderColor;
            borderInitial.a = 0f;
            leftBorderImage.color = borderInitial;
        }
    }

    void Update()
    {
        // Animação de escala suave
        UpdateScaleAnimation();
        
        // Animação de posição (slide)
        UpdatePositionAnimation();
        
        // Animação da borda
        UpdateBorderAnimation();
    }

    /// <summary>
    /// Atualiza a animação de escala suave.
    /// </summary>
    private void UpdateScaleAnimation()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            animationSpeed * Time.unscaledDeltaTime
        );
    }

    /// <summary>
    /// Atualiza a animação de posição (slide no hover).
    /// </summary>
    private void UpdatePositionAnimation()
    {
        rectTransform.anchoredPosition = Vector3.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            animationSpeed * Time.unscaledDeltaTime
        );
    }

    /// <summary>
    /// Atualiza a animação da borda lateral.
    /// </summary>
    private void UpdateBorderAnimation()
    {
        if (leftBorderImage == null) return;

        Color currentBorder = leftBorderImage.color;
        float newAlpha = Mathf.Lerp(currentBorder.a, targetBorderAlpha, animationSpeed * Time.unscaledDeltaTime);
        
        leftBorderImage.color = new Color(borderColor.r, borderColor.g, borderColor.b, newAlpha);
    }

    /// <summary>
    /// Callback quando o mouse entra na row.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = Vector3.one * hoverScale;
        targetPosition = basePosition + new Vector3(slideAmount, 0, 0);
        targetBorderAlpha = 1f; // Mostra borda

        // Muda para cor de hover
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverBackgroundColor;
        }
    }

    /// <summary>
    /// Callback quando o mouse sai da row.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = Vector3.one;
        targetPosition = basePosition;
        targetBorderAlpha = 0f; // Esconde borda

        // Volta para cor idle
        if (backgroundImage != null)
        {
            backgroundImage.color = idleBackgroundColor;
        }
    }
}
