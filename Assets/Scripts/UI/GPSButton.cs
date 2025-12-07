using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Componente UI para o botão de GPS.
/// Gerencia interação do jogador com o sistema de pathfinding GPS.
/// Estilo visual compatível com CostOptionButton.
/// </summary>
public class GPSButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Referências")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image glowImage; // Opcional: glow atrás do botão
    [SerializeField] private PathfinderGPS gpsSystem;
    [SerializeField] private PlayerController playerController;

    [Header("Configuração Visual")]
    [SerializeField] private Color idleColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Preto/cinza escuro
    [SerializeField] private Color hoverColor = new Color(0.3f, 0.7f, 1f, 1f); // Azul
    [SerializeField] private Color activeColor = new Color(0.3f, 0.7f, 1f, 1f); // Azul quando GPS ativo
    [SerializeField] private Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Cinza desabilitado
    
    [ColorUsage(true, true)]
    [SerializeField] private Color glowColor = new Color(0.5f, 0.8f, 1f, 0.4f); // Glow azul
    
    [Header("Efeitos de Hover")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;

    private bool isGPSActive = false;
    private bool isHovered = false;
    
    // Animação
    private Vector3 targetScale = Vector3.one;
    private float targetGlowAlpha = 0f;

    private void Awake()
    {
        // Auto-detecta componentes
        if (button == null)
            button = GetComponent<Button>();

        if (buttonText == null)
            buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (gpsSystem == null)
            gpsSystem = FindObjectOfType<PathfinderGPS>();

        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        // Registra evento de click
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void Start()
    {
        // Inicializa cores
        if (backgroundImage != null)
            backgroundImage.color = idleColor;

        // Inicializa glow como transparente
        if (glowImage != null)
        {
            Color glowInitial = glowColor;
            glowInitial.a = 0f;
            glowImage.color = glowInitial;
        }

        UpdateButtonState();
    }

    /// <summary>
    /// Define a referência do PlayerController (chamado pelo GameController após instanciar o player)
    /// </summary>
    public void SetPlayerController(PlayerController player)
    {
        playerController = player;
        UpdateButtonState();
        Debug.Log("[GPSButton] PlayerController configurado");
    }

    private void Update()
    {
        // Atualiza estado do botão a cada frame
        UpdateButtonState();
        
        // Animação de escala suave
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            animationSpeed * Time.deltaTime
        );
        
        // Animação de glow
        UpdateGlowAnimation();
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
    /// Callback quando o mouse entra no botão
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button == null || !button.interactable) return;

        isHovered = true;
        targetScale = Vector3.one * hoverScale;
        targetGlowAlpha = 1f; // Ativa glow

        // Muda para cor de hover com texto preto
        if (backgroundImage != null)
            backgroundImage.color = hoverColor;

        if (buttonText != null)
            buttonText.color = Color.black;
    }

    /// <summary>
    /// Callback quando o mouse sai do botão
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = Vector3.one;
        targetGlowAlpha = 0f; // Desativa glow

        // Volta para cor idle
        UpdateButtonColors();
    }

    /// <summary>
    /// Chamado quando o botão é clicado
    /// </summary>
    private void OnButtonClicked()
    {
        if (gpsSystem == null)
        {
            Debug.LogError("[GPSButton] PathfinderGPS não encontrado!");
            return;
        }

        // Feedback visual de click (squash & stretch)
        StartCoroutine(ClickFeedback());

        if (isGPSActive)
        {
            // Cancela GPS
            gpsSystem.CancelGPS();
            isGPSActive = false;
        }
        else
        {
            // Ativa GPS
            if (gpsSystem.CanUseGPS)
            {
                gpsSystem.ActivateGPS();
                isGPSActive = true;
            }
            else
            {
                Debug.Log("[GPSButton] Sanidade insuficiente para usar GPS");
            }
        }
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
    /// Atualiza o estado visual do botão
    /// </summary>
    private void UpdateButtonState()
    {
        if (gpsSystem == null || playerController == null || button == null)
            return;

        // Verifica se o GPS está esperando seleção de sala
        bool isWaitingForSelection = gpsSystem.IsWaitingForRoomSelection;
        
        // Atualiza se GPS está ativo
        isGPSActive = isWaitingForSelection;

        // Verifica se pode usar GPS
        bool canUse = gpsSystem.CanUseGPS;
        float sanityCost = gpsSystem.SanityCost;

        // Atualiza interatividade
        button.interactable = canUse || isGPSActive;

        // Atualiza texto
        if (buttonText != null)
        {
            if (isGPSActive)
            {
                buttonText.text = "Cancelar GPS";
            }
            else
            {
                buttonText.text = $"Encontrar Caminho ({sanityCost:F0} SP)";
            }
        }

        // Atualiza cores (se não estiver em hover)
        if (!isHovered)
        {
            UpdateButtonColors();
        }
    }

    /// <summary>
    /// Atualiza as cores do botão baseado no estado
    /// </summary>
    private void UpdateButtonColors()
    {
        if (backgroundImage == null || buttonText == null) return;

        bool canUse = gpsSystem != null && gpsSystem.CanUseGPS;

        if (isGPSActive)
        {
            // GPS ativo: azul
            backgroundImage.color = activeColor;
            buttonText.color = Color.white;
        }
        else if (!canUse)
        {
            // Desabilitado: cinza
            backgroundImage.color = disabledColor;
            buttonText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
        }
        else
        {
            // Normal: preto/cinza escuro
            backgroundImage.color = idleColor;
            buttonText.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
