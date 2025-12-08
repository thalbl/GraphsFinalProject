using UnityEngine;

/// <summary>
/// Script de teste para o sistema de Save/Load.
/// Anexe este script a um GameObject na cena para testar salvamento e carregamento.
/// Use as teclas F5 (salvar), F6 (carregar), F7 (vitória), F8 (derrota) durante o jogo.
/// NOTA: Desativado automaticamente em builds de produção.
/// </summary>
public class SaveLoadTester : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Se true, funciona mesmo em builds de produção. Use apenas para testes.")]
    [SerializeField] private bool forceEnableInBuild = false;

    private GameController gameController;
    private GameLog gameLog;
    private bool isEnabled = true;

    void Start()
    {
        // ═══ DESATIVA EM BUILDS DE PRODUÇÃO ═══
        #if !UNITY_EDITOR && !DEVELOPMENT_BUILD
        if (!forceEnableInBuild)
        {
            Debug.Log("[SaveLoadTester] Desativado em build de produção.");
            isEnabled = false;
            enabled = false;
            return;
        }
        #endif

        gameController = FindObjectOfType<GameController>();
        gameLog = FindObjectOfType<GameLog>();
        
        if (gameController == null)
        {
            Debug.LogError("[SaveLoadTester] GameController não encontrado!");
        }
        else
        {
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("  SAVE/LOAD TESTER ATIVO");
            Debug.Log("═══════════════════════════════════════════════════════");
            Debug.Log("  F5 = Salvar Jogo");
            Debug.Log("  F6 = Carregar Jogo");
            Debug.Log("  F7 = Forçar Vitória (testar métricas)");
            Debug.Log("  F8 = Forçar Derrota (testar métricas)");
            Debug.Log("  F9 = Listar Saves Disponíveis");
            Debug.Log("  F10 = Deletar Último Save");
            Debug.Log("═══════════════════════════════════════════════════════");
        }
    }

    void Update()
    {
        if (gameController == null) return;

        // F5 - Salvar
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TestSave();
        }

        // F6 - Carregar
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestLoad();
        }

        // F7 - Vitória
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestVictory();
        }

        // F8 - Derrota
        if (Input.GetKeyDown(KeyCode.F8))
        {
            TestDefeat();
        }

        // F9 - Listar saves
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ListSaves();
        }

        // F10 - Deletar save
        if (Input.GetKeyDown(KeyCode.F10))
        {
            DeleteTestSave();
        }
    }

    private void TestSave()
    {
        Debug.Log("──────────────────────────────────────");
        Debug.Log("🔹 TESTANDO SAVE...");
        Debug.Log("──────────────────────────────────────");

        bool success = gameController.SaveCurrentGame("test_save");

        if (success)
        {
            Debug.Log("✅ SAVE BEM-SUCEDIDO!");
            Debug.Log($"Arquivo: {Application.persistentDataPath}/Saves/test_save.json");
            
            // Log na UI do jogo
            if (gameLog != null)
                gameLog.LogMessage("> Progresso salvo com sucesso.", LogMessageType.Info);
        }
        else
        {
            Debug.LogError("❌ FALHA NO SAVE!");
            
            if (gameLog != null)
                gameLog.LogMessage("> ERRO: Falha ao salvar progresso!", LogMessageType.Danger);
        }

        Debug.Log("──────────────────────────────────────\n");
    }

    private void TestLoad()
    {
        Debug.Log("──────────────────────────────────────");
        Debug.Log("🔹 TESTANDO LOAD...");
        Debug.Log("──────────────────────────────────────");

        if (!SaveManager.SaveExists("test_save"))
        {
            Debug.LogWarning("⚠️ Nenhum save 'test_save' encontrado!");
            Debug.Log("Primeiro salve o jogo com F5!");
            Debug.Log("──────────────────────────────────────\n");
            
            if (gameLog != null)
                gameLog.LogMessage("> Nenhum save encontrado.", LogMessageType.Info);
            return;
        }

        if (gameLog != null)
            gameLog.LogMessage("> Carregando progresso...", LogMessageType.Info);

        bool success = gameController.LoadSavedGame("test_save");

        if (success)
        {
            Debug.Log("✅ LOAD BEM-SUCEDIDO!");
            Debug.Log("O jogo foi restaurado para o estado salvo.");
            
            if (gameLog != null)
                gameLog.LogMessage("> Progresso restaurado com sucesso.", LogMessageType.Gain);
        }
        else
        {
            Debug.LogError("❌ FALHA NO LOAD!");
            
            if (gameLog != null)
                gameLog.LogMessage("> ERRO: Falha ao carregar progresso!", LogMessageType.Danger);
        }

        Debug.Log("──────────────────────────────────────\n");
    }

    private void TestVictory()
    {
        Debug.Log("──────────────────────────────────────");
        Debug.Log("FORCANDO VITORIA PARA TESTE...");
        Debug.Log("──────────────────────────────────────");

        gameController.OnGameEnd(true);

        Debug.Log("Você verá as métricas calculadas acima!");
        Debug.Log("──────────────────────────────────────\n");
    }

    private void TestDefeat()
    {
        Debug.Log("──────────────────────────────────────");
        Debug.Log("💀 FORÇANDO DERROTA PARA TESTE...");
        Debug.Log("──────────────────────────────────────");

        gameController.OnGameEnd(false);

        Debug.Log("Você verá as métricas calculadas acima!");
        Debug.Log("──────────────────────────────────────\n");
    }

    private void ListSaves()
    {
        Debug.Log("──────────────────────────────────────");
        Debug.Log("📂 SAVES DISPONÍVEIS:");
        Debug.Log("──────────────────────────────────────");

        string[] saves = SaveManager.GetAllSaveFiles();

        if (saves.Length == 0)
        {
            Debug.Log("Nenhum save encontrado.");
        }
        else
        {
            for (int i = 0; i < saves.Length; i++)
            {
                Debug.Log($"{i + 1}. {saves[i]}");
            }
        }

        Debug.Log($"\nLocal: {Application.persistentDataPath}/Saves/");
        Debug.Log("──────────────────────────────────────\n");
    }

    private void DeleteTestSave()
    {
        Debug.Log("──────────────────────────────────────");
        Debug.Log("🗑️ DELETANDO test_save...");
        Debug.Log("──────────────────────────────────────");

        bool success = SaveManager.DeleteSave("test_save");

        if (success)
        {
            Debug.Log("✅ Save deletado com sucesso!");
        }
        else
        {
            Debug.LogWarning("⚠️ Save 'test_save' não encontrado!");
        }

        Debug.Log("──────────────────────────────────────\n");
    }
}
