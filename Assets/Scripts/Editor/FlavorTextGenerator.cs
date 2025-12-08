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

        // ═══════════════════════════════════════════════════════════════════
        // ═══════════════════════════════════════════════════════════════════
        //                    NOVAS FRASES ADICIONAIS
        // ═══════════════════════════════════════════════════════════════════
        // ═══════════════════════════════════════════════════════════════════

        // NOTA: GPS flavor texts são tratados diretamente em PathfinderGPS.cs
        // para garantir que apareçam apenas ao usar o GPS, não em room selection.

        // ═══════════════════════════════════════════════════
        // ROOM SELECTION - EVENT
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Selection_Event_Normal", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Event,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Info,
            phrases = new List<string>
            {
                "> Algo incomum emana desta passagem.",
                "> O destino aguarda com uma carta marcada.",
                "> Um encontro inesperado se aproxima.",
                "> O ar crepita com possibilidades...",
                "> Nem tudo o que brilha é ouro. Nem tudo o que assusta é perigo.",
                "> O acaso prepara seu próximo lance."
            }
        });

        // ═══════════════════════════════════════════════════
        // ROOM ENTER - ALL TYPES
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "RoomEnter_Event", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Event,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Info,
            phrases = new List<string>
            {
                "> O inesperado se revela...",
                "> O destino teceu sua trama.",
                "> Uma decisão deve ser tomada.",
                "> Os dados foram lançados.",
                "> O véu do desconhecido se abre."
            }
        });

        count += CreateFlavorText(folderPath, "RoomEnter_Boss", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Boss,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> ═══ O SENHOR DESTA MASMORRA AGUARDAVA. ═══",
                "> ═══ A ESCURIDÃO ABSOLUTA TE ENVOLVE. ═══",
                "> ═══ VOCÊ CHEGOU AO FIM DA JORNADA. ═══",
                "> ═══ O DESTINO FOI SELADO NESTE MOMENTO. ═══",
                "> ═══ A ÚLTIMA PORTA SE ABRE. NÃO HÁ VOLTA. ═══"
            }
        });

        count += CreateFlavorText(folderPath, "RoomEnter_Spawn", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Spawn,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Info,
            phrases = new List<string>
            {
                "> A entrada da dungeon. Não há caminho de volta.",
                "> A aventura começa aqui.",
                "> A luz da superfície se despede de você.",
                "> O portal se fecha. Só resta avançar."
            }
        });

        count += CreateFlavorText(folderPath, "RoomEnter_Combat", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Combat,
            anyRoomType = false,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> As sombras se movem. Prepare-se para lutar!",
                "> O inimigo foi avistado. Não há como recuar.",
                "> O cheiro de sangue invade suas narinas.",
                "> Olhos brilham na escuridão. Você não está sozinho.",
                "> O silêncio é quebrado por um grito de guerra!",
                "> Armas em riste. A batalha começou."
            }
        });

        // ═══════════════════════════════════════════════════
        // CONDITIONS - HIGH/LOW HEALTH & SANITY
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Movement_HighHealth", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            condition = FlavorCondition.HighHealth,
            phrases = new List<string>
            {
                "> Os músculos respondem com vigor renovado.",
                "> Cada passo é firme e decidido.",
                "> A força pulsa em suas veias.",
                "> A vitalidade transborda."
            }
        });

        count += CreateFlavorText(folderPath, "Movement_HighSanity", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            condition = FlavorCondition.HighSanity,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> A mente está clara como água cristalina.",
                "> Foco absoluto. O caminho parece óbvio.",
                "> A racionalidade domina os pensamentos.",
                "> A sanidade firme mantém os demônios à distância."
            }
        });

        count += CreateFlavorText(folderPath, "Selection_Combat_LowHealth", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            condition = FlavorCondition.LowHealth,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> *Neste estado, seria suicídio...*",
                "> *As mãos tremem ao segurar a arma.*",
                "> *Mais uma batalha pode ser a última.*",
                "> *O corpo suplica por descanso antes de lutar.*"
            }
        });

        count += CreateFlavorText(folderPath, "Selection_Camp_LowHealth", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Camp,
            anyRoomType = false,
            condition = FlavorCondition.LowHealth,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> *Um lugar para descansar... preciso tanto disso.*",
                "> *Se eu conseguir chegar lá, posso me recuperar.*",
                "> *A esperança de descanso dá forças aos passos.*"
            }
        });

        // ═══════════════════════════════════════════════════
        // MORE GENERAL MOVEMENT PHRASES
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Movement_Normal_Extra", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            phrases = new List<string>
            {
                "> As pedras frias ecoam sob as botas.",
                "> A escuridão parece engolir a luz da tocha.",
                "> Teias de aranha adornam as paredes antigas.",
                "> O ar está pesado com cheiro de mofo.",
                "> Riscos na parede contam histórias de outros que passaram.",
                "> O caminho serpenteia para mais fundo na terra.",
                "> Ossos antigos jazem esquecidos nos cantos.",
                "> A umidade se infiltra através das rachaduras.",
                "> Sussurros de vento perdem-se nos corredores.",
                "> O eco dos passos é o único companheiro."
            }
        });

        count += CreateFlavorText(folderPath, "Selection_General_Extra", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            anyRoomType = true,
            phrases = new List<string>
            {
                "> Cada escolha é um passo para o desconhecido.",
                "> O que aguarda além desta porta?",
                "> A decisão pesa sobre os ombros.",
                "> Avançar ou recuar... a escolha é sua.",
                "> O caminho se divide. Qual trilha seguir?",
                "> A dungeon oferece múltiplas rotas. Todas perigosas."
            }
        });

        // ═══════════════════════════════════════════════════
        // AFFLICTION/VIRTUE GAINED
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Affliction_Gained", new FlavorConfig
        {
            context = FlavorContext.Affliction,
            anyRoomType = true,
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> *A escuridão encontrou uma brecha na mente.*",
                "> *Uma nova cicatriz marca a alma.*",
                "> *O peso da dungeon cobra seu preço mental.*",
                "> *A mente cede... mas se adapta.*",
                "> *Os demônios internos ganham força.*"
            }
        });

        count += CreateFlavorText(folderPath, "Virtue_Gained", new FlavorConfig
        {
            context = FlavorContext.Affliction, // Usando mesmo contexto
            anyRoomType = true,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> *Do caos, nasce clareza.*",
                "> *A mente se fortalece contra a adversidade.*",
                "> *Uma nova força desperta no interior.*",
                "> *Da escuridão, surge uma virtude.*",
                "> *O espírito encontra propósito renovado.*"
            }
        });

        // ═══════════════════════════════════════════════════
        // HEALING (Recovery)
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Healing_Normal", new FlavorConfig
        {
            context = FlavorContext.Healing,
            anyRoomType = true,
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> As feridas começam a se fechar.",
                "> Força retorna aos membros cansados.",
                "> O corpo agradece o momento de alívio.",
                "> Um suspiro de alívio escapa dos lábios.",
                "> A dor recua, ao menos por agora."
            }
        });

        // ═══════════════════════════════════════════════════
        // DEATH
        // ═══════════════════════════════════════════════════
        
        count += CreateFlavorText(folderPath, "Death_Normal", new FlavorConfig
        {
            context = FlavorContext.Death,
            anyRoomType = true,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> ═══ A ESCURIDÃO VENCE. ═══",
                "> ═══ O FIM CHEGOU. ═══",
                "> ═══ MAIS UM AVENTUREIRO PERDIDO NAS SOMBRAS. ═══",
                "> ═══ A DUNGEON COBRA SEU TRIBUTO. ═══",
                "> ═══ OS OSSOS SE JUNTAM AOS OUTROS NOS CORREDORES. ═══"
            }
        });

        // ═══════════════════════════════════════════════════════════════════
        //           AFFLICTIONS - COMPLETO PARA TODOS OS CONTEXTOS
        // ═══════════════════════════════════════════════════════════════════

        // ───────────── PARANÓICO ─────────────
        count += CreateFlavorText(folderPath, "Paranoico_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Paranóico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Paranóico) \"Eles estavam aqui. Eu sinto o cheiro deles.\"",
                "> (Paranóico) \"Não confiem em nada que virem aqui.\"",
                "> (Paranóico) *Olha para todos os cantos obsessivamente.*"
            }
        });

        count += CreateFlavorText(folderPath, "Paranoico_Damage", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Paranóico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Paranóico) \"EU SABIA! TODOS QUEREM ME MATAR!\"",
                "> (Paranóico) \"Era uma armadilha desde o início!\"",
                "> (Paranóico) *Grita acusações contra aliados invisíveis.*"
            }
        });

        // ───────────── IMPRUDENTE ─────────────
        count += CreateFlavorText(folderPath, "Imprudente_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Imprudente",
            phrases = new List<string>
            {
                "> (Imprudente) \"O que estamos esperando? Vamos destruir tudo!\"",
                "> (Imprudente) *Corre para dentro sem verificar perigos.*",
                "> (Imprudente) \"Primeiro entro, depois penso!\""
            }
        });

        count += CreateFlavorText(folderPath, "Imprudente_Damage", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Imprudente",
            phrases = new List<string>
            {
                "> (Imprudente) \"HA! Isso nem doeu!\"",
                "> (Imprudente) \"Sangue me deixa mais forte!\"",
                "> (Imprudente) *Ignora o ferimento e avança.*"
            }
        });

        count += CreateFlavorText(folderPath, "Imprudente_Select_Treasure", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Treasure,
            anyRoomType = false,
            requiredTrait = "Imprudente",
            phrases = new List<string>
            {
                "> (Imprudente) \"Armadilhas? Que armadilhas? Eu quero aquele tesouro!\"",
                "> (Imprudente) \"Saiam do caminho, o ouro é meu!\""
            }
        });

        // ───────────── HESITANTE ─────────────
        count += CreateFlavorText(folderPath, "Hesitante_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Hesitante",
            overrideMessageType = true,
            messageType = LogMessageType.Neutral,
            phrases = new List<string>
            {
                "> (Hesitante) \"Não sei se devíamos ter entrado aqui...\"",
                "> (Hesitante) *Para na porta, inseguro.*",
                "> (Hesitante) \"Ainda dá tempo de voltar... não dá?\""
            }
        });

        count += CreateFlavorText(folderPath, "Hesitante_Select_Combat", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Hesitante",
            overrideMessageType = true,
            messageType = LogMessageType.Neutral,
            phrases = new List<string>
            {
                "> (Hesitante) \"Combate? Tem CERTEZA que não há outro caminho?\"",
                "> (Hesitante) \"Talvez devêssemos planejar mais...\"",
                "> (Hesitante) *Seca o suor das mãos nervosamente.*"
            }
        });

        count += CreateFlavorText(folderPath, "Hesitante_Damage", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Hesitante",
            overrideMessageType = true,
            messageType = LogMessageType.Neutral,
            phrases = new List<string>
            {
                "> (Hesitante) \"E-eu disse que não devíamos...\"",
                "> (Hesitante) *Recua trêmulo.*"
            }
        });

        // ───────────── FRÁGIL ─────────────
        count += CreateFlavorText(folderPath, "Fragil_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Frágil",
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> (Frágil) *Apoia-se na parede para não cair.*",
                "> (Frágil) \"Só mais um pouco... aguente...\""
            }
        });

        count += CreateFlavorText(folderPath, "Fragil_Select_Camp", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Camp,
            anyRoomType = false,
            requiredTrait = "Frágil",
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> (Frágil) \"Por favor... um lugar para descansar...\"",
                "> (Frágil) *Olha para o acampamento com olhos esperançosos.*"
            }
        });

        // ───────────── GANANCIOSO ─────────────
        count += CreateFlavorText(folderPath, "Ganancioso_RoomEnter_Treasure", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            roomType = RoomType.Treasure,
            anyRoomType = false,
            requiredTrait = "Ganancioso",
            overrideMessageType = true,
            messageType = LogMessageType.Gain,
            phrases = new List<string>
            {
                "> (Ganancioso) \"MEUS PRECIOSOS! TODO MEU!\"",
                "> (Ganancioso) *Olhos brilham com a luz do ouro.*",
                "> (Ganancioso) \"Finalmente! Riqueza merecida!\""
            }
        });

        count += CreateFlavorText(folderPath, "Ganancioso_Movement", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Ganancioso",
            phrases = new List<string>
            {
                "> (Ganancioso) *Vasculha cada canto em busca de moedas.*",
                "> (Ganancioso) \"Há tesouros escondidos aqui. Eu sinto.\""
            }
        });

        count += CreateFlavorText(folderPath, "Ganancioso_Select_Combat", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Ganancioso",
            phrases = new List<string>
            {
                "> (Ganancioso) \"Inimigos mortos deixam espólios. Vamos.\""
            }
        });

        // ───────────── CLAUSTROFÓBICO ─────────────
        count += CreateFlavorText(folderPath, "Claustrofobico_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Claustrofóbico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Claustrofóbico) \"O espaço está diminuindo... está diminuindo!\"",
                "> (Claustrofóbico) *Respiração pesada e suor frio.*",
                "> (Claustrofóbico) \"Preciso de ar! ONDE ESTÁ A SAÍDA?!\""
            }
        });

        count += CreateFlavorText(folderPath, "Claustrofobico_Select", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            anyRoomType = true,
            requiredTrait = "Claustrofóbico",
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Claustrofóbico) \"As paredes... estão se aproximando?\"",
                "> (Claustrofóbico) \"Não quero entrar em mais corredores estreitos!\""
            }
        });

        // ═══════════════════════════════════════════════════════════════════
        //           VIRTUDES - COMPLETO PARA TODOS OS CONTEXTOS
        // ═══════════════════════════════════════════════════════════════════

        // ───────────── ESTOICO ─────────────
        count += CreateFlavorText(folderPath, "Estoico_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Estoico",
            phrases = new List<string>
            {
                "> (Estoico) \"O medo é uma ilusão. Nós controlamos a resposta.\"",
                "> (Estoico) *Avança com calma inabalável.*"
            }
        });

        count += CreateFlavorText(folderPath, "Estoico_Select_Boss", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Boss,
            anyRoomType = false,
            requiredTrait = "Estoico",
            phrases = new List<string>
            {
                "> (Estoico) \"O fim vem para todos. Encontremos o nosso com dignidade.\"",
                "> (Estoico) \"Seja o que for que nos aguarda, estamos prontos.\""
            }
        });

        count += CreateFlavorText(folderPath, "Estoico_Movement_LowSanity", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Estoico",
            condition = FlavorCondition.LowSanity,
            overrideMessageType = true,
            messageType = LogMessageType.Sanity,
            phrases = new List<string>
            {
                "> (Estoico) \"Mantenham o foco. Não cedam às visões.\"",
                "> (Estoico) \"A mente é mais forte que os demônios.\""
            }
        });

        // ───────────── VIGOROSO ─────────────
        count += CreateFlavorText(folderPath, "Vigoroso_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Vigoroso",
            phrases = new List<string>
            {
                "> (Vigoroso) \"Mais uma sala, mais uma vitória!\"",
                "> (Vigoroso) *Estala os dedos, pronto para ação.*"
            }
        });

        count += CreateFlavorText(folderPath, "Vigoroso_Select_Combat", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Vigoroso",
            phrases = new List<string>
            {
                "> (Vigoroso) \"Eu vou na frente. Ninguém me derruba.\"",
                "> (Vigoroso) \"Combate? Excelente. Estava precisando de exercício.\""
            }
        });

        count += CreateFlavorText(folderPath, "Vigoroso_Movement_LowHealth", new FlavorConfig
        {
            context = FlavorContext.Movement,
            anyRoomType = true,
            requiredTrait = "Vigoroso",
            condition = FlavorCondition.LowHealth,
            overrideMessageType = true,
            messageType = LogMessageType.Danger,
            phrases = new List<string>
            {
                "> (Vigoroso) \"Apenas arranhões. Continuem.\"",
                "> (Vigoroso) \"O corpo aguenta. A missão continua.\""
            }
        });

        // ───────────── LIGEIRO ─────────────
        count += CreateFlavorText(folderPath, "Ligeiro_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Ligeiro",
            phrases = new List<string>
            {
                "> (Ligeiro) *Desliza para dentro como uma sombra.*",
                "> (Ligeiro) \"Área segura... por enquanto.\""
            }
        });

        count += CreateFlavorText(folderPath, "Ligeiro_Select", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            anyRoomType = true,
            requiredTrait = "Ligeiro",
            phrases = new List<string>
            {
                "> (Ligeiro) \"Eu chegarei primeiro. Sigam quando for seguro.\"",
                "> (Ligeiro) \"A velocidade é nossa maior aliada aqui.\""
            }
        });

        // ───────────── ESTRATEGISTA ─────────────
        count += CreateFlavorText(folderPath, "Estrategista_RoomEnter", new FlavorConfig
        {
            context = FlavorContext.RoomEnter,
            anyRoomType = true,
            requiredTrait = "Estrategista",
            phrases = new List<string>
            {
                "> (Estrategista) \"Analisem o ambiente. Saídas, cobertura, ameaças.\"",
                "> (Estrategista) *Mapeia mentalmente cada detalhe.*"
            }
        });

        count += CreateFlavorText(folderPath, "Estrategista_Select_Combat", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Combat,
            anyRoomType = false,
            requiredTrait = "Estrategista",
            phrases = new List<string>
            {
                "> (Estrategista) \"Primeiro, observamos. Depois, atacamos.\"",
                "> (Estrategista) \"O plano está traçado. Executem.\""
            }
        });

        count += CreateFlavorText(folderPath, "Estrategista_Select_Boss", new FlavorConfig
        {
            context = FlavorContext.RoomSelection,
            roomType = RoomType.Boss,
            anyRoomType = false,
            requiredTrait = "Estrategista",
            phrases = new List<string>
            {
                "> (Estrategista) \"Esta é a batalha final. Cada movimento conta.\"",
                "> (Estrategista) \"Analisei as opções. Esta é nossa melhor chance.\""
            }
        });

        count += CreateFlavorText(folderPath, "Estrategista_Damage", new FlavorConfig
        {
            context = FlavorContext.Damage,
            anyRoomType = true,
            requiredTrait = "Estrategista",
            phrases = new List<string>
            {
                "> (Estrategista) \"Erro calculado. Ajustando táticas.\"",
                "> (Estrategista) \"Recuem e reagrupem!\""
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
