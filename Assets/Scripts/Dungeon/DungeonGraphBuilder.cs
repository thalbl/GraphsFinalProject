using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Módulo responsável pela geração da estrutura do grafo de salas.
/// Fase 1-2: Cria a árvore base (DFS) e adiciona ciclos.
/// </summary>
public class DungeonGraphBuilder
{
    private System.Random prng;
    private int maxRooms;
    private float cycleChance;
    
    public DungeonGraphBuilder(System.Random randomGenerator, int maxRooms, float cycleChance)
    {
        this.prng = randomGenerator;
        this.maxRooms = maxRooms;
        this.cycleChance = cycleChance;
    }

    /// <summary>
    /// Gera o grafo de salas usando DFS (Depth-First Search)
    /// </summary>
    public RoomNode GenerateRoomGraph(out Dictionary<Vector2Int, RoomNode> occupiedPositions, out List<RoomNode> allRooms)
    {
        occupiedPositions = new Dictionary<Vector2Int, RoomNode>();
        allRooms = new List<RoomNode>();

        Vector2Int startPos = Vector2Int.zero;
        RoomNode spawnRoom = new RoomNode(startPos);
        spawnRoom.roomType = RoomType.Spawn;

        occupiedPositions.Add(startPos, spawnRoom);
        allRooms.Add(spawnRoom);

        Stack<RoomNode> stack = new Stack<RoomNode>();
        stack.Push(spawnRoom);

        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        while (stack.Count > 0 && allRooms.Count < maxRooms)
        {
            RoomNode currentRoom = stack.Pop();
            ShuffleArray(directions);

            foreach (Vector2Int dir in directions)
            {
                Vector2Int newPos = currentRoom.logicalPosition + dir;

                if (!occupiedPositions.ContainsKey(newPos))
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

        return spawnRoom;
    }

    /// <summary>
    /// Adiciona ciclos ao grafo conectando salas vizinhas
    /// </summary>
    public int AddCycles(List<RoomNode> allRooms, Dictionary<Vector2Int, RoomNode> occupiedPositions)
    {
        int cyclesAdded = 0;

        Vector2Int[] checkOffsets = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1), // Dist 1
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), // Diagonal
            new Vector2Int(2, 0), new Vector2Int(-2, 0), new Vector2Int(0, 2), new Vector2Int(0, -2)  // Dist 2
        };

        foreach (RoomNode room in allRooms)
        {
            foreach (Vector2Int offset in checkOffsets)
            {
                Vector2Int neighborPos = room.logicalPosition + offset;

                if (occupiedPositions.TryGetValue(neighborPos, out RoomNode neighbor))
                {
                    if (room == neighbor) continue;
                    if (room.connections.Contains(neighbor)) continue;
                    if (room.parent == neighbor || neighbor.parent == room) continue;

                    if (prng.NextDouble() < cycleChance)
                    {
                        room.connections.Add(neighbor);

                        if (!neighbor.connections.Contains(room))
                        {
                            neighbor.connections.Add(room);
                        }

                        cyclesAdded++;
                    }
                }
            }
        }

        Debug.Log($"[GraphBuilder] Ciclos/Arestas Extras adicionadas: {cyclesAdded}");
        return cyclesAdded;
    }

    /// <summary>
    /// Calcula distâncias de todas as salas a partir do spawn usando BFS
    /// </summary>
    public void CalculateDistancesFromStart(RoomNode spawnRoom, List<RoomNode> allRooms)
    {
        Queue<RoomNode> queue = new Queue<RoomNode>();
        Dictionary<RoomNode, bool> visited = new Dictionary<RoomNode, bool>();

        foreach (var room in allRooms) visited[room] = false;

        queue.Enqueue(spawnRoom);
        visited[spawnRoom] = true;
        spawnRoom.distanceFromStart = 0;

        while (queue.Count > 0)
        {
            RoomNode current = queue.Dequeue();
            foreach (RoomNode neighbor in current.connections)
            {
                if (!visited[neighbor])
                {
                    visited[neighbor] = true;
                    neighbor.distanceFromStart = current.distanceFromStart + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// Embaralha um array usando Fisher-Yates shuffle
    /// </summary>
    private void ShuffleArray(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }
}
