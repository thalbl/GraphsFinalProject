using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Botão animado com hover effects (scale, glow, color).
/// Baseado no CostOptionButton mas genérico para qualquer botão.
/// </summary>
public class AnimatedButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image glowImage; // Opcional: glow atrás do botão
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private float slideAmount = -15f; // Pixels para deslizar no hover

    [Header("Colors")]
    [ColorUsage(false, false)]
    [SerializeField] private Color idleBackgroundColor = Color.black;
    
    [ColorUsage(false, false)]
    [SerializeField] private Color hoverBackgroundColor = new Color(0.82f, 0.1f, 0.13f, 1f); // #D31920
    
    [ColorUsage(false, false)]
    [SerializeField] private Color idleTextColor = Color.white;
    
    [ColorUsage(false, false)]
    [SerializeField] private Color hoverTextColor = Color.black;
    
    [ColorUsage(true, true)] // HDR para glow
    [SerializeField] private Color glowColor = new Color(0.82f, 0.1f, 0.13f, 0.5f);

    [Header("Click Feedback")]
    [SerializeField] private float clickScale = 0.95f;
    [SerializeField] private float clickDuration = 0.1f;

    // Estado
    private bool isHovered = false;
    private Vector3 targetScale = Vector3.one;
    private Vector3 basePosition;
    private RectTransform rectTransform;
    private float targetGlowAlpha = 0f;
    private Vector3 targetPosition;

    // Referência ao Button nativo (opcional, para usar onClick)
    private Button button;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
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
        UpdateColors(idleBackgroundColor, idleTextColor);

        // Inicializa glow como transparente
        if (glowImage != null)
        {
            Color glowInitial = glowColor;
            glowInitial.a = 0f;
            glowImage.color = glowInitial;
        }
    }

    void Update()
    {
        // Animação de escala suave
        UpdateScaleAnimation();
        
        // Animação de glow
        UpdateGlowAnimation();
        
        // Animação de posição (slide)
        UpdatePositionAnimation();
    }

    /// <summary>
    /// Atualiza a animação de escala suave.
    /// </summary>
    private void UpdateScaleAnimation()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            animationSpeed * Time.unscaledDeltaTime // unscaled para funcionar com Time.timeScale = 0
        );
    }

    /// <summary>
    /// Atualiza a animação do glow (fade in/out).
    /// </summary>
    private void UpdateGlowAnimation()
    {
        if (glowImage == null) return;

        Color currentGlow = glowImage.color;
        float newAlpha = Mathf.Lerp(currentGlow.a, targetGlowAlpha, animationSpeed * Time.unscaledDeltaTime);
        
        glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, newAlpha);
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
    /// Callback quando o mouse entra no botão.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = Vector3.one * hoverScale;
        targetGlowAlpha = 1f; // Ativa glow
        targetPosition = basePosition + new Vector3(slideAmount, 0, 0); // Slide para esquerda

        // Muda para cor de hover
        UpdateColors(hoverBackgroundColor, hoverTextColor);
    }

    /// <summary>
    /// Callback quando o mouse sai do botão.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = Vector3.one;
        targetGlowAlpha = 0f; // Desativa glow
        targetPosition = basePosition; // Volta para posição original

        // Volta para cor idle
        UpdateColors(idleBackgroundColor, idleTextColor);
    }

    /// <summary>
    /// Callback quando o botão é clicado.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se o botão está interactable
        if (button != null && !button.interactable) return;

        // Feedback visual de click
        StartCoroutine(ClickFeedback());

        // O evento onClick do Button será chamado automaticamente
    }

    /// <summary>
    /// Efeito visual rápido de click (squash).
    /// </summary>
    private IEnumerator ClickFeedback()
    {
        // Squash rápido
        transform.localScale = Vector3.one * clickScale;
        yield return new WaitForSecondsRealtime(clickDuration);
        
        // Volta ao normal (será interpolado pelo Update)
        targetScale = isHovered ? Vector3.one * hoverScale : Vector3.one;
    }

    /// <summary>
    /// Atualiza as cores dos elementos visuais.
    /// </summary>
    private void UpdateColors(Color backgroundColor, Color textColor)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }

        if (buttonText != null)
        {
            buttonText.color = textColor;
        }
    }

    /// <summary>
    /// Define cores customizadas para hover (útil para temas diferentes).
    /// </summary>
    public void SetHoverColors(Color background, Color text)
    {
        hoverBackgroundColor = background;
        hoverTextColor = text;
    }

    /// <summary>
    /// Define cor do glow.
    /// </summary>
    public void SetGlowColor(Color color)
    {
        glowColor = color;
    }
}
