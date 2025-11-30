using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Gerencia o menu de seleção de tipo de custo (Health, Sanity, Time).
/// Controla animações de abertura/fechamento e coordena com o GraphViewController.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CostSelectionMenu : MonoBehaviour
{
    [Header("Referências UI")]
    [SerializeField] private RectTransform menuContainer;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CostOptionButton healthButton;
    [SerializeField] private CostOptionButton sanityButton;
    [SerializeField] private CostOptionButton timeButton;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Animação")]
    [SerializeField] private float slideInDuration = 0.4f;
    [SerializeField] private float slideOutDuration = 0.3f;
    [SerializeField] private float slideDistance = 500f; // Distância do slide (pixels)

    [Header("Referências de Sistema")]
    [SerializeField] private GraphViewController graphViewController;

    // Estado
    private bool isMenuOpen = false;
    private RoomNode currentDestination;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    
    // Animação
    private float animationTimer = 0f;
    private bool isAnimating = false;
    private bool isOpening = false;

    // Eventos
    public event Action<CostType> OnCostSelected;

    void Awake()
    {
        // Auto-detecta componentes se não foram setados
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (menuContainer == null)
            menuContainer = GetComponent<RectTransform>();

        // Configura posições inicial e final
        visiblePosition = menuContainer.anchoredPosition;
        hiddenPosition = visiblePosition + new Vector2(slideDistance, 0);

        // Começa escondido
        menuContainer.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        // Conecta eventos dos botões
        if (healthButton != null)
            healthButton.OnButtonClicked += () => SelectCost(CostType.Health);
        
        if (sanityButton != null)
            sanityButton.OnButtonClicked += () => SelectCost(CostType.Sanity);
        
        if (timeButton != null)
            timeButton.OnButtonClicked += () => SelectCost(CostType.Time);

        // Busca GraphViewController se não foi setado
        if (graphViewController == null)
            graphViewController = FindObjectOfType<GraphViewController>();
    }

    void Update()
    {
        // Atualiza animação se está animando
        if (isAnimating)
        {
            UpdateAnimation();
        }
    }

    /// <summary>
    /// Mostra o menu para selecionar custo para uma sala destino.
    /// </summary>
    public void ShowMenu(RoomNode destination, RoomNode startRoom, DungeonGraph graph)
    {
        if (isMenuOpen || isAnimating) return;

        currentDestination = destination;
        isMenuOpen = true;

        // Calcula custos para cada tipo
        if (graph != null && startRoom != null)
        {
            UpdateCostValues(startRoom, destination, graph);
        }

        // Atualiza título
        if (titleText != null)
        {
            titleText.text = $"CAMINHO PARA {destination.logicalPosition}";
        }

        // Inicia animação de abertura
        StartAnimation(true);

        // Notifica GraphViewController para fazer zoom na sala destino
        if (graphViewController != null)
        {
            graphViewController.TransitionToMenuMode(destination);
        }

        // Ativa bobbing dos botões
        SetButtonsBobbingEnabled(true);
    }

    /// <summary>
    /// Esconde o menu.
    /// </summary>
    public void HideMenu()
    {
        if (!isMenuOpen || isAnimating) return;

        isMenuOpen = false;

        // Inicia animação de fechamento
        StartAnimation(false);

        // Notifica GraphViewController para restaurar o grafo
        if (graphViewController != null)
        {
            graphViewController.TransitionToNormalMode();
        }

        // Desativa bobbing dos botões
        SetButtonsBobbingEnabled(false);
    }

    /// <summary>
    /// Chamado quando um custo é selecionado.
    /// </summary>
    private void SelectCost(CostType costType)
    {
        Debug.Log($"Custo selecionado: {costType}");

        // Notifica listeners
        OnCostSelected?.Invoke(costType);

        // Fecha o menu
        HideMenu();
    }

    /// <summary>
    /// Inicia a animação de slide in/out.
    /// </summary>
    private void StartAnimation(bool opening)
    {
        isAnimating = true;
        isOpening = opening;
        animationTimer = 0f;

        // Se estiver abrindo, torna interativo imediatamente após a animação
        if (opening)
        {
            canvasGroup.blocksRaycasts = false; // Só ativa após animação completa
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Atualiza a animação frame a frame.
    /// </summary>
    private void UpdateAnimation()
    {
        float duration = isOpening ? slideInDuration : slideOutDuration;
        animationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTimer / duration);

        // Ease-out para suavidade
        float easedProgress = EaseOutCubic(progress);

        if (isOpening)
        {
            // Slide da direita para esquerda + fade in
            menuContainer.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, easedProgress);
            canvasGroup.alpha = easedProgress;
        }
        else
        {
            // Slide da esquerda para direita + fade out
            menuContainer.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, easedProgress);
            canvasGroup.alpha = 1f - easedProgress;
        }

        // Verifica se animação terminou
        if (progress >= 1f)
        {
            isAnimating = false;
            
            if (isOpening)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    /// <summary>
    /// Atualiza os valores de custo mostrados nos botões.
    /// </summary>
    private void UpdateCostValues(RoomNode start, RoomNode destination, DungeonGraph graph)
    {
        // Calcula path para cada tipo de custo e pega o custo total
        var healthPath = AStarPathfinder.FindPath(graph, start, destination, CostType.Health);
        float healthCost = AStarPathfinder.CalculatePathCost(graph, healthPath, CostType.Health);

        var sanityPath = AStarPathfinder.FindPath(graph, start, destination, CostType.Sanity);
        float sanityCost = AStarPathfinder.CalculatePathCost(graph, sanityPath, CostType.Sanity);

        var timePath = AStarPathfinder.FindPath(graph, start, destination, CostType.Time);
        float timeCost = AStarPathfinder.CalculatePathCost(graph, timePath, CostType.Time);

        // Atualiza os botões
        if (healthButton != null) healthButton.SetCostValue(healthCost);
        if (sanityButton != null) sanityButton.SetCostValue(sanityCost);
        if (timeButton != null) timeButton.SetCostValue(timeCost);
    }

    /// <summary>
    /// Ativa ou desativa a animação de bobbing nos botões.
    /// </summary>
    private void SetButtonsBobbingEnabled(bool enabled)
    {
        if (healthButton != null) healthButton.SetBobbingEnabled(enabled);
        if (sanityButton != null) sanityButton.SetBobbingEnabled(enabled);
        if (timeButton != null) timeButton.SetBobbingEnabled(enabled);
    }

    /// <summary>
    /// Função de easing cubic para animação suave.
    /// </summary>
    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    // Métodos públicos para verificação de estado
    public bool IsMenuOpen => isMenuOpen;
    public bool IsAnimating => isAnimating;
    public RoomNode CurrentDestination => currentDestination;
}
