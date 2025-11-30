using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerenciador do menu principal.
/// Controla as ações dos botões e navegação entre cenas.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Cenas do Jogo")]
    [Tooltip("Nome da cena do jogo principal")]
    public string gameSceneName = "GameScene";
    
    [Tooltip("Nome da cena de configurações")]
    public string configSceneName = "ConfigScene";

    [Header("Configurações")]
    [Tooltip("Tempo de delay antes de carregar a cena (para animação)")]
    public float sceneLoadDelay = 0.3f;

    /// <summary>
    /// Executada pelos botões do menu através do MenuButton.
    /// </summary>
    public void ExecuteMenuAction(string action)
    {
        Debug.Log($"Menu Action: {action}");

        switch (action.ToLower())
        {
            case "novo jogo":
            case "newgame":
                StartNewGame();
                break;

            case "carregar":
            case "load":
                LoadGame();
                break;

            case "config":
            case "settings":
                OpenSettings();
                break;

            case "exit":
            case "quit":
                QuitGame();
                break;

            default:
                Debug.LogWarning($"Ação de menu desconhecida: {action}");
                break;
        }
    }

    /// <summary>
    /// Inicia um novo jogo.
    /// </summary>
    void StartNewGame()
    {
        Debug.Log("Iniciando novo jogo...");
        
        // Reseta dados de save se necessário
        PlayerPrefs.DeleteKey("CurrentSave");
        
        // Carrega a cena do jogo
        Invoke(nameof(LoadGameScene), sceneLoadDelay);
    }

    /// <summary>
    /// Carrega um jogo salvo.
    /// </summary>
    void LoadGame()
    {
        Debug.Log("Carregando jogo salvo...");
        
        // Verifica se existe save
        if (PlayerPrefs.HasKey("CurrentSave"))
        {
            Invoke(nameof(LoadGameScene), sceneLoadDelay);
        }
        else
        {
            Debug.LogWarning("Nenhum save encontrado!");
            // Mostrar mensagem de erro na UI
        }
    }

    /// <summary>
    /// Abre o menu de configurações.
    /// </summary>
    void OpenSettings()
    {
        Debug.Log("Abrindo configurações...");
        
        if (!string.IsNullOrEmpty(configSceneName))
        {
            Invoke(nameof(LoadConfigScene), sceneLoadDelay);
        }
        else
        {
            Debug.LogWarning("Cena de configurações não definida!");
        }
    }

    /// <summary>
    /// Sai do jogo.
    /// </summary>
    void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Métodos auxiliares para Invoke
    void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    void LoadConfigScene()
    {
        SceneManager.LoadScene(configSceneName);
    }

    // Métodos públicos alternativos (podem ser chamados diretamente por UnityEvents)
    public void OnNewGameButton() => ExecuteMenuAction("novo jogo");
    public void OnLoadGameButton() => ExecuteMenuAction("carregar");
    public void OnSettingsButton() => ExecuteMenuAction("config");
    public void OnQuitButton() => ExecuteMenuAction("exit");
}
