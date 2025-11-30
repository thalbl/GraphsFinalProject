using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Módulo responsável por gerar custos de arestas (Health, Sanity, Time).
/// Fase 6: Define custos com base em ranges e modificadores de tipo de sala.
/// </summary>
public class EdgeCostGenerator
{
    private System.Random prng;
    private Vector2 healthCostRange;
    private Vector2 sanityCostRange;
    private Vector2 timeCostRange;
    private bool directedGraph;

    public EdgeCostGenerator(System.Random randomGenerator, Vector2 healthRange, Vector2 sanityRange, Vector2 timeRange, bool directed)
    {
        this.prng = randomGenerator;
        this.healthCostRange = healthRange;
        this.sanityCostRange = sanityRange;
        this.timeCostRange = timeRange;
        this.directedGraph = directed;
    }

    /// <summary>
    /// Gera custos para todas as arestas do grafo
    /// </summary>
    public Dictionary<(RoomNode, RoomNode), EdgeData> GenerateEdgeCosts(List<RoomNode> allRooms, DungeonGraph dungeonGraph = null)
    {
        Dictionary<(RoomNode, RoomNode), EdgeData> edgeCosts = new Dictionary<(RoomNode, RoomNode), EdgeData>();

        // Adiciona nós ao DungeonGraph se fornecido
        if (dungeonGraph != null)
        {
            foreach (var node in allRooms)
            {
                dungeonGraph.AddNode(node);
            }
        }

        foreach (RoomNode fromRoom in allRooms)
        {
            foreach (RoomNode toRoom in fromRoom.connections)
            {
                // Custo de IDA
                float h1 = (float)(healthCostRange.x + prng.NextDouble() * (healthCostRange.y - healthCostRange.x));
                float s1 = (float)(sanityCostRange.x + prng.NextDouble() * (sanityCostRange.y - sanityCostRange.x));
                float t1 = (float)(timeCostRange.x + prng.NextDouble() * (timeCostRange.y - timeCostRange.x));

                // Modificadores baseados no tipo de sala de destino
                if (toRoom.roomType == RoomType.Boss)
                {
                    h1 *= 1.5f;
                    s1 *= 1.5f;
                }
                else if (toRoom.roomType == RoomType.Camp)
                {
                    s1 *= 0.7f; // Caminho para descanso dá esperança
                }

                EdgeData data1 = new EdgeData(h1, s1, t1, "Corredor");

                edgeCosts[(fromRoom, toRoom)] = data1;
                if (dungeonGraph != null) dungeonGraph.AddEdge(fromRoom, toRoom, data1);

                if (!directedGraph)
                {
                    // Grafo não-direcionado: mesmos custos ida e volta
                    edgeCosts[(toRoom, fromRoom)] = data1;
                    if (dungeonGraph != null) dungeonGraph.AddEdge(toRoom, fromRoom, data1);
                }
                else
                {
                    // Grafo direcionado: custos diferentes para volta
                    float h2 = (float)(healthCostRange.x + prng.NextDouble() * (healthCostRange.y - healthCostRange.x));
                    float s2 = (float)(sanityCostRange.x + prng.NextDouble() * (sanityCostRange.y - sanityCostRange.x));
                    float t2 = (float)(timeCostRange.x + prng.NextDouble() * (timeCostRange.y - timeCostRange.x));

                    if (fromRoom.roomType == RoomType.Boss)
                    {
                        h2 *= 1.5f;
                        s2 *= 1.5f;
                    }
                    else if (fromRoom.roomType == RoomType.Camp)
                    {
                        s2 *= 0.7f;
                    }

                    EdgeData data2 = new EdgeData(h2, s2, t2, "Corredor (Volta)");

                    edgeCosts[(toRoom, fromRoom)] = data2;
                    if (dungeonGraph != null) dungeonGraph.AddEdge(toRoom, fromRoom, data2);
                }
            }
        }

        Debug.Log($"[EdgeCostGenerator] Custos gerados para {edgeCosts.Count} arestas lógicas.");
        return edgeCosts;
    }
}
