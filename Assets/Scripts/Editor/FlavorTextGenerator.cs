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
                "> Restos de uma fogueira prometem descanso e segurança.",
                "> Um refúgio onde corpo e mente podem se recuperar.",
                "> O ambiente calmo convida ao descanso.",
                "> Um lugar perfeito para restaurar as forças.",
                "> Finalmente, um momento de paz neste inferno."
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

        count += CreateFlavorText(folderPath, "RoomEnter_Camp", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Camp,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> O calor da fogueira acalma o corpo e a mente.",
                "> Finalmente, um momento para respirar e se recuperar.",
                "> As feridas começam a sarar no descanso.",
                "> A paz deste lugar restaura a sanidade.",
                "> Os pesadelos parecem mais distantes aqui."
            }
        });

        // ═══════════════════════════════════════════════════
        // TRAITS - AFFLICTIONS (ROOM SELECTION)
        // ═══════════════════════════════════════════════════

        // PARANÓICO - Room Selection (Combat)
        count += CreateFlavorText(folderPath, "Select_Combat_Paranoico", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Paranóico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Paranóico) \"Eles estão lá dentro... esperando por nós.\"",
                "> (Paranóico) \"Muito quieto. Quieto demais. É uma emboscada.\"",
                "> (Paranóico) \"Eu vi os olhos na escuridão. Vocês não viram?\""
            }
        });

        // IMPRUDENTE - Room Selection (Combat)
        count += CreateFlavorText(folderPath, "Select_Combat_Imprudente", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Imprudente",
            phrases = new List<string>
            {
                "> (Imprudente) \"Chega de planejar! Vamos matar alguma coisa!\"",
                "> (Imprudente) \"O caminho mais rápido é através deles.\"",
                "> (Imprudente) \"Eu vou na frente. Tente acompanhar.\""
            }
        });

        // HESITANTE - Room Selection
        count += CreateFlavorText(folderPath, "Select_Any_Hesitante", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            anyRoomType = true,
            requiredTrait = "Hesitante",
            overrideMessageType = true,
            messageType = LogMessageType.Neutral,
            phrases = new List<string>
            {
                "> (Hesitante) \"Temos... temos mesmo certeza disto?\"",
                "> (Hesitante) \"Talvez devêssemos voltar para a entrada...\"",
                "> (Hesitante) \"E se a tocha apagar? E se não houver saída?\""
            }
        });

        // GANANCIOSO - Room Selection (Treasure)
        count += CreateFlavorText(folderPath, "Select_Treasure_Ganancioso", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Treasure,
            anyRoomType = false,
            requiredTrait = "Ganancioso",
            overrideMessageType = true,
            messageType = LogMessageType.Info,
            phrases = new List<string>
            {
                "> (Ganancioso) \"Ouro! Eu sinto o cheiro dele!\"",
                "> (Ganancioso) \"Saiam da frente, esse baú é meu!\"",
                "> (Ganancioso) \"Finalmente, algo que vale a pena morrer.\""
            }
        });

        // FRÁGIL - Room Selection
        count += CreateFlavorText(folderPath, "Select_Any_Fragil", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            anyRoomType = true,
            requiredTrait = "Frágil",
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> (Frágil) \"Não sei se consigo dar mais um passo...\"",
                "> (Frágil) \"Minhas pernas tremem. Estou tão cansado.\""
            }
        });

        // ═══════════════════════════════════════════════════
        // TRAITS - AFFLICTIONS (MOVEMENT)
        // ═══════════════════════════════════════════════════

        // PARANÓICO - Movement
        count += CreateFlavorText(folderPath, "Movement_Paranoico_Extra", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Paranóico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Paranóico) \"Pare! Ouvi algo atrás de nós!\"",
                "> (Paranóico) \"Quem está a carregar a tocha? Não confio nele.\"",
                "> (Paranóico) \"Estamos a andar em círculos... eu sei que estamos.\""
            }
        });

        // IMPRUDENTE - Movement
        count += CreateFlavorText(folderPath, "Movement_Imprudente", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Imprudente",
            phrases = new List<string>
            {
                "> (Imprudente) \"Corram! A glória não espera por ninguém!\"",
                "> (Imprudente) \"Armadilhas são apenas cócegas.\""
            }
        });

        // HESITANTE - Movement
        count += CreateFlavorText(folderPath, "Movement_Hesitante", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Hesitante",
            overrideMessageType = true,
            messageType = LogMessageType.Neutral,
            phrases = new List<string>
            {
                "> (Hesitante) \"Um passo de cada vez... devagar...\"",
                "> (Hesitante) \"Espere! Acho que vi algo se mover.\""
            }
        });

        // CLAUSTROFÓBICO - Movement
        count += CreateFlavorText(folderPath, "Movement_Claustrofobico", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Claustrofóbico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Claustrofóbico) \"O teto... está a baixar. O TETO ESTÁ A BAIXAR!\"",
                "> (Claustrofóbico) \"Preciso de ar! Saiam da minha frente!\"",
                "> (Claustrofóbico) \"As paredes estão a esmagar-me...\"",
                "> (Claustrofóbico) *Respiração hiperventilada e pânico visível.*"
            }
        });

        // FRÁGIL - Movement
        count += CreateFlavorText(folderPath, "Movement_Fragil", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Frágil",
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> (Frágil) \"Cada passo é uma agonia...\"",
                "> (Frágil) \"Por favor... só um momento de descanso...\""
            }
        });

        // ═══════════════════════════════════════════════════
        // TRAITS - AFFLICTIONS (DAMAGE)
        // ═══════════════════════════════════════════════════

        // IMPRUDENTE - Damage
        count += CreateFlavorText(folderPath, "Damage_Imprudente", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Imprudente",
            overrideMessageType = true,
            messageType = LogMessageType.Neutral,
            phrases = new List<string>
            {
                "> (Imprudente) \"Hah! Isso é tudo o que tens?\"",
                "> (Imprudente) \"Sangue apenas me faz lutar melhor.\""
            }
        });

        // FRÁGIL - Damage
        count += CreateFlavorText(folderPath, "Damage_Fragil", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Frágil",
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> (Frágil) *Chora silenciosamente enquanto sangra.*",
                "> (Frágil) \"Eu sabia... eu vou quebrar.\""
            }
        });

        // ═══════════════════════════════════════════════════
        // TRAITS - VIRTUES
        // ═══════════════════════════════════════════════════

        // ESTOICO - Room Selection (Combat/Boss)
        count += CreateFlavorText(folderPath, "Select_Danger_Estoico", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Estoico",
            phrases = new List<string>
            {
                "> (Estoico) \"O medo é uma escolha. Nós avançamos.\"",
                "> (Estoico) \"Seja o que for, nós aguentamos.\""
            }
        });

        // ESTOICO - During Sanity Damage
        count += CreateFlavorText(folderPath, "Damage_Sanity_Estoico", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Estoico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Estoico) \"Mantenham o foco. É apenas uma ilusão.\"",
                "> (Estoico) \"A mente comanda o corpo. Não cedam.\""
            }
        });

        // VIGOROSO - Damage
        count += CreateFlavorText(folderPath, "Damage_Vigoroso", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Vigoroso",
            phrases = new List<string>
            {
                "> (Vigoroso) \"É apenas um arranhão. Continuem a marcha.\"",
                "> (Vigoroso) \"Já sofri pior no café da manhã.\"",
                "> (Vigoroso) *Cospe sangue e sorri.*"
            }
        });

        // VIGOROSO - Room Selection (Combat)
        count += CreateFlavorText(folderPath, "Select_Combat_Vigoroso", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Vigoroso",
            phrases = new List<string>
            {
                "> (Vigoroso) \"Eu serei o escudo. Fiquem atrás de mim.\""
            }
        });

        // ESTRATEGISTA - Room Selection
        count += CreateFlavorText(folderPath, "Select_Any_Estrategista", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            anyRoomType = true,
            requiredTrait = "Estrategista",
            phrases = new List<string>
            {
                "> (Estrategista) \"Calculado. A probabilidade de sucesso é aceitável.\"",
                "> (Estrategista) \"Esta é a rota mais eficiente. Sigam o plano.\""
            }
        });

        // ESTRATEGISTA - Movement
        count += CreateFlavorText(folderPath, "Movement_Estrategista", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Estrategista",
            phrases = new List<string>
            {
                "> (Estrategista) \"Mantenham a formação. Olhos nos flancos.\"",
                "> (Estrategista) \"Economizem recursos. A jornada é longa.\""
            }
        });

        // LIGEIRO - Movement
        count += CreateFlavorText(folderPath, "Movement_Ligeiro", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Ligeiro",
            phrases = new List<string>
            {
                "> (Ligeiro) \"Conheço este tipo de terreno. Sigam as minhas pegadas.\"",
                "> (Ligeiro) \"Rápido e silencioso, como uma sombra.\"",
                "> (Ligeiro) \"Posso ver o fim do túnel. Vamos!\""
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
