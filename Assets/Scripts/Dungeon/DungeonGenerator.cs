using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DungeonGenerator : MonoBehaviour {

    public System.Action OnDungeonGenerated;

    [Header("Generation Settings")]
    public int maxRooms = 15;
    public int randomSeed = 0;
    public bool generateOnStart = true;

    [Header("Room Sizes")]
    public Vector2 spawnRoomSize = new Vector2(12, 12);
    public Vector2 bossRoomSize = new Vector2(16, 16);
    public Vector2 normalRoomSize = new Vector2(8, 8);
    public float minGapBetweenRooms = 2f; // Espaço mínimo entre salas

    [Header("Visual Settings")]
    public Material connectionMaterial; // Crie um material simples (Unlit/Color) no Unity
    public Color spawnColor = Color.green;
    public Color bossColor = Color.red;
    public Color combatColor = Color.white;
    public Color treasureColor = Color.yellow;
    public Color campColor = Color.blue;
    public Color eventColor = Color.magenta; // Nova cor para eventos

    // Dados gerados
    [HideInInspector] public List<RoomNode> allRooms;
    [HideInInspector] public RoomNode spawnRoom;
    [HideInInspector] public RoomNode bossRoom;

    private Dictionary<Vector2Int, RoomNode> occupiedPositions;
    private GameObject dungeonContainer; // Objeto pai para organizar a hierarquia

    [Header("Cost Settings")]
    [SerializeField] private Vector2 healthCostRange = new Vector2(1, 5);
    [SerializeField] private Vector2 sanityCostRange = new Vector2(1, 8);
    [SerializeField] private Vector2 timeCostRange = new Vector2(1, 3);
    public bool directedGraph = false; // O grafo é direcionado?
    
    [Header("Graph Complexity")]
    [Range(0f, 1f)] 
    public float cycleChance = 0.4f; // Chance de conectar salas vizinhas na grade (40% é alto, cria muitos ciclos)
    
    [Header("Room Distribution Chances")]
    [Range(0f, 1f)] public float treasureChanceInDeadEnd = 0.8f; // Alta chance de tesouro em becos sem saída
    [Range(0f, 1f)] public float treasureChanceInCorridor = 0.1f; // Baixa chance em corredores
    [Range(0f, 1f)] public float campChance = 0.15f; // Chance global de acampamento
    [Range(0f, 1f)] public float eventChance = 0.1f; // Chance global de evento

    private Dictionary<(RoomNode, RoomNode), EdgeData> edgeCosts;
    private System.Random prng; // Gerador de números aleatórios privado e controlado

    [Header("System References")]
    public CameraController mainCamera;
    public DungeonGraph dungeonGraph; // Referência adicionada conforme solicitado anteriormente
    void Start() {
        if (generateOnStart) GenerateDungeon();
    }

    // Botão para o Inspector (opcional)
    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon() {
        // Inicializa o gerador de números aleatórios PRIVADO
        prng = new System.Random(randomSeed);

        // Limpa a dungeon anterior, se existir
        if (dungeonContainer != null) {
            // Use DestroyImmediate se estiver no Editor e não em Play Mode
            if (Application.isPlaying)
                Destroy(dungeonContainer);
            else
                DestroyImmediate(dungeonContainer);
        }

        dungeonContainer = new GameObject("Dungeon");

        // Inicializa
        occupiedPositions = new Dictionary<Vector2Int, RoomNode>();
        allRooms = new List<RoomNode>();

        edgeCosts = new Dictionary<(RoomNode, RoomNode), EdgeData>();

        if (dungeonGraph != null) dungeonGraph.Initialize();

        // --- Pipeline de Geração ---
        // 1. Estrutura Base (Árvore) ---
        GenerateRoomGraph();      // 1. Cria a estrutura (DFS)
        AddCycles();     // 2. Adiciona ciclos e conexões extras
        AssignRoomTypes();        // 2. Define Spawn, Boss, Tesouro (Pacing)
        ApplyPhysicalLayout();    // 3. Define tamanhos e escala (Evita colisão)
        NaturalizeLayout();       // 4. Torna menos "quadrado" (Gravidade)
        GenerateEdgeCosts();      // Fase 5 (Custos) Aplicar custos nas arestas
        CreateVisualRepresentation(); // 6. Desenha na tela

        Debug.Log($"Dungeon gerado: {allRooms.Count} salas.");

        OnDungeonGenerated?.Invoke();
    }

    // Fase 1: Random Walk (DFS) em Grade
    private void GenerateRoomGraph() {
        Vector2Int startPos = Vector2Int.zero;
        spawnRoom = new RoomNode(startPos);
        spawnRoom.roomType = RoomType.Spawn;

        occupiedPositions.Add(startPos, spawnRoom);
        allRooms.Add(spawnRoom);

        Stack<RoomNode> stack = new Stack<RoomNode>();
        stack.Push(spawnRoom);

        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        while (stack.Count > 0 && allRooms.Count < maxRooms) {
            RoomNode currentRoom = stack.Pop();
            ShuffleArray(directions); // Aleatoriza direções

            foreach (Vector2Int dir in directions) {
                Vector2Int newPos = currentRoom.logicalPosition + dir;

                if (!occupiedPositions.ContainsKey(newPos)) // Se a posição está livre
                {
                    RoomNode newRoom = new RoomNode(newPos);
                    newRoom.parent = currentRoom;

                    currentRoom.connections.Add(newRoom);
                    newRoom.connections.Add(currentRoom);

                    occupiedPositions.Add(newPos, newRoom);
                    allRooms.Add(newRoom);
                    stack.Push(newRoom);

                    if (allRooms.Count >= maxRooms) break;
                }
            }
        }
    }

    // Fase 2: Adicionar Ciclos (Arestas Extras Inteligentes)
    private void AddCycles() {
        int cyclesAdded = 0;
        
        // Direções estendidas: Cardeais de distância 1 e 2, e Diagonais
        // Isso permite conectar salas que estão "dobrando a esquina" ou paralelas
        Vector2Int[] checkOffsets = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1), // Dist 1
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), // Diagonal
            new Vector2Int(2, 0), new Vector2Int(-2, 0), new Vector2Int(0, 2), new Vector2Int(0, -2)  // Dist 2 (pula uma casa)
        };

        foreach (RoomNode room in allRooms) {
            foreach (Vector2Int offset in checkOffsets) {
                Vector2Int neighborPos = room.logicalPosition + offset;
                
                // Se existe uma sala vizinha nessa posição
                if (occupiedPositions.TryGetValue(neighborPos, out RoomNode neighbor)) {
                    
                    // Evita conectar consigo mesmo ou conexões já existentes
                    if (room == neighbor) continue;
                    if (room.connections.Contains(neighbor)) continue;
                    
                    // Evita conectar diretamente com o pai imediato (já conectado pelo DFS)
                    if (room.parent == neighbor || neighbor.parent == room) continue;
                    
                    // Rola o dado para criar o ciclo
                    if (prng.NextDouble() < cycleChance) {
                        room.connections.Add(neighbor);
                        
                        // Se quisermos ciclos bidirecionais fortes:
                        if (!neighbor.connections.Contains(room)) {
                            neighbor.connections.Add(room);
                        }
                        
                        cyclesAdded++;
                    }
                }
            }
        }

        Debug.Log($"Ciclos/Arestas Extras adicionadas: {cyclesAdded}");
    }

    // Fase 3: Distribuição de Tipos de Sala (Pacing Melhorado)
    private void AssignRoomTypes() {
        CalculateDistancesFromStart(); // Usa BFS para achar distâncias

        // 1. Encontrar Boss (Folha mais distante)
        List<RoomNode> sortedRooms = allRooms.OrderByDescending(r => r.distanceFromStart).ToList();
        
        // Tenta pegar uma folha longe, senão pega a sala mais longe qualquer
        RoomNode bossCandidate = sortedRooms.FirstOrDefault(r => r.IsLeaf && r != spawnRoom) ?? sortedRooms.First(r => r != spawnRoom);
        
        bossRoom = bossCandidate;
        bossRoom.roomType = RoomType.Boss;

        // 2. Separar o resto em "Becos sem Saída" e "Corredores"
        List<RoomNode> availableRooms = allRooms.Where(r => r != spawnRoom && r != bossRoom).ToList();
        
        List<RoomNode> deadEnds = availableRooms.Where(r => r.IsLeaf).ToList();
        List<RoomNode> corridors = availableRooms.Where(r => !r.IsLeaf).ToList();

        // 3. Preencher Becos sem Saída (Prioridade para Tesouros)
        foreach (RoomNode room in deadEnds) {
            if (prng.NextDouble() < treasureChanceInDeadEnd) {
                room.roomType = RoomType.Treasure;
            } else if (prng.NextDouble() < campChance) {
                room.roomType = RoomType.Camp;
            } else {
                room.roomType = RoomType.Combat; // Becos sem saída perigosos
            }
        }

        // 4. Preencher Corredores (O "Meio" do Grafo)
        foreach (RoomNode room in corridors) {
            double roll = prng.NextDouble();

            if (roll < treasureChanceInCorridor) {
                room.roomType = RoomType.Treasure; // Raro, mas possível achar tesouro no caminho
            }
            else if (roll < treasureChanceInCorridor + campChance) {
                room.roomType = RoomType.Camp; // Bom lugar para descansar no meio da jornada
            }
            else if (roll < treasureChanceInCorridor + campChance + eventChance) {
                room.roomType = RoomType.Event; // Evento aleatório
            }
            else {
                room.roomType = RoomType.Combat; // Padrão
            }
        }

        Debug.Log($"Boss colocado em distância {bossRoom.distanceFromStart}. Becos: {deadEnds.Count}, Corredores: {corridors.Count}");
    }

    // Fase 4: Define Tamanhos e Escala
    private void ApplyPhysicalLayout() {
        foreach (RoomNode room in allRooms) {
            room.roomRect.size = room.roomType switch {
                RoomType.Spawn => spawnRoomSize,
                RoomType.Boss => bossRoomSize,
                _ => normalRoomSize
            };
        }

        float graphScale = 15f;
        bool collisionFound;
        int protection = 0; // Evita loop infinito no editor

        do {
            collisionFound = false;
            protection++;

            foreach (RoomNode room in allRooms) {
                room.roomRect.center = (Vector2)room.logicalPosition * graphScale;

            }

            for (int i = 0; i < allRooms.Count; i++) {
                for (int j = i + 1; j < allRooms.Count; j++) {
                    Rect gapRect = ExpandRect(allRooms[i].roomRect, minGapBetweenRooms);
                    if (gapRect.Overlaps(allRooms[j].roomRect)) {
                        collisionFound = true;
                        break;
                    }
                }
                if (collisionFound) break;
            }

            if (collisionFound) graphScale += 2f;

        } while (collisionFound && protection < 100);
    }

    // Fase 5: "Gravidade" para layout mais natural
    private void NaturalizeLayout() {
        float moveStep = 0.5f;
        Vector2 spawnCenter = spawnRoom.roomRect.center;

        for (int iteration = 0; iteration < 50; iteration++) {
            bool moved = false;
            foreach (RoomNode room in allRooms) {
                if (room == spawnRoom) continue;

                Vector2 originalPos = room.roomRect.center;
                Vector2 moveDir = (spawnCenter - originalPos).normalized;
                Vector2 newPos = originalPos + moveDir * moveStep;

                if (!CausesCollision(room, newPos) && MaintainsAlignment(room, newPos)) {
                    room.roomRect.center = newPos;
                    moved = true;
                }
            }
            if (!moved) break;
        }
    }

    // Fase 6: Custos do Grafo
    private void GenerateEdgeCosts() {
        // Se tivermos a referência do MonoBehaviour DungeonGraph, adicionamos os nós lá
        if (dungeonGraph != null) {
            foreach (var node in allRooms) dungeonGraph.AddNode(node);
        }

        foreach (RoomNode fromRoom in allRooms) {
            foreach (RoomNode toRoom in fromRoom.connections) {
                
                // Custo de IDA
                float h1 = (float)(healthCostRange.x + prng.NextDouble() * (healthCostRange.y - healthCostRange.x));
                float s1 = (float)(sanityCostRange.x + prng.NextDouble() * (sanityCostRange.y - sanityCostRange.x));
                float t1 = (float)(timeCostRange.x + prng.NextDouble() * (timeCostRange.y - timeCostRange.x));
                
                // Modificadores baseados no tipo de sala de destino (Lógica de Gameplay)
                if (toRoom.roomType == RoomType.Boss) {
                    h1 *= 1.5f; s1 *= 1.5f; // Caminho para o boss é mais árduo
                } else if (toRoom.roomType == RoomType.Camp) {
                    s1 *= 0.7f; // Caminho para descanso dá esperança (menos sanidade)
                }
                
                EdgeData data1 = new EdgeData(h1, s1, t1, "Corredor");
                
                edgeCosts[(fromRoom, toRoom)] = data1;
                if (dungeonGraph != null) dungeonGraph.AddEdge(fromRoom, toRoom, data1);

                if (!directedGraph) {
                    edgeCosts[(toRoom, fromRoom)] = data1;
                    if (dungeonGraph != null) dungeonGraph.AddEdge(toRoom, fromRoom, data1);
                } else {
                    // Custo de VOLTA diferente
                    float h2 = (float)(healthCostRange.x + prng.NextDouble() * (healthCostRange.y - healthCostRange.x));
                    float s2 = (float)(sanityCostRange.x + prng.NextDouble() * (sanityCostRange.y - sanityCostRange.x));
                    float t2 = (float)(timeCostRange.x + prng.NextDouble() * (timeCostRange.y - timeCostRange.x));
                    
                    // Modificadores também para a volta
                    if (fromRoom.roomType == RoomType.Boss) {
                        h2 *= 1.5f; s2 *= 1.5f;
                    } else if (fromRoom.roomType == RoomType.Camp) {
                        s2 *= 0.7f;
                    }
                    
                    EdgeData data2 = new EdgeData(h2, s2, t2, "Corredor (Volta)");
                    
                    edgeCosts[(toRoom, fromRoom)] = data2;
                    if (dungeonGraph != null) dungeonGraph.AddEdge(toRoom, fromRoom, data2);
                }
            }
        }
        Debug.Log($"Custos gerados para {edgeCosts.Count} arestas lógicas.");
    }

    // Fase 7: Instancia os objetos visuais
    private void CreateVisualRepresentation() {
        foreach (RoomNode room in allRooms) {
            GameObject roomGO = new GameObject($"Room_{room.logicalPosition}");
            roomGO.transform.SetParent(dungeonContainer.transform);
            roomGO.transform.position = room.GetWorldPosition();

            SpriteRenderer sr = roomGO.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRoomSprite(room.roomRect.size);
            sr.color = GetRoomColor(room);
            sr.sortingOrder = 1;

            BoxCollider2D collider = roomGO.AddComponent<BoxCollider2D>();
            collider.size = room.roomRect.size;

            room.visualInstance = roomGO;

            RoomVisual roomVisual = roomGO.AddComponent<RoomVisual>();
            roomVisual.Initialize(room);
        }

        // Cria as Linhas de Conexão
        foreach (RoomNode room in allRooms) {
            room.connectionLines = new LineRenderer[room.connections.Count];

            for (int i = 0; i < room.connections.Count; i++) {
                RoomNode connectedRoom = room.connections[i];

                // Como é um dígrafo, desenhamos setas ou linhas para todas as conexões de saída.
                // Se quiser limpar o visual e evitar linha dupla sobreposta visualmente (embora logicamente existam 2),
                // podemos manter a checagem de HashCode APENAS PARA O DESENHO.
                // Mas para ver os ciclos, desenhar tudo é melhor, talvez com um pequeno offset.
                
                // Usa uma chave ordenada para evitar duplicatas visuais
                bool shouldDraw = room.GetHashCode() <= connectedRoom.GetHashCode();
                
                if (shouldDraw) {
                    GameObject connectionGO = new GameObject($"Connection_{room.logicalPosition}->{connectedRoom.logicalPosition}");
                    connectionGO.transform.SetParent(dungeonContainer.transform);

                    LineRenderer lr = connectionGO.AddComponent<LineRenderer>();
                    lr.positionCount = 2;
                    
                    // Pequeno offset para ver arestas de ida e volta separadas visualmente
                    Vector3 offset = Vector3.zero;
                    if (directedGraph && connectedRoom.connections.Contains(room)) {
                        Vector3 dir = (connectedRoom.GetWorldPosition() - room.GetWorldPosition()).normalized;
                        Vector3 perp = new Vector3(-dir.y, dir.x, 0) * 0.2f; // Desloca um pouco para o lado
                        offset = perp;
                    }
                    
                    lr.SetPosition(0, room.GetWorldPosition() + offset);
                    lr.SetPosition(1, connectedRoom.GetWorldPosition() + offset);
                    lr.startWidth = 0.15f;
                    lr.endWidth = 0.15f;
                    lr.material = connectionMaterial;
                    lr.sortingOrder = 0;
                    
                    // Cor da linha baseada no custo? (Opcional futuro)
                    lr.startColor = Color.gray;
                    lr.endColor = Color.gray;

                    room.connectionLines[i] = lr;
                }
            }
        }
    }

    // --- Métodos Utilitários ---

    // Utilitário BFS para calcular distâncias
    private void CalculateDistancesFromStart() {
        Queue<RoomNode> queue = new Queue<RoomNode>();
        Dictionary<RoomNode, bool> visited = new Dictionary<RoomNode, bool>();

        foreach (var room in allRooms) visited[room] = false;

        queue.Enqueue(spawnRoom);
        visited[spawnRoom] = true;
        spawnRoom.distanceFromStart = 0;

        while (queue.Count > 0) {
            RoomNode current = queue.Dequeue();
            foreach (RoomNode neighbor in current.connections) {
                if (!visited[neighbor]) {
                    visited[neighbor] = true;
                    neighbor.distanceFromStart = current.distanceFromStart + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private Sprite CreateRoomSprite(Vector2 size) {
        Texture2D texture = new Texture2D((int)size.x, (int)size.y);
        Color[] colors = new Color[texture.width * texture.height];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 1f);
    }

    public Color GetRoomColor(RoomNode room) {
        return room.roomType switch {
            RoomType.Spawn => spawnColor,
            RoomType.Boss => bossColor,
            RoomType.Treasure => treasureColor,
            RoomType.Camp => campColor,
            RoomType.Event => eventColor,
            _ => combatColor
        };
    }

    private void ShuffleArray(Vector2Int[] array) {
        for (int i = 0; i < array.Length; i++) {
            int randomIndex = prng.Next(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }

    private Rect ExpandRect(Rect original, float expansion) {
        return new Rect(
            original.x - expansion,
            original.y - expansion,
            original.width + (expansion * 2),
            original.height + (expansion * 2)
        );
    }

    private bool CausesCollision(RoomNode movingRoom, Vector2 newPosition) {
        Rect testRect = movingRoom.roomRect;
        testRect.center = newPosition;
        testRect = ExpandRect(testRect, minGapBetweenRooms);

        foreach (RoomNode otherRoom in allRooms) {
            if (otherRoom != movingRoom && testRect.Overlaps(otherRoom.roomRect))
                return true;
        }
        return false;
    }

    private bool MaintainsAlignment(RoomNode room, Vector2 newPosition) {
        if (room.parent == null) return true;
        Vector2 parentPos = room.parent.roomRect.center;

        bool horizontallyAligned = Mathf.Abs(newPosition.x - parentPos.x) < 0.1f;
        bool verticallyAligned = Mathf.Abs(newPosition.y - parentPos.y) < 0.1f;

        return horizontallyAligned || verticallyAligned;
    }

    public EdgeData GetEdgeCost(RoomNode from, RoomNode to)
    {
        if (edgeCosts.TryGetValue((from, to), out EdgeData data))
        {
            return data;
        }
        return null; // Não existe aresta lógica
    }
    
}