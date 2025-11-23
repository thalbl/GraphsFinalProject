using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador do Menu de Pause estilo Persona 5 com dois painéis:
/// 1. Main Menu (Continuar, Config, Opções, Sair)
/// 2. Debug Menu (Configurações da Dungeon)
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject debugMenuPanel;
    
    [Header("Panel Animators (Optional)")]
    [SerializeField] private PanelAnimator mainMenuAnimator;
    [SerializeField] private PanelAnimator debugMenuAnimator;
    
    [Header("Background Blocker")]
    [SerializeField] private MenuBlocker menuBlocker; // Bloqueia interação com o mundo
    
    [Header("Main Menu Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button configRealityButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Debug Menu - Sliders")]
    [SerializeField] private Slider maxRoomsSlider;
    [SerializeField] private Slider complexitySlider;
    
    [Header("Debug Menu - Input Fields")]
    [SerializeField] private TMP_InputField seedInputField;
    
    [Header("Debug Menu - Toggle")]
    [SerializeField] private Toggle directedGraphToggle;
    
    [Header("Debug Menu - Value Displays")]
    [SerializeField] private TextMeshProUGUI maxRoomsValueText;
    [SerializeField] private TextMeshProUGUI complexityValueText;
    
    [Header("Debug Menu - Action Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button regenButton;
    
    [Header("References")]
    [SerializeField] private SceneReloader sceneReloader;
    
    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isMenuOpen = false;
    private bool isInMainMenu = true; // true = Main Menu, false = Debug Menu

    private void Start()
    {
        // Conecta os listeners dos botões
        SetupMainMenuButtons();
        SetupDebugMenuControls();
        
        // Começa com o menu fechado
        CloseMenu();
    }

    private void Update()
    {
        // Toggle do menu com ESC
        if (Input.GetKeyDown(toggleKey))
        {
            if (isMenuOpen)
            {
                // Se estiver no debug menu, volta pro main menu
                if (!isInMainMenu)
                {
                    ShowMainMenu();
                }
                else
                {
                    // Se estiver no main menu, fecha tudo
                    ContinueGame();
                }
            }
            else
            {
                // Abre o menu principal
                OpenMenu();
            }
        }
    }

    // --- MENU PRINCIPAL ---

    private void SetupMainMenuButtons()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
            
        if (configRealityButton != null)
            configRealityButton.onClick.AddListener(ShowDebugMenu);
            
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMainMenu);
    }

    private void OpenMenu()
    {
        isMenuOpen = true;
        isInMainMenu = true;
        
        // Ativa o blocker (escurece fundo e bloqueia raycasts)
        if (menuBlocker != null)
        {
            menuBlocker.Show();
        }
        
        // Animação: se temos animador, usa; senão ativa direto
        if (mainMenuAnimator != null)
        {
            mainMenuAnimator.Show();
        }
        else
        {
            mainMenuPanel?.SetActive(true);
        }
        
        // Garante que o debug menu esteja escondido
        if (debugMenuAnimator != null)
        {
            debugMenuAnimator.HideImmediate();
        }
        else
        {
            debugMenuPanel?.SetActive(false);
        }
        
        Time.timeScale = 0f; // Pausa o jogo
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        
        // Desativa o blocker
        if (menuBlocker != null)
        {
            menuBlocker.Hide();
        }
        
        // Animação de saída
        if (mainMenuAnimator != null)
        {
            mainMenuAnimator.Hide();
        }
        else
        {
            mainMenuPanel?.SetActive(false);
        }
        
        if (debugMenuAnimator != null)
        {
            debugMenuAnimator.HideImmediate();
        }
        else
        {
            debugMenuPanel?.SetActive(false);
        }
        
        Time.timeScale = 1f; // Despausa o jogo
    }

    public void ContinueGame()
    {
        Debug.Log("[PauseMenu] Continuing game...");
        CloseMenu();
    }

    private void OpenOptions()
    {
        Debug.Log("[PauseMenu] Options menu not implemented yet");
        // TODO: Implementar menu de opções
    }

    private void QuitToMainMenu()
    {
        Debug.Log("[PauseMenu] Quitting to main menu...");
        Time.timeScale = 1f; // Restaura time scale antes de trocar de cena
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // --- DEBUG MENU ---

    private void ShowDebugMenu()
    {
        Debug.Log("[PauseMenu] Opening Debug Menu");
        isInMainMenu = false;
        
        // Anima saída do Main Menu
        if (mainMenuAnimator != null)
        {
            mainMenuAnimator.Hide();
        }
        else
        {
            mainMenuPanel?.SetActive(false);
        }
        
        // Anima entrada do Debug Menu (com pequeno delay)
        if (debugMenuAnimator != null)
        {
            debugMenuAnimator.Show(0.1f); // Delay de 100ms
        }
        else
        {
            debugMenuPanel?.SetActive(true);
        }
        
        // Carrega valores atuais do GameSessionState
        LoadDebugValues();
    }

    private void ShowMainMenu()
    {
        Debug.Log("[PauseMenu] Returning to Main Menu");
        isInMainMenu = true;
        
        // Anima saída do Debug Menu
        if (debugMenuAnimator != null)
        {
            debugMenuAnimator.Hide();
        }
        else
        {
            debugMenuPanel?.SetActive(false);
        }
        
        // Anima entrada do Main Menu (com pequeno delay)
        if (mainMenuAnimator != null)
        {
            mainMenuAnimator.Show(0.1f);
        }
        else
        {
            mainMenuPanel?.SetActive(true);
        }
    }

    private void SetupDebugMenuControls()
    {
        // Sliders
        if (maxRoomsSlider != null)
            maxRoomsSlider.onValueChanged.AddListener(OnMaxRoomsChanged);
            
        if (complexitySlider != null)
            complexitySlider.onValueChanged.AddListener(OnComplexityChanged);
        
        // Input Field
        if (seedInputField != null)
            seedInputField.onEndEdit.AddListener(OnSeedChanged);
        
        // Toggle
        if (directedGraphToggle != null)
            directedGraphToggle.onValueChanged.AddListener(OnDirectedGraphChanged);
        
        // Action Buttons
        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);
            
        if (regenButton != null)
            regenButton.onClick.AddListener(OnRegenButtonClicked);
    }

    private void LoadDebugValues()
    {
        GameSessionState state = GameSessionState.Instance;

        if (maxRoomsSlider != null)
        {
            maxRoomsSlider.value = state.maxRooms;
            UpdateMaxRoomsDisplay(state.maxRooms);
        }

        if (complexitySlider != null)
        {
            complexitySlider.value = state.complexity * 100f;
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

    // --- Event Handlers ---

    private void OnMaxRoomsChanged(float value)
    {
        int rooms = Mathf.RoundToInt(value);
        UpdateMaxRoomsDisplay(rooms);
        GameSessionState.Instance.maxRooms = rooms;
    }

    private void OnComplexityChanged(float value)
    {
        float complexity = value / 100f;
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
            seedInputField.text = GameSessionState.Instance.randomSeed.ToString();
        }
    }

    private void OnDirectedGraphChanged(bool isDirected)
    {
        GameSessionState.Instance.directedGraph = isDirected;
    }

    private void UpdateMaxRoomsDisplay(int value)
    {
        if (maxRoomsValueText != null)
            maxRoomsValueText.text = value.ToString();
    }

    private void UpdateComplexityDisplay(float value)
    {
        if (complexityValueText != null)
            complexityValueText.text = value.ToString("F2");
    }

    // --- REGENERAÇÃO ---

    private void OnRegenButtonClicked()
    {
        Debug.Log("[PauseMenu] TAKE YOUR HEART! Regenerating dungeon...");
        
        // Feedback visual
        if (regenButton != null)
        {
            var buttonText = regenButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string originalText = buttonText.text;
                buttonText.text = "GERANDO...";
                
                // Restaura após regeneração
                StartCoroutine(ResetButtonTextAfterDelay(buttonText, originalText, 0.5f));
            }
        }
        
        // NÃO restaura time scale - mantém pausado durante regeneração
        // O menu continua aberto para o player ver os novos valores
        
        if (sceneReloader != null)
        {
            GameSessionState state = GameSessionState.Instance;
            
            // USA O NOVO MÉTODO: Regenera sem recarregar cena
            sceneReloader.UpdateAndRegenerate(
                state.maxRooms,
                state.randomSeed,
                state.complexity,
                state.directedGraph
            );
            
            Debug.Log("[PauseMenu] Dungeon regenerated! Menu remains open.");
        }
        else
        {
            Debug.LogError("[PauseMenu] SceneReloader reference is missing!");
        }
    }

    private System.Collections.IEnumerator ResetButtonTextAfterDelay(TextMeshProUGUI textComponent, string originalText, float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Usa realtime pois Time.timeScale = 0
        textComponent.text = originalText;
    }
}
