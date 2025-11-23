using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// Botão de opção de custo com animação idle estilo Persona (bobbing).
/// </summary>
public class CostOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Configuração")]
    [SerializeField] private CostType costType;
    
    [Header("Referências Visuais")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image glowImage; // Glow atrás do botão (opcional)
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costValueText;
    [SerializeField] private RectTransform bobbingContainer; // Container que faz bobbing

    [Header("Animação Idle (Bobbing)")]
    [SerializeField] private float bobbingSpeed = 1f;
    [SerializeField] private float bobbingAmount = 5f;
    [SerializeField] private float phaseOffset = 0f; // Offset de fase para dessincronizar

    [Header("Animação Hover")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Cores")]
    [ColorUsage(true, true)]
    [SerializeField] private Color idleColor = Color.white;
    
    [ColorUsage(true, true)]
    [SerializeField] private Color hoverColor = Color.white * 1.5f;
    
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = Color.white; // Cor do glow no hover

    // Estado
    private bool isHovered = false;
    private bool bobbingEnabled = false;
    private Vector3 basePosition;
    private Vector3 targetScale = Vector3.one;
    private float currentCostValue = 0f;
    private float targetGlowAlpha = 0f; // Alpha alvo do glow

    // Eventos
    public event Action OnButtonClicked;

    void Start()
    {
        // Salva posição base do container de bobbing
        if (bobbingContainer != null)
        {
            basePosition = bobbingContainer.anchoredPosition;
        }
        else
        {
            // Se não há container específico, usa o próprio
            bobbingContainer = GetComponent<RectTransform>();
            basePosition = bobbingContainer.anchoredPosition;
        }

        // Configura cor inicial
        UpdateColors(idleColor, Color.white);

        // Inicializa glow como transparente
        if (glowImage != null)
        {
            Color glowInitial = glowColor;
            glowInitial.a = 0f;
            glowImage.color = glowInitial;
        }

        // Configura textos baseado no tipo de custo
        SetupCostTypeVisuals();
    }

    void Update()
    {
        // Animação de bobbing (se ativada)
        if (bobbingEnabled && !isHovered)
        {
            UpdateBobbingAnimation();
        }

        // Animação de escala (hover)
        UpdateScaleAnimation();
        
        // Animação de glow
        UpdateGlowAnimation();
    }

    /// <summary>
    /// Atualiza a animação de bobbing (movimento vertical senoidal).
    /// </summary>
    private void UpdateBobbingAnimation()
    {
        float bobOffset = Mathf.Sin(Time.time * bobbingSpeed + phaseOffset) * bobbingAmount;
        bobbingContainer.anchoredPosition = basePosition + new Vector3(0, bobOffset, 0);
    }

    /// <summary>
    /// Atualiza a animação de escala suave.
    /// </summary>
    private void UpdateScaleAnimation()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            animationSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Atualiza a animação do glow (fade in/out).
    /// </summary>
    private void UpdateGlowAnimation()
    {
        if (glowImage == null) return;

        Color currentGlow = glowImage.color;
        float newAlpha = Mathf.Lerp(currentGlow.a, targetGlowAlpha, animationSpeed * Time.deltaTime);
        
        glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, newAlpha);
    }

    /// <summary>
    /// Callback quando o mouse entra no botão.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        targetScale = Vector3.one * hoverScale;
        targetGlowAlpha = 1f; // Ativa glow
        
        // Para o bobbing na posição base
        if (bobbingContainer != null)
        {
            bobbingContainer.anchoredPosition = basePosition;
        }

        // Muda para cor de hover com texto preto
        UpdateColors(hoverColor, Color.black);
    }

    /// <summary>
    /// Callback quando o mouse sai do botão.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = Vector3.one;
        targetGlowAlpha = 0f; // Desativa glow
        
        // Volta para cor idle com texto branco
        UpdateColors(idleColor, Color.white);
    }

    /// <summary>
    /// Callback quando o botão é clicado.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Feedback visual de click (squash & stretch)
        StartCoroutine(ClickFeedback());

        // Notifica listeners
        OnButtonClicked?.Invoke();
    }

    /// <summary>
    /// Efeito visual rápido de click.
    /// </summary>
    private System.Collections.IEnumerator ClickFeedback()
    {
        // Squash rápido
        transform.localScale = Vector3.one * 0.95f;
        yield return new WaitForSeconds(0.1f);
        
        // Volta ao normal (será interpolado pelo Update)
        targetScale = isHovered ? Vector3.one * hoverScale : Vector3.one;
    }

    /// <summary>
    /// Atualiza as cores dos elementos visuais.
    /// </summary>
    private void UpdateColors(Color backgroundColor, Color textColor)
    {
        // Apenas o background muda de cor
        if (backgroundImage != null) backgroundImage.color = backgroundColor;
        
        // Ícone sempre branco
        if (iconImage != null) iconImage.color = Color.white;
        
        // Atualiza cor dos textos
        if (titleText != null) titleText.color = textColor;
        if (descriptionText != null) descriptionText.color = textColor;
        if (costValueText != null) costValueText.color = textColor;
    }

    /// <summary>
    /// Configura visuais baseado no tipo de custo.
    /// </summary>
    private void SetupCostTypeVisuals()
    {
        // Background sempre começa preto no idle
        Color baseIdleColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Cinza escuro/preto
        
        switch (costType)
        {
            case CostType.Health:
                if (titleText != null) titleText.text = "SAÚDE";
                if (descriptionText != null) descriptionText.text = "Menor dano físico";
                idleColor = baseIdleColor;
                hoverColor = new Color(0f, 0.8f, 0.42f, 1f); // Verde mais suave no hover
                glowColor = new Color(0f, 1f, 0.53f, 0.4f); // Glow verde semi-transparente
                phaseOffset = 0f;
                break;

            case CostType.Sanity:
                if (titleText != null) titleText.text = "SANIDADE";
                if (descriptionText != null) descriptionText.text = "Preserva a mente";
                idleColor = baseIdleColor;
                hoverColor = new Color(0.5f, 0.16f, 0.8f, 1f); // Roxo mais suave no hover
                glowColor = new Color(0.6f, 0.2f, 1f, 0.4f); // Glow roxo semi-transparente
                phaseOffset = Mathf.PI * 2f / 3f; // 120°
                break;

            case CostType.Time:
                if (titleText != null) titleText.text = "TEMPO";
                if (descriptionText != null) descriptionText.text = "Caminho mais rápido";
                idleColor = baseIdleColor;
                hoverColor = new Color(0.8f, 0.7f, 0f, 1f); // Amarelo mais suave no hover
                glowColor = new Color(1f, 0.87f, 0f, 0.4f); // Glow amarelo semi-transparente
                phaseOffset = Mathf.PI * 4f / 3f; // 240°
                break;
        }

        // Aplica cor idle (preto) ao background
        if (backgroundImage != null)
        {
            backgroundImage.color = idleColor;
        }
        
        // Mantém ícone branco sempre
        if (iconImage != null)
        {
            iconImage.color = Color.white;
        }
    }

    /// <summary>
    /// Define o valor de custo mostrado no botão.
    /// </summary>
    public void SetCostValue(float cost)
    {
        currentCostValue = cost;
        
        if (costValueText != null)
        {
            costValueText.text = cost.ToString("F1");
        }
    }

    /// <summary>
    /// Ativa ou desativa a animação de bobbing.
    /// </summary>
    public void SetBobbingEnabled(bool enabled)
    {
        bobbingEnabled = enabled;
        
        // Se desabilitar, volta pra posição base
        if (!enabled && bobbingContainer != null)
        {
            bobbingContainer.anchoredPosition = basePosition;
        }
    }

    // Propriedades públicas
    public CostType CostType => costType;
    public float CurrentCostValue => currentCostValue;
}
