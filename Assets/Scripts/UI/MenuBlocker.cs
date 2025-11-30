using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Painel que bloqueia raycasts e escurece o fundo quando o menu está aberto.
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class MenuBlocker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.5f); // Preto 50% transparente
    [SerializeField] private float fadeSpeed = 8f;

    private Image image;
    private CanvasGroup canvasGroup;
    private bool isActive = false;
    private float currentAlpha = 0f;
    private float targetAlpha = 0f;

    void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Configura imagem
        image.color = overlayColor;
        
        // Começa invisível
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        currentAlpha = 0f;
    }

    void Update()
    {
        // Fade suave usando MoveTowards para garantir que alcançamos o valor alvo
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
        canvasGroup.alpha = currentAlpha;

        // Desativa o GameObject quando o fade out estiver completo
        if (!isActive && currentAlpha < 0.01f)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Ativa o blocker (escurece fundo e bloqueia raycasts).
    /// </summary>
    public void Show()
    {
        isActive = true;
        targetAlpha = overlayColor.a;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Desativa o blocker.
    /// </summary>
    public void Hide()
    {
        isActive = false;
        targetAlpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        // Não precisa mais do Invoke, o Update vai cuidar de desativar
    }
}