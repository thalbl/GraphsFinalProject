using System.Collections.Generic;
using UnityEngine;


// O MonoBehaviour que gerencia o Grafo Lógico
public class DungeonGraph : MonoBehaviour
{
    // Dicionário que mapeia uma conexão DIRECIONADA (De -> Para) aos seus Custos
    // Usamos uma Tupla (RoomNode, RoomNode) como chave
    private Dictionary<(RoomNode, RoomNode), EdgeData> edgeMap;
    
    // Lista de todos os nós para acesso rápido
    private List<RoomNode> nodes;

    // Chamado pelo DungeonGenerator no início da geração
    public void Initialize()
    {
        edgeMap = new Dictionary<(RoomNode, RoomNode), EdgeData>();
        nodes = new List<RoomNode>();
    }

    // Registra uma sala no grafo
    public void AddNode(RoomNode node)
    {
        if (nodes == null) nodes = new List<RoomNode>();
        
        if (!nodes.Contains(node))
        {
            nodes.Add(node);
        }
    }

    // Registra uma aresta com seus custos (Chamado na Fase 5 do Generator)
    public void AddEdge(RoomNode from, RoomNode to, EdgeData data)
    {
        if (edgeMap == null) edgeMap = new Dictionary<(RoomNode, RoomNode), EdgeData>();

        // Armazena os dados usando o par (Origem, Destino) como chave única
        edgeMap[(from, to)] = data;
    }

    // Método vital para o A* (Pathfinding) e para o PlayerController
    // Retorna os custos para ir de A para B
    public EdgeData GetEdgeData(RoomNode from, RoomNode to)
    {
        if (edgeMap != null && edgeMap.TryGetValue((from, to), out EdgeData data))
        {
            return data;
        }
        // Se não encontrar, retorna null (significa que não há conexão direta lógica)
        return null; 
    }

    // Retorna vizinhos de um nó (Helper para o A*)
    public List<RoomNode> GetNeighbors(RoomNode node)
    {
        // Como a lista de conexões já está no RoomNode, apenas a repassamos
        return node.connections;
    }

    // Retorna todos os nós (Helper para inicialização/debug)
    public List<RoomNode> GetAllNodes()
    {
        return nodes ?? new List<RoomNode>();
    }
}