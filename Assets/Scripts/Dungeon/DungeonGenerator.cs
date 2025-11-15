using System.Collections.Generic;
using UnityEngine;

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
    public float minGapBetweenRooms = 2f; // Espa�o m�nimo entre salas

    [Header("Visual Settings")]
    public Material connectionMaterial; // Crie um material simples (Unlit/Color) no Unity
    public Color spawnColor = Color.green;
    public Color bossColor = Color.red;
    public Color combatColor = Color.white;
    public Color treasureColor = Color.yellow;
    public Color campColor = Color.blue;

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
    [SerializeField] private bool directedGraph = true; // O grafo é direcionado?
    
    [Header("Graph Complexity")]
    [SerializeField] private float additionalEdgeChance = 0.15f; // 15% de chance de adicionar aresta extra
    [SerializeField] private int maxAdditionalEdges = 5; // Máximo de arestas adicionais

    private Dictionary<(RoomNode, RoomNode), EdgeData> edgeCosts;

    [Header("System References")]
    public CameraController mainCamera;
    void Start() {
        if (generateOnStart) GenerateDungeon();
    }

    // Bot�o para o Inspector (opcional)
    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon() {
        // Limpa a dungeon anterior, se existir
        if (dungeonContainer != null) {
            // Use DestroyImmediate se estiver no Editor e n�o em Play Mode
            if (Application.isPlaying)
                Destroy(dungeonContainer);
            else
                DestroyImmediate(dungeonContainer);
        }

        dungeonContainer = new GameObject("Dungeon");

        // Inicializa
        if (randomSeed != 0) Random.InitState(randomSeed);
        occupiedPositions = new Dictionary<Vector2Int, RoomNode>();
        allRooms = new List<RoomNode>();

        edgeCosts = new Dictionary<(RoomNode, RoomNode), EdgeData>();

        // --- Fases da Gera��o ---
        GenerateRoomGraph();      // 1. Cria a estrutura (DFS)
        AddAdditionalEdges();     // 1.5. Adiciona arestas extras para complexidade
        AssignRoomTypes();        // 2. Define Spawn, Boss, Tesouro (Pacing)
        ApplyPhysicalLayout();    // 3. Define tamanhos e escala (Evita colis�o)
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
            ShuffleArray(directions); // Aleatoriza dire��es

            foreach (Vector2Int dir in directions) {
                Vector2Int newPos = currentRoom.logicalPosition + dir;

                if (!occupiedPositions.ContainsKey(newPos)) // Se a posi��o est� livre
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

    // Fase 1.5: Adiciona arestas extras para tornar o grafo mais complexo
    private void AddAdditionalEdges() {
        int edgesAdded = 0;
        int attempts = 0;
        int maxAttempts = allRooms.Count * 10; // Limite de tentativas

        while (edgesAdded < maxAdditionalEdges && attempts < maxAttempts) {
            attempts++;
            
            // Seleciona duas salas aleatórias
            RoomNode roomA = allRooms[Random.Range(0, allRooms.Count)];
            RoomNode roomB = allRooms[Random.Range(0, allRooms.Count)];
            
            // Não adiciona aresta se:
            // - São a mesma sala
            // - Já estão conectadas
            // - São adjacentes na grade (já devem estar conectadas pela árvore)
            if (roomA == roomB) continue;
            if (roomA.connections.Contains(roomB)) continue;
            
            Vector2Int diff = roomA.logicalPosition - roomB.logicalPosition;
            int manhattanDistance = Mathf.Abs(diff.x) + Mathf.Abs(diff.y);
            
            // Só adiciona arestas entre salas que não são adjacentes imediatas
            // e que estão a uma distância razoável (atalhos)
            if (manhattanDistance <= 1) continue;
            if (manhattanDistance > 4) continue; // Limita distância máxima
            
            // Chance baseada na distância (atalhos mais próximos são mais prováveis)
            float chance = additionalEdgeChance * (1f / manhattanDistance);
            if (Random.value > chance) continue;
            
            // Adiciona a conexão bidirecional
            roomA.connections.Add(roomB);
            roomB.connections.Add(roomA);
            edgesAdded++;
            
            Debug.Log($"Aresta adicional criada: {roomA.logicalPosition} <-> {roomB.logicalPosition} (distância: {manhattanDistance})");
        }
        
        Debug.Log($"Arestas adicionais criadas: {edgesAdded}");
    }

    // Fase 2: Pacing (Define Boss, etc.)
    private void AssignRoomTypes() {
        CalculateDistancesFromStart(); // Usa BFS para achar dist�ncias

        List<RoomNode> leafRooms = new List<RoomNode>();
        foreach (RoomNode room in allRooms) {
            if (room.IsLeaf && room != spawnRoom) {
                leafRooms.Add(room);
            }
        }

        if (leafRooms.Count > 0) {
            leafRooms.Sort((a, b) => b.distanceFromStart.CompareTo(a.distanceFromStart));
            bossRoom = leafRooms[0];
            bossRoom.roomType = RoomType.Boss;

            for (int i = 1; i < Mathf.Min(leafRooms.Count, 3); i++) {
                leafRooms[i].roomType = RoomType.Treasure;
            }
        }
        else {
            bossRoom = allRooms[allRooms.Count - 1];
            bossRoom.roomType = RoomType.Boss;
        }
    }

    // Fase 3: Define Tamanhos e Escala
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

    // Fase 4: "Gravidade" para layout mais natural
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

    private void GenerateEdgeCosts()
    {
        foreach (RoomNode fromRoom in allRooms)
        {
            // O 'connections' do seu RoomNode já armazena os vizinhos
            foreach (RoomNode toRoom in fromRoom.connections)
            {
                // Vamos criar uma aresta lógica para cada conexão visual
                
                // Custo de IDA (From -> To)
                float h1 = Random.Range(healthCostRange.x, healthCostRange.y);
                float s1 = Random.Range(sanityCostRange.x, sanityCostRange.y);
                float t1 = Random.Range(timeCostRange.x, timeCostRange.y);
                EdgeData data1 = new EdgeData(h1, s1, t1, "Corredor");
                
                // Adiciona a aresta ao nosso "banco de dados"
                edgeCosts[(fromRoom, toRoom)] = data1;

                if (!directedGraph)
                {
                    // Se o grafo NÃO for direcionado, a volta (To -> From)
                    // tem os mesmos custos da ida.
                    edgeCosts[(toRoom, fromRoom)] = data1;
                }
                else
                {
                    // Se o grafo É direcionado, a volta tem custos PRÓPRIOS
                    float h2 = Random.Range(healthCostRange.x, healthCostRange.y);
                    float s2 = Random.Range(sanityCostRange.x, sanityCostRange.y);
                    float t2 = Random.Range(timeCostRange.x, timeCostRange.y);
                    EdgeData data2 = new EdgeData(h2, s2, t2, "Corredor (Volta)");
                    
                    edgeCosts[(toRoom, fromRoom)] = data2;
                }
            }
        }
        Debug.Log($"Custos gerados para {edgeCosts.Count} arestas lógicas.");
    }

    // Fase 6: Instancia os objetos visuais
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

        // Cria as Linhas de Conex�o
        foreach (RoomNode room in allRooms) {
            room.connectionLines = new LineRenderer[room.connections.Count];

            for (int i = 0; i < room.connections.Count; i++) {
                RoomNode connectedRoom = room.connections[i];

                // Desenha todas as conexões (incluindo arestas adicionais)
                // Usa uma chave ordenada para evitar duplicatas
                bool shouldDraw = room.GetHashCode() <= connectedRoom.GetHashCode();
                
                if (shouldDraw) {
                    GameObject connectionGO = new GameObject($"Connection_{room.logicalPosition}_to_{connectedRoom.logicalPosition}");
                    connectionGO.transform.SetParent(dungeonContainer.transform);

                    LineRenderer lr = connectionGO.AddComponent<LineRenderer>();
                    lr.positionCount = 2;
                    lr.SetPosition(0, room.GetWorldPosition());
                    lr.SetPosition(1, connectedRoom.GetWorldPosition());
                    lr.startWidth = 0.2f;
                    lr.endWidth = 0.2f;
                    lr.material = connectionMaterial;
                    lr.sortingOrder = 0;
                    lr.startColor = Color.gray;
                    lr.endColor = Color.gray;

                    room.connectionLines[i] = lr;
                }
            }
        }
    }

    // --- M�todos Utilit�rios ---

    // Utilit�rio BFS para calcular dist�ncias
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
            _ => combatColor
        };
    }

    private void ShuffleArray(Vector2Int[] array) {
        for (int i = 0; i < array.Length; i++) {
            int randomIndex = Random.Range(i, array.Length);
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