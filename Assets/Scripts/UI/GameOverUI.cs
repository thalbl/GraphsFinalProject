using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Exibe overlay escuro, texto animado, linha decorativa e botão estilizado.
/// Usa unscaled time para funcionar com Time.timeScale = 0.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private GameObject gameOverPanel; // Panel raiz que contém tudo
    [SerializeField] private CanvasGroup canvasGroup; // Para fade-in
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Image lineSeparator; // Linha decorativa
    [SerializeField] private Button mainMenuButton;

    [Header("Fallback: UI Text")]
    [SerializeField] private Text gameOverTextLegacy;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "Menu_Test"; // Nome da cena do menu principal

    [Header("Animation Settings")]
    [SerializeField] private float overlayFadeDuration = 1.5f; // Duração do fade-in do overlay
    [SerializeField] private float textAnimationDuration = 2f; // Duração da animação do texto
    [SerializeField] private float textInitialScale = 1.2f; // Escala inicial do texto
    [SerializeField] private float textFinalScale = 1f; // Escala final do texto
    [SerializeField] private float lineAnimationDelay = 1f; // Delay antes da linha aparecer
    [SerializeField] private float lineAnimationDuration = 1f; // Duração da animação da linha
    [SerializeField] private float buttonDelay = 2f; // Delay antes do botão aparecer
    [SerializeField] private float buttonFadeDuration = 0.3f; // Duração do fade-in do botão

    private PlayerController playerController;
    private RectTransform lineSeparatorRect;
    private CanvasGroup buttonCanvasGroup;

    void Start()
    {
        // Esconde o painel por padrão
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Auto-detecta CanvasGroup se não atribuído
        if (canvasGroup == null && gameOverPanel != null)
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();

        // Configura componentes de animação
        if (lineSeparator != null)
        {
            lineSeparatorRect = lineSeparator.GetComponent<RectTransform>();
        }

        // Adiciona CanvasGroup ao botão para animação
        if (mainMenuButton != null)
        {
            buttonCanvasGroup = mainMenuButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null)
                buttonCanvasGroup = mainMenuButton.gameObject.AddComponent<CanvasGroup>();

            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        // Encontra o player e se inscreve no evento de morte
        FindPlayer();

        // Se não encontrou, tenta novamente após um delay
        if (playerController == null)
        {
            Invoke(nameof(FindPlayer), 0.5f);
        }
    }

    /// <summary>
    /// Busca o PlayerController e se inscreve no evento de morte.
    /// </summary>
    private void FindPlayer()
    {
        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            playerController.stats.OnPlayerDied += ShowGameOver;
            Debug.Log("GameOverUI conectado ao evento de morte do Player!");
        }
        else
        {
            Debug.LogWarning("GameOverUI não encontrou PlayerController na cena!");
        }
    }

    /// <summary>
    /// Mostra a tela de Game Over com animações.
    /// </summary>
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("═══ GAME OVER UI ATIVADA ═══");

            // Inicia animações
            StartCoroutine(PlayGameOverAnimation());
        }
        else
        {
            Debug.LogError("GameOverPanel não está atribuído no GameOverUI!");
        }

        // Atualiza texto
        if (gameOverText != null)
            gameOverText.text = "GAME OVER";
        if (gameOverTextLegacy != null)
            gameOverTextLegacy.text = "FIM DE JOGO";
    }

    /// <summary>
    /// Coroutine que executa todas as animações em sequência.
    /// Usa unscaled time para funcionar com Time.timeScale = 0.
    /// </summary>
    private IEnumerator PlayGameOverAnimation()
    {
        // ═══ SETUP INICIAL ═══
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (gameOverText != null)
        {
            gameOverText.transform.localScale = Vector3.one * textInitialScale;
            Color textColor = gameOverText.color;
            textColor.a = 0f;
            gameOverText.color = textColor;
        }

        if (lineSeparatorRect != null)
        {
            lineSeparatorRect.sizeDelta = new Vector2(0, lineSeparatorRect.sizeDelta.y);
        }

        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 0f;
        }

        // ═══ ANIMAÇÃO 1: FADE-IN DO OVERLAY ═══
        float elapsed = 0f;
        while (elapsed < overlayFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / overlayFadeDuration);
            yield return null;
        }
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // ═══ ANIMAÇÃO 2: TEXTO "GAME OVER" (SCALE + FADE) ═══
        elapsed = 0f;
        while (elapsed < textAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / textAnimationDuration;
            // Ease out cubic
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            if (gameOverText != null)
            {
                gameOverText.transform.localScale = Vector3.one * Mathf.Lerp(textInitialScale, textFinalScale, easedT);
                Color textColor = gameOverText.color;
                textColor.a = easedT;
                gameOverText.color = textColor;
            }
            yield return null;
        }
        if (gameOverText != null)
        {
            gameOverText.transform.localScale = Vector3.one * textFinalScale;
            Color textColor = gameOverText.color;
            textColor.a = 1f;
            gameOverText.color = textColor;
        }

        // ═══ ANIMAÇÃO 3: LINHA DECORATIVA (após delay) ═══
        yield return new WaitForSecondsRealtime(lineAnimationDelay - textAnimationDuration);

        elapsed = 0f;
        float targetWidth = 500f; // Largura alvo da linha
        while (elapsed < lineAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lineAnimationDuration;
            // Ease out
            float easedT = 1f - Mathf.Pow(1f - t, 2f);

            if (lineSeparatorRect != null)
            {
                lineSeparatorRect.sizeDelta = new Vector2(
                    Mathf.Lerp(0, targetWidth, easedT),
                    lineSeparatorRect.sizeDelta.y
                );
            }
            yield return null;
        }
        if (lineSeparatorRect != null)
        {
            lineSeparatorRect.sizeDelta = new Vector2(targetWidth, lineSeparatorRect.sizeDelta.y);
        }

        // ═══ ANIMAÇÃO 4: BOTÃO (após delay total) ═══
        float totalElapsed = overlayFadeDuration + lineAnimationDelay + lineAnimationDuration;
        float buttonWait = buttonDelay - totalElapsed;
        if (buttonWait > 0)
            yield return new WaitForSecondsRealtime(buttonWait);

        elapsed = 0f;
        while (elapsed < buttonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (buttonCanvasGroup != null)
                buttonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / buttonFadeDuration);
            yield return null;
        }
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 1f;
            buttonCanvasGroup.interactable = true; // Torna o botão clicável
        }

        Debug.Log("Animações de Game Over completas!");
    }

    /// <summary>
    /// Callback do botão Menu Principal.
    /// </summary>
    private void OnMainMenuClicked()
    {
        Debug.Log("Voltando ao Menu Principal...");

        // Reseta o timeScale (estava pausado)
        Time.timeScale = 1f;

        // Carrega a cena do menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        // Remove inscrição para evitar memory leaks
        if (playerController != null && playerController.stats != null)
        {
            playerController.stats.OnPlayerDied -= ShowGameOver;
        }

        // Remove listener do botão
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }
}
