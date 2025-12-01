using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gerencia o recarregamento da cena atual aplicando as configurações do GameSessionState
/// </summary>
public class SceneReloader : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Referência ao DungeonGenerator da cena")]
    public DungeonGenerator dungeonGenerator;

    [Header("Settings")]
    [Tooltip("Se true, aplica as configurações do GameSessionState no Start")]
    public bool applySessionStateOnStart = true;

    private void Start()
    {
        if (applySessionStateOnStart)
        {
            ApplySessionStateToGenerator();
        }
    }

    /// <summary>
    /// Aplica as configurações do GameSessionState ao DungeonGenerator e regenera
    /// </summary>
    public void ApplySessionStateToGenerator()
    {
        if (dungeonGenerator == null)
        {
            // Tenta encontrar automaticamente
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
            
            if (dungeonGenerator == null)
            {
                Debug.LogError("[SceneReloader] DungeonGenerator not found in scene!");
                return;
            }
        }

        GameSessionState.Instance.ApplyToDungeonGenerator(dungeonGenerator);
    }

    /// <summary>
    /// Regenera a dungeon sem recarregar a cena (mais rápido e mantém o estado)
    /// </summary>
    public void RegenerateDungeon()
    {
        if (dungeonGenerator == null)
        {
            dungeonGenerator = FindObjectOfType<DungeonGenerator>();
            
            if (dungeonGenerator == null)
            {
                Debug.LogError("[SceneReloader] DungeonGenerator not found!");
                return;
            }
        }

        Debug.Log("[SceneReloader] Regenerating dungeon with new settings...");
        
        // Aplica os novos valores do GameSessionState
        GameSessionState.Instance.ApplyToDungeonGenerator(dungeonGenerator);
        
        // Regenera a dungeon (limpa a antiga e cria nova)
        dungeonGenerator.GenerateDungeon();
        
        Debug.Log("[SceneReloader] Dungeon regenerated successfully!");
    }

    /// <summary>
    /// Recarrega a cena atual (RESTART REALITY!)
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"[SceneReloader] TAKE YOUR HEART! Reloading scene: {currentSceneName}");
        
        SceneManager.LoadScene(currentSceneName);
    }

    /// <summary>
    /// Atualiza o GameSessionState e regenera a dungeon (SEM recarregar cena)
    /// </summary>
    public void UpdateAndRegenerate(int rooms, int seed, float complexity, bool directed)
    {
        GameSessionState.Instance.UpdateSettings(rooms, seed, complexity, directed);
        RegenerateDungeon();
    }

    /// <summary>
    /// Atualiza o GameSessionState e recarrega a cena (método antigo, mantido para compatibilidade)
    /// </summary>
    public void UpdateAndReload(int rooms, int seed, float complexity, bool directed)
    {
        GameSessionState.Instance.UpdateSettings(rooms, seed, complexity, directed);
        ReloadCurrentScene();
    }

    /// <summary>
    /// Reseta para valores padrão e recarrega
    /// </summary>
    public void ResetAndReload()
    {
        GameSessionState.Instance.ResetToDefaults();
        ReloadCurrentScene();
    }
}
