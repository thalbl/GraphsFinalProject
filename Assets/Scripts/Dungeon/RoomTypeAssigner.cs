using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// Módulo responsável por atribuir tipos às salas (Spawn, Boss, Treasure, Camp, Event, Combat).
/// Fase 3: Distribuição inteligente baseada em pacing e estrutura do grafo.
/// </summary>
public class RoomTypeAssigner
{
    private System.Random prng;
    private float treasureChanceInDeadEnd;
    private float treasureChanceInCorridor;
    private float campChance;
    private float eventChance;

    public RoomTypeAssigner(System.Random randomGenerator, float treasureDeadEnd, float treasureCorridor, float camp, float evt)
    {
        this.prng = randomGenerator;
        this.treasureChanceInDeadEnd = treasureDeadEnd;
        this.treasureChanceInCorridor = treasureCorridor;
        this.campChance = camp;
        this.eventChance = evt;
    }

    /// <summary>
    /// Atribui tipos às salas baseado em heurísticas de pacing
    /// </summary>
    public RoomNode AssignRoomTypes(List<RoomNode> allRooms, RoomNode spawnRoom)
    {
        // 1. Encontrar Boss (Folha mais distante)
        List<RoomNode> sortedRooms = allRooms.OrderByDescending(r => r.distanceFromStart).ToList();

        RoomNode bossCandidate = sortedRooms.FirstOrDefault(r => r.IsLeaf && r != spawnRoom) 
                                 ?? sortedRooms.First(r => r != spawnRoom);

        RoomNode bossRoom = bossCandidate;
        bossRoom.roomType = RoomType.Boss;

        // 2. Separar em becos sem saída e corredores
        List<RoomNode> availableRooms = allRooms.Where(r => r != spawnRoom && r != bossRoom).ToList();

        List<RoomNode> deadEnds = availableRooms.Where(r => r.IsLeaf).ToList();
        List<RoomNode> corridors = availableRooms.Where(r => !r.IsLeaf).ToList();

        // 3. Preencher becos sem saída (prioridade para tesouros)
        foreach (RoomNode room in deadEnds)
        {
            if (prng.NextDouble() < treasureChanceInDeadEnd)
            {
                room.roomType = RoomType.Treasure;
            }
            else if (prng.NextDouble() < campChance)
            {
                room.roomType = RoomType.Camp;
            }
            else
            {
                room.roomType = RoomType.Combat;
            }
        }

        // 4. Preencher corredores
        foreach (RoomNode room in corridors)
        {
            double roll = prng.NextDouble();

            if (roll < treasureChanceInCorridor)
            {
                room.roomType = RoomType.Treasure;
            }
            else if (roll < treasureChanceInCorridor + campChance)
            {
                room.roomType = RoomType.Camp;
            }
            else if (roll < treasureChanceInCorridor + campChance + eventChance)
            {
                room.roomType = RoomType.Event;
            }
            else
            {
                room.roomType = RoomType.Combat;
            }
        }

        Debug.Log($"[RoomTypeAssigner] Boss em distância {bossRoom.distanceFromStart}. Becos: {deadEnds.Count}, Corredores: {corridors.Count}");
        return bossRoom;
    }
}
