using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI de resumo final estilizada inspirada em design de revista/jornal
/// Exibe métricas de performance com visual dramático
/// </summary>
public class RunSummaryUIStyled : MonoBehaviour
{
    [Header("Container Principal")]
    [SerializeField] private GameObject reportContainer;
    
    [Header("Header")]
    [SerializeField] private TextMeshProUGUI headerTitle;
    [SerializeField] private TextMeshProUGUI headerSubtitle;
    
    [Header("Card de Eficiência (Grande)")]
    [SerializeField] private TextMeshProUGUI efficiencyValue;
    [SerializeField] private Slider efficiencySlider;
    [SerializeField] private TextMeshProUGUI efficiencyDescription;
    [SerializeField] private TextMeshProUGUI resourcesText;
    [SerializeField] private TextMeshProUGUI gradeStamp;
    
    [Header("Card de Exploração")]
    [SerializeField] private TextMeshProUGUI explorationValue;
    [SerializeField] private TextMeshProUGUI explorationLabel;
    [SerializeField] private Slider explorationSlider;
    [SerializeField] private TextMeshProUGUI explorationDescription;
    
    [Header("Mini Cards")]
    [SerializeField] private TextMeshProUGUI backtrackingValue;
    [SerializeField] private TextMeshProUGUI profileValue;
    [SerializeField] private TextMeshProUGUI profileDescription;
    
    [Header("Footer")]
    [SerializeField] private Button mainMenuButton;

    [Header("Animação")]
    [SerializeField] private CanvasGroup[] animatedCards;
    [SerializeField] private RectTransform stampTransform;
    [SerializeField] private float slideDelay = 0.2f;
    [SerializeField] private float slideDuration = 0.5f;

    private void Start()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void ShowReport(GameMetrics metrics)
    {
        HideGameplayUI();

        if (reportContainer != null)
            reportContainer.SetActive(true);

        // === HEADER ===
        if (headerTitle != null)
            headerTitle.text = "ANÁLISE";
        
        if (headerSubtitle != null)
            headerSubtitle.text = "ANÁLISE DE PERFORMANCE";

        // === EFICIÊNCIA ===
        float efficiencyPercent = metrics.pathOptimalityRatio * 100f;
        if (efficiencyValue != null)
            efficiencyValue.text = $"{efficiencyPercent:F0}%";
        
        if (efficiencySlider != null)
        {
            efficiencySlider.value = 0; // Garante que começa vazio para encher
            StartCoroutine(AnimateSlider(efficiencySlider, metrics.pathOptimalityRatio));
        }
        
        if (efficiencyDescription != null)
            efficiencyDescription.text = GetEfficiencyFeedback(metrics.pathOptimalityRatio);
        
        if (resourcesText != null)
        {
            float totalHealth = 0, totalSanity = 0, totalTime = 0;
            foreach (var cost in metrics.costsApplied)
            {
                totalHealth += cost.costHealth;
                totalSanity += cost.costSanity;
                totalTime += cost.costTime;
            }
            resourcesText.text = $"HP: <b>-{totalHealth:F0}</b>  |  SAN: <b>-{totalSanity:F0}</b>  |  TIME: <b>{totalTime:F0} Cycles</b>";
        }
        
        if (gradeStamp != null)
        {
            gradeStamp.text = GetGrade(metrics.pathOptimalityRatio);
            if (stampTransform != null)
                StartCoroutine(AnimateStamp(stampTransform));
        }

        // === EXPLORAÇÃO ===
        float explorationPercent = metrics.explorationIndex * 100f;
        if (explorationValue != null)
            explorationValue.text = $"{explorationPercent:F0}%";
        
        if (explorationLabel != null)
            explorationLabel.text = GetExplorationLabel(metrics.explorationIndex);
        
        if (explorationSlider != null)
        {
            explorationSlider.value = 0; // Garante que começa vazio para encher
            StartCoroutine(AnimateSlider(explorationSlider, metrics.explorationIndex, 0.4f));
        }
        
        if (explorationDescription != null)
            explorationDescription.text = GetExplorationFeedback(metrics.explorationIndex);

        // === BACKTRACKING ===
        if (backtrackingValue != null)
            backtrackingValue.text = metrics.backtrackingCost.ToString();

        // === PERFIL DE RISCO ===
        string cleanProfile = metrics.riskProfile;
        if (cleanProfile.Contains("("))
            cleanProfile = cleanProfile.Split('(')[0].Trim();

        if (profileValue != null)
            profileValue.text = cleanProfile.ToUpper();
        
        if (profileDescription != null)
            profileDescription.text = GetProfileDescription(cleanProfile);

        // === ANIMAÇÕES ===
        StartCoroutine(AnimateCardsEntrance());
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f; // Remove pause
        SceneManager.LoadScene("Menu_Test");
    }

    private void HideGameplayUI()
    {
        string[] canvasesToHide = new string[] { "PauseMenuCanvas", "CostSelectionCanvas", "PlayerUICanvas" };
        foreach (string name in canvasesToHide)
        {
            GameObject canvas = GameObject.Find(name);
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }
    }

    private IEnumerator AnimateCardsEntrance()
    {
        if (animatedCards == null) yield break;

        foreach (var card in animatedCards)
        {
            if (card != null)
            {
                card.alpha = 0;
                card.transform.localPosition += Vector3.right * 50;
            }
        }

        for (int i = 0; i < animatedCards.Length; i++)
        {
            if (animatedCards[i] != null)
            {
                yield return new WaitForSecondsRealtime(slideDelay * i);
                StartCoroutine(SlideIn(animatedCards[i]));
            }
        }
    }

    private IEnumerator SlideIn(CanvasGroup card)
    {
        float elapsed = 0;
        Vector3 startPos = card.transform.localPosition;
        Vector3 endPos = startPos - Vector3.right * 50;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            float eased = 1 - Mathf.Pow(1 - t, 3); // EaseOut Cubic

            card.alpha = eased;
            card.transform.localPosition = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        card.alpha = 1;
        card.transform.localPosition = endPos;
    }

    private IEnumerator AnimateSlider(Slider slider, float targetValue, float delay = 0.6f)
    {
        yield return new WaitForSecondsRealtime(delay);
        
        float elapsed = 0;
        float duration = 1.5f;
        float startValue = slider.value;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            slider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }
        
        slider.value = targetValue;
    }

    private IEnumerator AnimateStamp(RectTransform stamp)
    {
        yield return new WaitForSecondsRealtime(1f);
        
        stamp.localScale = Vector3.zero;
        CanvasGroup cg = stamp.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 0;

        float elapsed = 0;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float eased = Mathf.Sin(t * Mathf.PI * 0.5f); // EaseOut Sine
            
            stamp.localScale = Vector3.one * eased;
            if (cg != null) cg.alpha = eased;
            yield return null;
        }

        stamp.localScale = Vector3.one;
        if (cg != null) cg.alpha = 1;
    }

    // === FEEDBACK TEXTS ===
    private string GetEfficiencyFeedback(float ratio)
    {
        if (ratio >= 0.9f) return "Impressionante. Suas decisões foram quase robóticas. O caminho escolhido economizou recursos vitais.";
        if (ratio >= 0.7f) return "Sólido. Poucas decisões ruins. Manteve-se no rastro ideal na maior parte do tempo.";
        if (ratio >= 0.5f) return "Aceitável. Houve desvios, mas chegou ao objetivo sem desperdício excessivo.";
        return "Caótico. Muitos erros de navegação. A rota foi ineficiente e custosa.";
    }

    private string GetExplorationFeedback(float index)
    {
        if (index >= 0.8f) return "Explorador incansável. Vasculhou cada canto.";
        if (index >= 0.5f) return "Equilibrado. Explorou áreas importantes.";
        return "Focado no objetivo. Ignorou salas opcionais.";
    }

    private string GetExplorationLabel(float index)
    {
        if (index >= 0.8f) return "COMPLECIONISTA";
        if (index >= 0.5f) return "EQUILIBRADO";
        return "PRAGMÁTICO";
    }

    private string GetGrade(float ratio)
    {
        if (ratio >= 0.95f) return "S";
        if (ratio >= 0.85f) return "A";
        if (ratio >= 0.7f) return "B";
        if (ratio >= 0.5f) return "C";
        return "D";
    }

    private string GetProfileDescription(string profile)
    {
        switch (profile.ToUpper())
        {
            case "O MÁRTIR": return "Gastou muita Vida";
            case "O LOUCO": return "Gastou muita Sanidade";
            case "O IMPRUDENTE": return "Gastou muito Tempo";
            case "O PRUDENTE": return "Poupou Vida";
            case "O RACIONAL": return "Poupou Sanidade";
            case "O PACIENTE": return "Poupou Tempo";
            case "O EQUILIBRADO": return "Custos Balanceados";
            default: return "Estilo Único";
        }
    }

#if UNITY_EDITOR
    [ContextMenu("⚡ GERAR ESTRUTURA VISUAL (Clique Aqui)")]
    private void GenerateHierarchy()
    {
        // 1. Limpa filhos anteriores (segurança)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 2. Cria Container Principal
        reportContainer = CreateObj("ReportContainer", transform);
        RectTransform containerRect = reportContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // 3. Header
        GameObject header = CreateObj("Header", reportContainer.transform);
        SetupRect(header, 0, 1, 0, 1, 0, -50, 0, 150);
        headerTitle = CreateText("Title", "ANÁLISE", 80, header.transform);
        headerSubtitle = CreateText("Subtitle", "ANÁLISE DE PERFORMANCE", 30, header.transform, -60);

        // 4. Grid Area (Central)
        GameObject grid = CreateObj("MetricsGrid", reportContainer.transform);
        SetupRect(grid, 0.5f, 0.5f, 0.5f, 0.5f, -400, 200, 800, 500);

        // === CARD EFICIÊNCIA (Esquerda Grande) ===
        GameObject cardOptim = CreateImage("CardEfficiency", grid.transform, Color.white);
        SetupRect(cardOptim, 0, 0.5f, 0, 0.5f, -200, 0, 380, 500);
        cardOptim.transform.localRotation = Quaternion.Euler(0, 0, -2);

        efficiencyValue = CreateText("ValueBig", "94%", 90, cardOptim.transform, 20, Color.red);
        SetupRect(efficiencyValue.gameObject, 0.5f, 0.5f, 0.5f, 0.5f, 0, 50, 300, 120);

        // SLIDER EFICIÊNCIA
        GameObject sliderObj = CreateObj("EfficiencySlider", cardOptim.transform);
        SetupRect(sliderObj, 0.5f, 0.5f, 0.5f, 0.5f, 0, -20, 300, 20);
        efficiencySlider = CreateSlider(sliderObj, Color.red);

        efficiencyDescription = CreateText("Desc", "Análise de performance...", 18, cardOptim.transform, -80, Color.black);
        resourcesText = CreateText("Resources", "HP: -10 | SAN: -5", 16, cardOptim.transform, -130, Color.gray);
        
        GameObject stampObj = CreateObj("GradeStamp", cardOptim.transform);
        SetupRect(stampObj, 1, 1, 1, 1, -60, -60, 100, 100);
        gradeStamp = CreateText("Text", "S", 80, stampObj.transform, 0, Color.red);
        stampTransform = stampObj.GetComponent<RectTransform>();

        // === CARD EXPLORAÇÃO (Direita Cima) ===
        GameObject cardExplo = CreateImage("CardExploration", grid.transform, Color.white);
        SetupRect(cardExplo, 1, 1, 1, 1, -180, -90, 350, 180);

        explorationValue = CreateText("Value", "45%", 60, cardExplo.transform, 0, Color.black);
        explorationLabel = CreateText("Label", "PRAGMÁTICO", 20, cardExplo.transform, 40, Color.red);
        
        // SLIDER EXPLORAÇÃO
        GameObject exploSliderObj = CreateObj("ExplorationSlider", cardExplo.transform);
        SetupRect(exploSliderObj, 0.5f, 0, 0.5f, 0, 0, 20, 300, 10);
        explorationSlider = CreateSlider(exploSliderObj, Color.black);

        explorationDescription = CreateText("Desc", "Focado no objetivo", 16, cardExplo.transform, -50, Color.gray);

        // === MINI CARDS (Direita Baixo) ===
        GameObject cardBack = CreateImage("CardBacktracking", grid.transform, new Color(0.1f, 0.1f, 0.1f));
        SetupRect(cardBack, 1, 0, 1, 0, -270, 90, 160, 150);
        backtrackingValue = CreateText("Value", "2", 50, cardBack.transform, 0, Color.white);
        CreateText("Label", "Backtracking", 14, cardBack.transform, 40, Color.gray);

        GameObject cardProf = CreateImage("CardProfile", grid.transform, new Color(0.1f, 0.1f, 0.1f));
        SetupRect(cardProf, 1, 0, 1, 0, -90, 90, 160, 150);
        profileValue = CreateText("Value", "SÁBIO", 24, cardProf.transform, 0, Color.yellow);
        CreateText("Label", "PERFIL", 15, cardProf.transform, 40, Color.gray); 
        profileDescription = CreateText("Desc", "Poupou Sanidade", 12, cardProf.transform, -30, Color.gray);

        // === FOOTER ===
        GameObject footer = CreateObj("Footer", reportContainer.transform);
        SetupRect(footer, 0.5f, 0, 0.5f, 0, 0, 50, 400, 60);

        // BOTÃO MENU
        GameObject btnObj = CreateImage("BtnMenu", footer.transform, new Color(0.2f, 0.2f, 0.2f));
        SetupRect(btnObj, 0, 0, 1, 1, 0, 0, 0, 0); // Ocupa todo footer
        mainMenuButton = btnObj.AddComponent<Button>();
        CreateText("BtnText", "MENU PRINCIPAL", 24, btnObj.transform, 0, Color.white);
        
        // Auto-assign
        animatedCards = new CanvasGroup[] { 
            GetOrAddCG(cardOptim), 
            GetOrAddCG(cardExplo), 
            GetOrAddCG(cardBack), 
            GetOrAddCG(cardProf) 
        };

        Debug.Log("Estrutura de UI gerada com sucesso! Ajuste as fontes e sprites agora.");
    }

    // --- Helpers de Geração ---
    private GameObject CreateObj(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private GameObject CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = CreateObj(name, parent);
        Image img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    private TextMeshProUGUI CreateText(string name, string content, float size, Transform parent, float yOffset = 0, Color? color = null)
    {
        GameObject go = CreateObj(name, parent);
        TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.fontSize = size;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = color ?? Color.white;
        
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.sizeDelta = new Vector2(0, size + 10);
        rt.anchoredPosition = new Vector2(0, yOffset);
        
        return txt;
    }

    private void SetupRect(GameObject go, float amnX, float amnY, float amxX, float amxY, float posX, float posY, float w, float h)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(amnX, amnY);
        rt.anchorMax = new Vector2(amxX, amxY);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta = new Vector2(w, h);
    }

    private Slider CreateSlider(GameObject go, Color fillColor)
    {
        Slider slider = go.AddComponent<Slider>();
        
        // Background
        GameObject bg = CreateImage("Background", go.transform, new Color(0.9f, 0.9f, 0.9f));
        SetupRect(bg, 0, 0, 1, 1, 0, 0, 0, 0);
        
        // Fill Area - esticado para preencher o slider
        GameObject fillArea = CreateObj("Fill Area", go.transform);
        SetupRect(fillArea, 0, 0, 1, 1, 0, 0, 0, 0);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.offsetMin = new Vector2(5, 2); 
        fillAreaRect.offsetMax = new Vector2(-5, -2);

        // Fill - usando Image.Filled para controle por código
        GameObject fill = CreateImage("Fill", fillArea.transform, fillColor);
        RectTransform fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        
        // Configura como Filled Image (como no artigo do Health Bar)
        Image fillImage = fill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0; // Começa vazio
        
        // Configura Slider
        slider.targetGraphic = bg.GetComponent<Image>();
        slider.fillRect = fillRT;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;
        
        // Desativa interação do usuário (apenas visual, controlado por código)
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;

        return slider;
    }

    private CanvasGroup GetOrAddCG(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
#endif
}
