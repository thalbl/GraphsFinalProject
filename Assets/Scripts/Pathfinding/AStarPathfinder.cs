using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classe estática utilitária que implementa o algoritmo A* para pathfinding no dungeon.
/// Calcula o caminho de menor custo entre duas salas baseado no tipo de custo escolhido.
/// </summary>
public static class AStarPathfinder
{
    /// <summary>
    /// Classe interna para armazenar informações de pathfinding de cada nó durante o A*.
    /// </summary>
    private class PathNode
    {
        public RoomNode room;          // A sala que este nó representa
        public PathNode parent;        // De onde viemos (para reconstruir o caminho)
        public float gCost;            // Custo do caminho do início até aqui
        public float hCost;            // Heurística (estimativa de distância até o fim)
        public float fCost => gCost + hCost; // Custo total (g + h)

        public PathNode(RoomNode room)
        {
            this.room = room;
        }
    }

    /// <summary>
    /// Encontra o caminho de menor custo entre duas salas usando A*.
    /// </summary>
    /// <param name="graph">O grafo do dungeon contendo as arestas e custos</param>
    /// <param name="start">Sala de início</param>
    /// <param name="end">Sala de destino</param>
    /// <param name="costType">Tipo de custo a ser otimizado (Health, Sanity, Time)</param>
    /// <param name="traits">Lista de traços do jogador (opcional, não implementado ainda)</param>
    /// <returns>Lista de RoomNode do início ao fim, ou lista vazia se não há caminho</returns>
    public static List<RoomNode> FindPath(DungeonGraph graph, RoomNode start, RoomNode end, CostType costType, List<Trait> traits = null)
    {
        // Validação de entrada
        if (graph == null || start == null || end == null)
        {
            Debug.LogWarning("AStarPathfinder: Parâmetros inválidos!");
            return new List<RoomNode>();
        }

        // Se início e fim são a mesma sala, retorna caminho de 1 nó
        if (start == end)
        {
            return new List<RoomNode> { start };
        }

        // Conjuntos do A*
        List<PathNode> openSet = new List<PathNode>();
        HashSet<RoomNode> closedSet = new HashSet<RoomNode>();
        Dictionary<RoomNode, PathNode> nodeMap = new Dictionary<RoomNode, PathNode>();

        // Cria o nó inicial
        PathNode startNode = new PathNode(start)
        {
            gCost = 0,
            hCost = CalculateHeuristic(start, end)
        };

        openSet.Add(startNode);
        nodeMap[start] = startNode;

        // Loop principal do A*
        while (openSet.Count > 0)
        {
            // Encontra o nó com menor fCost no openSet
            PathNode currentNode = GetLowestFCostNode(openSet);

            // Se chegamos no destino, reconstrói e retorna o caminho
            if (currentNode.room == end)
            {
                return ReconstructPath(currentNode);
            }

            // Move o nó atual do openSet para o closedSet
            openSet.Remove(currentNode);
            closedSet.Add(currentNode.room);

            // Examina todos os vizinhos
            List<RoomNode> neighbors = graph.GetNeighbors(currentNode.room);
            if (neighbors == null) continue;

            foreach (RoomNode neighbor in neighbors)
            {
                // Ignora vizinhos já processados
                if (closedSet.Contains(neighbor))
                    continue;

                // Obtém os dados da aresta (custos)
                EdgeData edgeData = graph.GetEdgeData(currentNode.room, neighbor);
                if (edgeData == null)
                {
                    Debug.LogWarning($"AStarPathfinder: Aresta {currentNode.room.logicalPosition} -> {neighbor.logicalPosition} não encontrada!");
                    continue;
                }

                // Calcula o custo baseado no tipo escolhido
                float movementCost = GetCostFromEdgeData(edgeData, costType);

                // TODO: Aplicar modificadores de Traços aqui
                // if (traits != null) {
                //     foreach (Trait trait in traits) {
                //         movementCost = ApplyTraitModifier(movementCost, trait, costType);
                //     }
                // }

                float tentativeGCost = currentNode.gCost + movementCost;

                // Verifica se já temos um PathNode para este vizinho
                PathNode neighborNode;
                if (!nodeMap.TryGetValue(neighbor, out neighborNode))
                {
                    // Cria novo PathNode se não existir
                    neighborNode = new PathNode(neighbor)
                    {
                        hCost = CalculateHeuristic(neighbor, end)
                    };
                    nodeMap[neighbor] = neighborNode;
                }

                // Se encontramos um caminho melhor para este vizinho
                if (!openSet.Contains(neighborNode) || tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.parent = currentNode;

                    if (!openSet.Contains(neighborNode))
                    {
                        openSet.Add(neighborNode);
                    }
                }
            }
        }

        // Se chegamos aqui, não há caminho possível
        Debug.LogWarning($"AStarPathfinder: Nenhum caminho encontrado de {start.logicalPosition} para {end.logicalPosition}");
        return new List<RoomNode>();
    }

    /// <summary>
    /// Calcula a heurística (estimativa de distância) entre duas salas.
    /// Usa a distância Euclidiana entre as posições físicas.
    /// </summary>
    private static float CalculateHeuristic(RoomNode from, RoomNode to)
    {
        return Vector2.Distance(from.roomRect.center, to.roomRect.center);
    }

    /// <summary>
    /// Extrai o custo correto do EdgeData baseado no CostType.
    /// </summary>
    private static float GetCostFromEdgeData(EdgeData data, CostType costType)
    {
        switch (costType)
        {
            case CostType.Health:
                return data.costHealth;
            case CostType.Sanity:
                return data.costSanity;
            case CostType.Time:
                return data.costTime;
            default:
                Debug.LogWarning($"AStarPathfinder: CostType desconhecido {costType}, usando Health");
                return data.costHealth;
        }
    }

    /// <summary>
    /// Encontra o nó com menor fCost na lista (busca linear simples).
    /// </summary>
    private static PathNode GetLowestFCostNode(List<PathNode> nodes)
    {
        PathNode lowest = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].fCost < lowest.fCost ||
                (nodes[i].fCost == lowest.fCost && nodes[i].hCost < lowest.hCost))
            {
                lowest = nodes[i];
            }
        }
        return lowest;
    }

    /// <summary>
    /// Reconstrói o caminho percorrendo os parents de volta até o início.
    /// </summary>
    private static List<RoomNode> ReconstructPath(PathNode endNode)
    {
        List<RoomNode> path = new List<RoomNode>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.room);
            currentNode = currentNode.parent;
        }

        path.Reverse(); // Inverte para ficar do início ao fim
        return path;
    }

    /// <summary>
    /// Calcula o custo total de um caminho baseado no tipo de custo.
    /// Útil para debug e exibição.
    /// </summary>
    public static float CalculatePathCost(DungeonGraph graph, List<RoomNode> path, CostType costType)
    {
        if (path == null || path.Count <= 1)
            return 0;

        float totalCost = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            EdgeData edgeData = graph.GetEdgeData(path[i], path[i + 1]);
            if (edgeData != null)
            {
                totalCost += GetCostFromEdgeData(edgeData, costType);
            }
        }

        return totalCost;
    }
}
