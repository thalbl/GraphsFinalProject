using System.Collections.Generic;
using UnityEngine;

// Enumeração dos tipos de sala possíveis
public enum RoomType { Spawn, Boss, Combat, Treasure, Camp, Event }

[System.Serializable]
public class RoomNode {
    public Vector2Int logicalPosition; // Posição na grade lógica (ex: 0,0)
    public List<RoomNode> connections;   // Salas conectadas
    public RoomType roomType;            // O que esta sala é?
    public Rect roomRect;                // Posição e tamanho no mundo físico
    public int distanceFromStart;       // Distância em "saltos" do Spawn
    public RoomNode parent;              // De qual sala viemos (para a Fase 4)

    // Referência para o objeto visual instanciado
    [System.NonSerialized] public GameObject visualInstance;

    // Referências para as linhas que conectam esta sala
    [System.NonSerialized] public LineRenderer[] connectionLines;

    // Uma sala é "folha" (sem saída) se tiver apenas 1 conexão
    public bool IsLeaf => connections.Count == 1;

    public RoomNode(Vector2Int pos) {
        logicalPosition = pos;
        connections = new List<RoomNode>();
        roomType = RoomType.Combat; // Padrão é combate
        distanceFromStart = 0;
    }

    // Helper para obter a posição central no mundo 3D (para visualização)
    public Vector3 GetWorldPosition() {
        return new Vector3(roomRect.center.x, roomRect.center.y, 0);
    }
}