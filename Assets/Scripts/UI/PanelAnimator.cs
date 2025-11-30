using UnityEngine;
using System.Collections;

/// <summary>
/// Anima painéis do menu com efeitos estilo Persona 5
/// (Fade + Scale + Slide)
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class PanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Transform Effects")]
    [SerializeField] private float slideDistance = -100f; // Distância do slide (negativo = esquerda)
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 visibleScale = Vector3.one;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Coroutine currentAnimation;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    /// <summary>
    /// Mostra o painel com animação
    /// </summary>
    public void Show(float delay = 0f)
    {
        gameObject.SetActive(true);
        
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        // Só inicia coroutine se o GameObject está ativo e componentes inicializados
        if (gameObject.activeInHierarchy && canvasGroup != null && rectTransform != null)
        {
            currentAnimation = StartCoroutine(AnimateShow(delay));
        }
        else
        {
            // Fallback: mostra imediatamente sem animação
            ShowImmediate();
        }
    }

    /// <summary>
    /// Esconde o painel com animação
    /// </summary>
    public void Hide(float delay = 0f)
    {
        // Só pode esconder se estiver ativo (coroutines precisam de GameObject ativo)
        if (!gameObject.activeInHierarchy || canvasGroup == null || rectTransform == null)
        {
            HideImmediate();
            return;
        }
        
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
            
        currentAnimation = StartCoroutine(AnimateHide(delay));
    }

    /// <summary>
    /// Mostra instantaneamente sem animação
    /// </summary>
    public void ShowImmediate()
    {
        if (currentAnimation != null && gameObject.activeInHierarchy)
            StopCoroutine(currentAnimation);
        
        // Verifica se componentes existem
        if (canvasGroup == null || rectTransform == null)
            return;
            
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = visibleScale;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Esconde instantaneamente sem animação
    /// </summary>
    public void HideImmediate()
    {
        if (currentAnimation != null && gameObject.activeInHierarchy)
            StopCoroutine(currentAnimation);
        
        // Verifica se componentes existem antes de usar
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        gameObject.SetActive(false);
    }

    private IEnumerator AnimateShow(float delay)
    {
        // Estado inicial (escondido)
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = originalPosition + new Vector3(slideDistance, 0, 0);
        rectTransform.localScale = hiddenScale;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Usa unscaled pois Time.timeScale pode ser 0
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = animationCurve.Evaluate(t);

            // Animate opacity
            canvasGroup.alpha = curveValue;

            // Animate position (slide)
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, originalPosition, curveValue);

            // Animate scale
            rectTransform.localScale = Vector3.Lerp(startScale, visibleScale, curveValue);

            yield return null;
        }

        // Estado final
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = visibleScale;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        currentAnimation = null;
    }

    private IEnumerator AnimateHide(float delay)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 targetPos = originalPosition + new Vector3(slideDistance, 0, 0);

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = animationCurve.Evaluate(t);

            // Animate opacity (reverse)
            canvasGroup.alpha = 1f - curveValue;

            // Animate position (reverse slide)
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, curveValue);

            // Animate scale (reverse)
            rectTransform.localScale = Vector3.Lerp(visibleScale, hiddenScale, curveValue);

            yield return null;
        }

        // Estado final
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = targetPos;
        rectTransform.localScale = hiddenScale;
        gameObject.SetActive(false);

        currentAnimation = null;
    }
}
