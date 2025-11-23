using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador do Menu de Debug estilo Persona 5
/// Gerencia a UI e sincroniza com o GameSessionState
/// </summary>
public class DebugMenuController : MonoBehaviour
{
    [Header("UI References - Sliders")]
    [SerializeField] private Slider maxRoomsSlider;
    [SerializeField] private Slider complexitySlider;
    
    [Header("UI References - Input Fields")]
    [SerializeField] private TMP_InputField seedInputField;
    
    [Header("UI References - Toggle")]
    [SerializeField] private Toggle directedGraphToggle;
    
    [Header("UI References - Value Displays")]
    [SerializeField] private TextMeshProUGUI maxRoomsValueText;
    [SerializeField] private TextMeshProUGUI complexityValueText;
    
    [Header("UI References - Panel")]
    [SerializeField] private GameObject menuPanel;
    
    [Header("References")]
    [SerializeField] private SceneReloader sceneReloader;
    
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    private bool isMenuOpen = false;

    private void Start()
    {
        // Inicializa os valores da UI com os valores do GameSessionState
        LoadValuesFromSessionState();
        
        // Conecta os listeners
        SetupListeners();
        
        // Começa com o menu fechado
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Toggle do menu com ESC
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// Carrega os valores do GameSessionState para a UI
    /// </summary>
    private void LoadValuesFromSessionState()
    {
        GameSessionState state = GameSessionState.Instance;

        if (maxRoomsSlider != null)
        {
            maxRoomsSlider.value = state.maxRooms;
            UpdateMaxRoomsDisplay(state.maxRooms);
        }

        if (complexitySlider != null)
        {
            complexitySlider.value = state.complexity * 100f; // 0-100 no slider
            UpdateComplexityDisplay(state.complexity);
        }

        if (seedInputField != null)
        {
            seedInputField.text = state.randomSeed.ToString();
        }

        if (directedGraphToggle != null)
        {
            directedGraphToggle.isOn = state.directedGraph;
        }
    }

    /// <summary>
    /// Conecta os eventos dos elementos UI
    /// </summary>
    private void SetupListeners()
    {
        if (maxRoomsSlider != null)
        {
            maxRoomsSlider.onValueChanged.AddListener(OnMaxRoomsChanged);
        }

        if (complexitySlider != null)
        {
            complexitySlider.onValueChanged.AddListener(OnComplexityChanged);
        }

        if (seedInputField != null)
        {
            seedInputField.onEndEdit.AddListener(OnSeedChanged);
        }

        if (directedGraphToggle != null)
        {
            directedGraphToggle.onValueChanged.AddListener(OnDirectedGraphChanged);
        }
    }

    // --- Event Handlers ---

    private void OnMaxRoomsChanged(float value)
    {
        int rooms = Mathf.RoundToInt(value);
        UpdateMaxRoomsDisplay(rooms);
        GameSessionState.Instance.maxRooms = rooms;
    }

    private void OnComplexityChanged(float value)
    {
        float complexity = value / 100f; // Converte 0-100 para 0-1
        UpdateComplexityDisplay(complexity);
        GameSessionState.Instance.complexity = complexity;
    }

    private void OnSeedChanged(string value)
    {
        if (int.TryParse(value, out int seed))
        {
            GameSessionState.Instance.randomSeed = seed;
        }
        else
        {
            // Se inválido, volta para o valor atual
            seedInputField.text = GameSessionState.Instance.randomSeed.ToString();
        }
    }

    private void OnDirectedGraphChanged(bool isDirected)
    {
        GameSessionState.Instance.directedGraph = isDirected;
    }

    // --- UI Display Updates ---

    private void UpdateMaxRoomsDisplay(int value)
    {
        if (maxRoomsValueText != null)
        {
            maxRoomsValueText.text = value.ToString();
        }
    }

    private void UpdateComplexityDisplay(float value)
    {
        if (complexityValueText != null)
        {
            complexityValueText.text = value.ToString("F2"); // 2 casas decimais
        }
    }

    // --- Menu Control ---

    /// <summary>
    /// Abre/fecha o menu
    /// </summary>
    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuOpen);
        }

        // Pausa o jogo quando o menu está aberto (opcional)
        Time.timeScale = isMenuOpen ? 0f : 1f;
    }

    /// <summary>
    /// Botão "RESTART REALITY" - Reinicia a cena com os novos valores
    /// </summary>
    public void OnRestartButtonClicked()
    {
        Debug.Log("[DebugMenu] TAKE YOUR HEART! Restarting scene with new settings...");
        
        // Restaura o time scale antes de recarregar
        Time.timeScale = 1f;
        
        if (sceneReloader != null)
        {
            GameSessionState state = GameSessionState.Instance;
            sceneReloader.UpdateAndReload(
                state.maxRooms, 
                state.randomSeed, 
                state.complexity, 
                state.directedGraph
            );
        }
        else
        {
            Debug.LogError("[DebugMenu] SceneReloader reference is missing!");
        }
    }

    /// <summary>
    /// Reseta para valores padrão
    /// </summary>
    public void OnResetButtonClicked()
    {
        GameSessionState.Instance.ResetToDefaults();
        LoadValuesFromSessionState();
    }
}
