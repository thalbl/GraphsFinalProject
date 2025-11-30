using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Editor Tool que gera automaticamente todos os FlavorTextData assets
/// com as frases narrativas do banco de dados.
/// </summary>
public class FlavorTextGenerator
{
    [MenuItem("Tools/Gerar Flavor Texts (Narrativa Completa)")]
    public static void GenerateAllFlavorTexts()
    {
        string folderPath = "Assets/Data/FlavorTexts";
        
        // Cria pasta se não existir
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        int count = 0;

        // ═══════════════════════════════════════════════════
        // ROOM SELECTION - COMBAT
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Selection_Combat_Normal", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            phrases = new List<string>
            {
                "> O cheiro de sangue seco emana desta porta.",
                "> Ruídos de metal arranhando pedra ecoam à frente.",
                "> Algo aguarda na escuridão... e está com fome.",
                "> O silêncio aqui é um aviso.",
                "> Marcas de garras adornam o arco da entrada."
            }
        });

        count += CreateFlavorText(folderPath, "Selection_Combat_LowSanity", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            condition = FlavorCondition.LowSanity,
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> *As paredes sussurram seus nomes.*",
                "> *NÃO ENTRE! ELES SABEM QUE VOCÊ ESTÁ AQUI!*",
                "> *Você ouve risadas... mas não há ninguém lá.*"
            }
        });

        count += CreateFlavorText(folderPath, "Selection_Combat_Imprudente", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Imprudente",
            phrases = new List<string>
            {
                "> (Imprudente) \"Vamos logo, eu quero sangue!\"",
                "> (Imprudente) \"Sem tempo para cautela, avancem!\"",
                "> (Imprudente) \"Finalmente, ação de verdade!\""
            }
        });

        // ═══════════════════════════════════════════════════
        // ROOM SELECTION - TREASURE
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Selection_Treasure_Normal", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Treasure,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Info,
            phrases = new List<string>
            {
                "> Um brilho dourado rompe a escuridão.",
                "> O ar parece menos pesado nesta direção.",
                "> Uma promessa de riqueza... ou uma isca?",
                "> Baús antigos aguardam, cobertos de poeira."
            }
        });

        count += CreateFlavorText(folderPath, "Selection_Treasure_Ganancioso", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Treasure,
            anyRoomType = false,
            requiredTrait = "Ganancioso",
            phrases = new List<string>
            {
                "> (Ganancioso) \"Ouro! Finalmente algo que vale a pena!\"",
                "> (Ganancioso) \"Minhas mãos já estão coçando...\"",
                "> (Ganancioso) \"MEUS preciosos...\""
            }
        });

        // ═══════════════════════════════════════════════════
        // ROOM SELECTION - CAMP
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Selection_Camp_Normal", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Camp,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> Restos de uma fogueira antiga indicam segurança.",
                "> Um local defensável para descansar os ossos.",
                "> O ambiente parece estranhamente calmo.",
                "> Marcas antigas sugerem que outros acamparam aqui."
            }
        });

        // ═══════════════════════════════════════════════════
        // ROOM SELECTION - BOSS
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Selection_Boss_Normal", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Boss,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> ═══ Uma presença opressora esmaga sua mente. ═══",
                "> ═══ Não há volta. O abismo olha de volta. ═══",
                "> ═══ O coração da dungeon bate atrás desta porta. ═══"
            }
        });

        // ═══════════════════════════════════════════════════
        // MOVEMENT - NORMAL
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Movement_Normal", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            phrases = new List<string>
            {
                "> A tocha tremula com o vento frio.",
                "> Passos ecoam nos corredores de pedra.",
                "> A poeira dança na luz fraca.",
                "> Avançando para o desconhecido.",
                "> O eco de seus passos é o único som."
            }
        });

        count += CreateFlavorText(folderPath, "Movement_LowHealth", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            condition = FlavorCondition.LowHealth,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> O rastro de sangue marca o caminho.",
                "> Cada passo é uma agonia.",
                "> A visão turva dificulta a marcha.",
                "> Mal consegue se manter em pé."
            }
        });

        count += CreateFlavorText(folderPath, "Movement_LowSanity", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            condition = FlavorCondition.LowSanity,
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> *As sombras parecem se alongar e agarrar as botas.*",
                "> *Sussurros ininteligíveis vêm das paredes.*",
                "> *Estaria o corredor ficando mais estreito?*",
                "> *Eles estão nos observando... eu sei que estão.*",
                "> *Os olhos nas paredes... tantos olhos...*"
            }
        });

        count += CreateFlavorText(folderPath, "Movement_Paranoico", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Paranóico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Paranóico) \"Ouvi algo atrás de nós!\"",
                "> (Paranóico) \"Isso é uma armadilha. EU SEI QUE É!\"",
                "> (Paranóico) \"Estão nos seguindo... eu sinto.\""
            }
        });

        // ═══════════════════════════════════════════════════
        // DAMAGE
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Damage_Normal", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> IMPACTO! -{amount} de {source}!",
                "> {source} desfere um golpe devastador! -{amount}",
                "> Sangue escorre. -{amount} HP."
            }
        });

        count += CreateFlavorText(folderPath, "Damage_LowHealth", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            condition = FlavorCondition.LowHealth,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> *A visão escurece.* -{amount} HP",
                "> *Mal consegue se manter consciente.* -{amount}",
                "> *Um golpe a mais e tudo acaba.*"
            }
        });

        // ═══════════════════════════════════════════════════
        // ROOM ENTER
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "RoomEnter_Treasure", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Treasure,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> Um baú reluzente aguarda.",
                "> Recursos recuperados. A jornada continua.",
                "> O fardo parece um pouco mais leve.",
                "> Um momento de alívio em meio ao horror."
            }
        });

        // Salva e atualiza
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ {count} Flavor Texts gerados em: {folderPath}");
        EditorUtility.DisplayDialog("Sucesso!", 
            $"{count} Flavor Text assets foram criados!\n\nCaminho: {folderPath}", 
            "OK");
    }

    /// <summary>
    /// Estrutura de configuração temporária para organizar dados.
    /// </summary>
    private class FlavorConfig
    {
        public FlavorContext context;
        public RoomType roomType = RoomType.Spawn;
        public bool anyRoomType = true;
        public FlavorCondition condition = FlavorCondition.None;
        public string requiredTrait = "";
        public bool overrideMessageType = false;
        public LogMessageType messageType = LogMessageType.Neutral;
        public List<string> phrases;
    }

    /// <summary>
    /// Cria um FlavorTextData asset.
    /// </summary>
    private static int CreateFlavorText(string folder, string fileName, FlavorConfig config)
    {
        FlavorTextData asset = ScriptableObject.CreateInstance<FlavorTextData>();
        
        asset.context = config.context;
        asset.roomType = config.roomType;
        asset.anyRoomType = config.anyRoomType;
        asset.condition = config.condition;
        asset.requiredTraitName = config.requiredTrait;
        asset.overrideMessageType = config.overrideMessageType;
        asset.messageTypeOverride = config.messageType;
        asset.phrases = config.phrases ?? new List<string>();

        string path = $"{folder}/{fileName}.asset";
        AssetDatabase.CreateAsset(asset, path);
        
        return 1; // Retorna 1 para contar
    }
}
