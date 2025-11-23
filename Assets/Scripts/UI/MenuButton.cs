using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Componente de botão do menu principal com efeitos visuais estilo "Ordem Paranormal".
/// Implementa animações de hover, rotação 3D e mudanças de cor com glow HDR.
/// </summary>
public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Referências Visuais")]
    [Tooltip("Container que será animado (rotação, escala, posição)")]
    public RectTransform visualContainer;
    
    [Tooltip("Image da borda que terá o glow vermelho")]
    public Image borderImage;
    
    [Tooltip("Texto do título principal")]
    public TextMeshProUGUI titleText;
    
    [Tooltip("Texto da descrição (opcional)")]
    public TextMeshProUGUI descText;
    
    [Tooltip("Numeração romana decorativa (opcional)")]
    public TextMeshProUGUI romanNumeralText;

    [Header("Configurações de Animação")]
    [Tooltip("Rotação inicial no eixo Y (graus)")]
    public float rotationIdle = -5f;
    
    [Tooltip("Rotação ao passar o mouse no eixo Y (graus)")]
    public float rotationHover = 0f;
    
    [Tooltip("Movimento horizontal no hover (pixels)")]
    public float moveXHover = -20f;
    
    [Tooltip("Escala no hover (multiplicador)")]
    public float scaleHover = 1.05f;
    
    [Tooltip("Velocidade da animação (maior = mais rápido)")]
    public float animationSpeed = 15f;

    [Header("Cores HDR (Glow)")]
    [ColorUsage(true, true)] 
    [Tooltip("Vermelho neon brilhante para hover (ajuste HDR intensity para glow)")]
    public Color colorRedNeon = new Color(1f, 0.1f, 0.1f, 1f) * 2f;
    
    [ColorUsage(true, true)]
    [Tooltip("Vermelho escuro para estado idle")]
    public Color colorDarkRed = new Color(0.35f, 0f, 0f, 1f);
    
    public Color colorWhite = Color.white;
    public Color colorBlack = Color.black;
    public Color colorGray = new Color(0.7f, 0.7f, 0.7f, 1f);

    [Header("Menu Action")]
    [Tooltip("Ação a ser executada (Novo Jogo, Carregar, Config, Exit)")]
    public string menuAction = "NoAction";

    // Estado interno
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetScale;
    private Vector3 originalPos;
    private MenuManager menuManager;

    void Start()
    {
        // Auto-detecta visualContainer se não foi setado
        if (visualContainer == null)
        {
            visualContainer = transform.Find("Visuals")?.GetComponent<RectTransform>();
            if (visualContainer == null)
                visualContainer = GetComponent<RectTransform>();
        }
        
        originalPos = visualContainer.anchoredPosition;
        
        // Busca o MenuManager na cena
        menuManager = FindObjectOfType<MenuManager>();
        
        // Define estado inicial
        ResetState();
    }

    void Update()
    {
        // Animação suave tipo CSS transition com Lerp
        float speed = animationSpeed * Time.deltaTime;

        visualContainer.anchoredPosition = Vector3.Lerp(
            visualContainer.anchoredPosition, 
            targetPosition, 
            speed
        );
        
        visualContainer.localRotation = Quaternion.Lerp(
            visualContainer.localRotation, 
            targetRotation, 
            speed
        );
        
        visualContainer.localScale = Vector3.Lerp(
            visualContainer.localScale, 
            targetScale, 
            speed
        );
    }

    /// <summary>
    /// Callback quando o mouse entra no botão (equivalente a :hover)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Define alvos da animação
        targetPosition = originalPos + new Vector3(moveXHover, 0, 0);
        targetRotation = Quaternion.Euler(0, rotationHover, 0);
        targetScale = Vector3.one * scaleHover;

        // Ativa cores de hover (glow)
        if (borderImage) borderImage.color = colorRedNeon;
        if (titleText) titleText.color = colorWhite;
        if (descText) descText.color = colorWhite;
        if (romanNumeralText) romanNumeralText.color = colorWhite;
    }

    /// <summary>
    /// Callback quando o mouse sai do botão
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        ResetState();
    }

    /// <summary>
    /// Callback quando o botão é clicado
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Flash branco no clique
        if (titleText) titleText.color = colorBlack;
        if (borderImage) borderImage.color = colorWhite;
        
        // Executa a ação do menu
        if (menuManager != null)
        {
            menuManager.ExecuteMenuAction(menuAction);
        }
        else
        {
            Debug.LogWarning($"MenuManager não encontrado! Ação: {menuAction}");
        }
    }

    /// <summary>
    /// Reseta o botão para o estado idle (cores e animação)
    /// </summary>
    void ResetState()
    {
        targetPosition = originalPos;
        targetRotation = Quaternion.Euler(0, rotationIdle, 0);
        targetScale = Vector3.one;

        // Restaura cores originais
        if (borderImage) borderImage.color = colorDarkRed;
        if (titleText) titleText.color = colorRedNeon;
        if (descText) descText.color = colorGray;
        if (romanNumeralText) romanNumeralText.color = colorRedNeon * 0.6f; // Mais apagado
    }
}