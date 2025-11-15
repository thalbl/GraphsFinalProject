using System.Collections.Generic;
using UnityEngine;

// Enumera��o dos tipos de sala poss�veis
public enum RoomType { Spawn, Boss, Combat, Treasure, Camp, Event }

[System.Serializable]
public class RoomNode {
    public Vector2Int logicalPosition; // Posi��o na grade l�gica (ex: 0,0)
    [System.NonSerialized] public List<RoomNode> connections;   // Salas conectadas
    public RoomType roomType;            // O que esta sala �?
    public Rect roomRect;                // Posi��o e tamanho no mundo f�sico
    public int distanceFromStart;       // Dist�ncia em "saltos" do Spawn
    [System.NonSerialized] public RoomNode parent;              // De qual sala viemos (para a Fase 4)

    // Refer�ncia para o objeto visual instanciado
    [System.NonSerialized] public GameObject visualInstance;

    // Refer�ncias para as linhas que conectam esta sala
    [System.NonSerialized] public LineRenderer[] connectionLines;

    // Uma sala � "folha" (sem sa�da) se tiver apenas 1 conex�o
    public bool IsLeaf => connections.Count == 1;

    public RoomNode(Vector2Int pos) {
        logicalPosition = pos;
        connections = new List<RoomNode>();
        roomType = RoomType.Combat; // Padr�o � combate
        distanceFromStart = 0;
    }

    // Helper para obter a posi��o central no mundo 3D (para visualiza��o)
    public Vector3 GetWorldPosition() {
        return new Vector3(roomRect.center.x, roomRect.center.y, 0);
    }
}