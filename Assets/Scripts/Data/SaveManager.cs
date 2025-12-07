using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Gerencia a serialização e deserialização de saves do jogo.
/// Utiliza JSON para armazenar dados na pasta Data do projeto.
/// </summary>
public static class SaveManager
{
    private const string DATA_FOLDER = "Data";
    private const string FILE_EXTENSION = ".json";

    /// <summary>
    /// Retorna o caminho completo da pasta de saves.
    /// Em builds: pasta "Data" ao lado do executável (portátil).
    /// No editor: pasta "Data" na raiz do projeto.
    /// </summary>
    private static string GetSaveFolderPath()
    {
        string basePath;
        
        #if UNITY_EDITOR
        // No editor: pasta Data na raiz do projeto (ao lado de Assets)
        basePath = Path.GetDirectoryName(Application.dataPath);
        #else
        // Em builds: pasta Data ao lado do executável
        // Application.dataPath em build = [GameFolder]/[GameName]_Data
        // Queremos [GameFolder]/Data
        basePath = Path.GetDirectoryName(Application.dataPath);
        #endif
        
        string dataPath = Path.Combine(basePath, DATA_FOLDER);
        
        // Cria a pasta se não existir
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
            Debug.Log($"[SaveManager] Pasta Data criada em: {dataPath}");
        }
        
        return dataPath;
    }

    /// <summary>
    /// Retorna o caminho completo de um arquivo de save.
    /// </summary>
    private static string GetSaveFilePath(string fileName)
    {
        if (!fileName.EndsWith(FILE_EXTENSION))
        {
            fileName += FILE_EXTENSION;
        }
        
        return Path.Combine(GetSaveFolderPath(), fileName);
    }

    /// <summary>
    /// Salva o estado atual do jogo em JSON.
    /// </summary>
    /// <param name="fileName">Nome do arquivo (sem extensão)</param>
    /// <param name="saveData">Dados a serem salvos</param>
    /// <returns>True se o save foi bem-sucedido</returns>
    public static bool SaveGame(string fileName, GameSaveData saveData)
    {
        try
        {
            string filePath = GetSaveFilePath(fileName);
            string json = JsonUtility.ToJson(saveData, true); // prettyPrint = true
            
            File.WriteAllText(filePath, json);
            
            Debug.Log($"[SaveManager] Jogo salvo com sucesso: {filePath}");
            Debug.Log($"[SaveManager] Salas visitadas: {saveData.progressData.visitedRoomIndices.Count}, Passos: {saveData.progressData.totalSteps}");
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Erro ao salvar jogo: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Carrega um save do disco.
    /// </summary>
    /// <param name="fileName">Nome do arquivo (sem extensão)</param>
    /// <returns>Dados carregados ou null se houve erro</returns>
    public static GameSaveData LoadGame(string fileName)
    {
        try
        {
            string filePath = GetSaveFilePath(fileName);
            
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[SaveManager] Arquivo de save não encontrado: {filePath}");
                return null;
            }
            
            string json = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
            
            Debug.Log($"[SaveManager] Save carregado: {filePath}");
            Debug.Log($"[SaveManager] Seed: {saveData.dungeonData.seed}, Salas: {saveData.dungeonData.maxRooms}");
            Debug.Log($"[SaveManager] Progresso: {saveData.progressData.visitedRoomIndices.Count} salas visitadas");
            
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Erro ao carregar save: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verifica se um arquivo de save existe.
    /// </summary>
    public static bool SaveExists(string fileName)
    {
        string filePath = GetSaveFilePath(fileName);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Deleta um arquivo de save.
    /// </summary>
    public static bool DeleteSave(string fileName)
    {
        try
        {
            string filePath = GetSaveFilePath(fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[SaveManager] Save deletado: {filePath}");
                return true;
            }
            
            Debug.LogWarning($"[SaveManager] Save não encontrado para deletar: {filePath}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Erro ao deletar save: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Lista todos os saves disponíveis.
    /// </summary>
    public static string[] GetAllSaveFiles()
    {
        try
        {
            string savePath = GetSaveFolderPath();
            string[] files = Directory.GetFiles(savePath, "*" + FILE_EXTENSION);
            
            // Remove o path e a extensão, deixando apenas os nomes
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileNameWithoutExtension(files[i]);
            }
            
            Debug.Log($"[SaveManager] {files.Length} save(s) encontrado(s)");
            return files;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Erro ao listar saves: {e.Message}");
            return new string[0];
        }
    }

    /// <summary>
    /// Cria um GameSaveData a partir do estado atual do jogo.
    /// </summary>
    public static GameSaveData CreateSaveData(
        DungeonGenerator generator,
        PlayerController player,
        PlayerStats playerStats,
        HashSet<int> visitedRoomIndices,
        List<int> pathHistory,
        int totalSteps,
        List<EdgeConnectionData> costsApplied)
    {
        GameSaveData saveData = new GameSaveData();
        
        // 1. Dados de geração do dungeon
        saveData.dungeonData = new DungeonGenerationData(generator);
        
        // 2. Conexões das arestas (usando índices)
        saveData.edgeConnections = new List<EdgeConnectionData>();
        foreach (var room in generator.allRooms)
        {
            int fromIndex = generator.allRooms.IndexOf(room);
            
            foreach (var neighbor in room.connections)
            {
                int toIndex = generator.allRooms.IndexOf(neighbor);
                EdgeData edgeData = generator.GetEdgeCost(room, neighbor);
                
                if (edgeData != null)
                {
                    saveData.edgeConnections.Add(new EdgeConnectionData(fromIndex, toIndex, edgeData));
                }
            }
        }
        
        // 3. Estado do jogador
        RoomNode currentRoom = player.GetCurrentRoom();
        int currentRoomIndex = generator.allRooms.IndexOf(currentRoom);
        saveData.playerState = new PlayerStateData(currentRoomIndex, playerStats);
        
        // 4. Progresso do jogo
        saveData.progressData = new GameProgressData();
        saveData.progressData.visitedRoomIndices = new List<int>(visitedRoomIndices);
        saveData.progressData.pathHistory = new List<int>(pathHistory);
        saveData.progressData.totalSteps = totalSteps;
        saveData.progressData.costsApplied = new List<EdgeConnectionData>(costsApplied);
        
        Debug.Log($"[SaveManager] SaveData criado - {saveData.edgeConnections.Count} arestas, {visitedRoomIndices.Count} salas visitadas");
        
        return saveData;
    }

    /// <summary>
    /// Aplica dados de save carregados ao jogo.
    /// IMPORTANTE: O dungeon deve ser gerado ANTES de chamar este método.
    /// </summary>
    public static void ApplySaveData(
        GameSaveData saveData,
        DungeonGenerator generator,
        PlayerController player,
        PlayerStats playerStats,
        out HashSet<int> visitedRoomIndices,
        out List<int> pathHistory,
        out int totalSteps,
        out List<EdgeConnectionData> costsApplied)
    {
        // 1. Restaura estado do jogador
        RoomNode targetRoom = generator.allRooms[saveData.playerState.currentRoomIndex];
        
        Debug.Log($"[SaveManager] Aplicando save - VALORES SALVOS:");
        Debug.Log($"  Max HP: {saveData.playerState.maxHealth}, Current HP: {saveData.playerState.currentHealth}");
        Debug.Log($"  Max Sanity: {saveData.playerState.maxSanity}, Current Sanity: {saveData.playerState.currentSanity}");
        Debug.Log($"  Max Supplies: {saveData.playerState.maxSupplies}, Current Supplies: {saveData.playerState.currentSupplies}");
        
        playerStats.maxHealth = saveData.playerState.maxHealth;
        playerStats.currentHealth = saveData.playerState.currentHealth;
        playerStats.maxSanity = saveData.playerState.maxSanity;
        playerStats.currentSanity = saveData.playerState.currentSanity;
        playerStats.maxSupplies = saveData.playerState.maxSupplies;
        playerStats.currentSupplies = saveData.playerState.currentSupplies;
        
        Debug.Log($"[SaveManager] VALORES APLICADOS ao PlayerStats:");
        Debug.Log($"  HP: {playerStats.currentHealth:F1}/{playerStats.maxHealth:F1}");
        Debug.Log($"  Sanity: {playerStats.currentSanity:F1}/{playerStats.maxSanity:F1}");
        Debug.Log($"  Supplies: {playerStats.currentSupplies:F1}/{playerStats.maxSupplies:F1}");
        
        // Restaura traits
        playerStats.activeTraits.Clear();
        foreach (var serializedTrait in saveData.playerState.activeTraits)
        {
            playerStats.activeTraits.Add(serializedTrait.ToTrait());
        }
        
        // Move o player para a sala salva
        player.transform.position = targetRoom.GetWorldPosition();
        
        // 2. Restaura progresso
        visitedRoomIndices = new HashSet<int>(saveData.progressData.visitedRoomIndices);
        pathHistory = new List<int>(saveData.progressData.pathHistory);
        totalSteps = saveData.progressData.totalSteps;
        costsApplied = new List<EdgeConnectionData>(saveData.progressData.costsApplied);
        
        Debug.Log($"[SaveManager] Save aplicado - Player em sala {saveData.playerState.currentRoomIndex}");
        Debug.Log($"[SaveManager] Progresso: {visitedRoomIndices.Count} salas visitadas, {totalSteps} passos");
        
        // Notifica mudanças nos stats (CRÍTICO para atualizar UI)
        Debug.Log("[SaveManager] Chamando NotifyStatsChanged...");
        playerStats.NotifyStatsChanged();
    }

    /// <summary>
    /// Converte índices de sala para RoomNodes.
    /// </summary>
    public static List<RoomNode> IndicesToRooms(List<int> indices, List<RoomNode> allRooms)
    {
        List<RoomNode> rooms = new List<RoomNode>();
        foreach (int index in indices)
        {
            if (index >= 0 && index < allRooms.Count)
            {
                rooms.Add(allRooms[index]);
            }
        }
        return rooms;
    }

    /// <summary>
    /// Converte RoomNodes para índices.
    /// </summary>
    public static List<int> RoomsToIndices(List<RoomNode> rooms, List<RoomNode> allRooms)
    {
        List<int> indices = new List<int>();
        foreach (var room in rooms)
        {
            int index = allRooms.IndexOf(room);
            if (index >= 0)
            {
                indices.Add(index);
            }
        }
        return indices;
    }
}
