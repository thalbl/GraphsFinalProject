using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller para uma entrada individual do log com animações Persona 5-inspired.
/// Atribuído ao prefab de mensagem.
/// </summary>
public class LogEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image borderImage; // Borda esquerda colorida
    [SerializeField] private Image backgroundImage; // Fundo gradiente opcional

    [Header("Design Settings")]
    [SerializeField] private float borderWidth = 3f;
    [SerializeField] private float textPaddingLeft = 10f;

    [Header("Animation")]
    [SerializeField] private bool useFadeIn = true;
    [SerializeField] private bool useSlideIn = true;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float slideInDistance = 10f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Auto-adiciona CanvasGroup para animação
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null && (useFadeIn || useSlideIn))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Define a mensagem e o tipo (cor).
    /// </summary>
    public void SetMessage(string message, LogMessageType type)
    {
        // Configurar texto
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = GameLog.GetColorForType(type);
            
            // Aplicar estilos especiais por tipo
            ApplyTextStyle(type);
            
            // Ajustar padding baseado na borda
            if (borderImage != null)
            {
                messageText.margin = new Vector4(textPaddingLeft + borderWidth, 0, textPaddingLeft, 0);
            }
        }

        // Configurar borda
        if (borderImage != null)
        {
            borderImage.color = GameLog.GetBorderColorForType(type);
            
            // Ajustar largura da borda
            RectTransform borderRect = borderImage.GetComponent<RectTransform>();
            if (borderRect != null)
            {
                borderRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, borderWidth);
            }
        }

        // Configurar background gradiente (opcional)
        if (backgroundImage != null)
        {
            ApplyBackgroundStyle(type);
        }

        // Guardar posição original para animação
        originalPosition = rectTransform.anchoredPosition;

        // Iniciar animações
        if (useFadeIn || useSlideIn)
        {
            StartCoroutine(AnimateEntry());
        }
    }

    /// <summary>
    /// Aplica estilo de texto baseado no tipo de mensagem.
    /// </summary>
    private void ApplyTextStyle(LogMessageType type)
    {
        if (messageText == null) return;

        switch (type)
        {
            case LogMessageType.Info:
                messageText.fontStyle = FontStyles.Bold;
                break;
            case LogMessageType.Sanity:
                messageText.fontStyle = FontStyles.Italic;
                break;
            default:
                messageText.fontStyle = FontStyles.Normal;
                break;
        }
    }

    /// <summary>
    /// Aplica estilo de fundo baseado no tipo de mensagem.
    /// </summary>
    private void ApplyBackgroundStyle(LogMessageType type)
    {
        if (backgroundImage == null) return;

        // Mensagens de perigo têm background vermelho translúcido
        if (type == LogMessageType.Danger)
        {
            backgroundImage.color = new Color(0.83f, 0.1f, 0.13f, 0.1f); // #D31920 com alpha baixo
        }
        else
        {
            backgroundImage.color = Color.clear;
        }
    }

    /// <summary>
    /// Animação de entrada da mensagem.
    /// </summary>
    private System.Collections.IEnumerator AnimateEntry()
    {
        // Estado inicial
        if (useFadeIn && canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        if (useSlideIn)
            rectTransform.anchoredPosition = originalPosition + new Vector2(-slideInDistance, 0);

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Funciona mesmo com Time.timeScale = 0
            float progress = elapsed / animationDuration;

            // Fade in
            if (useFadeIn && canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }

            // Slide in com easing
            if (useSlideIn)
            {
                float easedProgress = EaseOutCubic(progress);
                rectTransform.anchoredPosition = Vector2.Lerp(
                    originalPosition + new Vector2(-slideInDistance, 0),
                    originalPosition,
                    easedProgress
                );
            }

            yield return null;
        }

        // Estado final garantido
        if (useFadeIn && canvasGroup != null)
            canvasGroup.alpha = 1f;
        
        if (useSlideIn)
            rectTransform.anchoredPosition = originalPosition;
    }

    /// <summary>
    /// Easing function para animação suave.
    /// </summary>
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    /// <summary>
    /// Configura a opacidade da mensagem (para fade out de mensagens antigas).
    /// </summary>
    public void SetOpacity(float opacity)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = opacity;
        }
        else if (messageText != null)
        {
            Color color = messageText.color;
            color.a = opacity;
            messageText.color = color;
        }
    }
}
