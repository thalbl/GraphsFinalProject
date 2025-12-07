using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Módulo responsável pelo posicionamento físico das salas no espaço 2D.
/// Fase 4-5: Escala, detecção de colisão e naturalização do layout.
/// </summary>
public class DungeonLayoutManager
{
    private Vector2 spawnRoomSize;
    private Vector2 bossRoomSize;
    private Vector2 normalRoomSize;
    private float minGapBetweenRooms;

    public DungeonLayoutManager(Vector2 spawnSize, Vector2 bossSize, Vector2 normalSize, float minGap)
    {
        this.spawnRoomSize = spawnSize;
        this.bossRoomSize = bossSize;
        this.normalRoomSize = normalSize;
        this.minGapBetweenRooms = minGap;
    }

    /// <summary>
    /// Define tamanhos de salas e aplica escala para evitar colisões
    /// </summary>
    public void ApplyPhysicalLayout(List<RoomNode> allRooms)
    {
        // Define tamanhos baseados no tipo
        foreach (RoomNode room in allRooms)
        {
            room.roomRect.size = room.roomType switch
            {
                RoomType.Spawn => spawnRoomSize,
                RoomType.Boss => bossRoomSize,
                _ => normalRoomSize
            };
        }

        float graphScale = 15f;
        bool collisionFound;
        int protection = 0;

        do
        {
            collisionFound = false;
            protection++;

            foreach (RoomNode room in allRooms)
            {
                room.roomRect.center = (Vector2)room.logicalPosition * graphScale;
            }

            for (int i = 0; i < allRooms.Count; i++)
            {
                for (int j = i + 1; j < allRooms.Count; j++)
                {
                    Rect gapRect = ExpandRect(allRooms[i].roomRect, minGapBetweenRooms);
                    if (gapRect.Overlaps(allRooms[j].roomRect))
                    {
                        collisionFound = true;
                        break;
                    }
                }
                if (collisionFound) break;
            }

            if (collisionFound) graphScale += 2f;

        } while (collisionFound && protection < 100);

        Debug.Log($"[LayoutManager] Layout aplicado com escala {graphScale}");
    }

    /// <summary>
    /// Aplica "gravidade" para tornar layout mais natural e compacto
    /// </summary>
    public void NaturalizeLayout(List<RoomNode> allRooms, RoomNode spawnRoom)
    {
        float moveStep = 0.5f;
        Vector2 spawnCenter = spawnRoom.roomRect.center;

        for (int iteration = 0; iteration < 50; iteration++)
        {
            bool moved = false;
            foreach (RoomNode room in allRooms)
            {
                if (room == spawnRoom) continue;

                Vector2 originalPos = room.roomRect.center;
                Vector2 moveDir = (spawnCenter - originalPos).normalized;
                Vector2 newPos = originalPos + moveDir * moveStep;

                if (!CausesCollision(room, newPos, allRooms) && MaintainsAlignment(room, newPos))
                {
                    room.roomRect.center = newPos;
                    moved = true;
                }
            }
            if (!moved) break;
        }

        Debug.Log("[LayoutManager] Layout naturalizado");
    }

    /// <summary>
    /// Expande um Rect por uma margem
    /// </summary>
    private Rect ExpandRect(Rect original, float expansion)
    {
        return new Rect(
            original.x - expansion,
            original.y - expansion,
            original.width + (expansion * 2),
            original.height + (expansion * 2)
        );
    }

    /// <summary>
    /// Verifica se mover uma sala causa colisão
    /// </summary>
    private bool CausesCollision(RoomNode movingRoom, Vector2 newPosition, List<RoomNode> allRooms)
    {
        Rect testRect = movingRoom.roomRect;
        testRect.center = newPosition;
        testRect = ExpandRect(testRect, minGapBetweenRooms);

        foreach (RoomNode otherRoom in allRooms)
        {
            if (otherRoom != movingRoom && testRect.Overlaps(otherRoom.roomRect))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Verifica se a sala mantém alinhamento horizontal ou vertical com o pai
    /// </summary>
    private bool MaintainsAlignment(RoomNode room, Vector2 newPosition)
    {
        if (room.parent == null) return true;
        Vector2 parentPos = room.parent.roomRect.center;

        bool horizontallyAligned = Mathf.Abs(newPosition.x - parentPos.x) < 0.1f;
        bool verticallyAligned = Mathf.Abs(newPosition.y - parentPos.y) < 0.1f;

        return horizontallyAligned || verticallyAligned;
    }
}
