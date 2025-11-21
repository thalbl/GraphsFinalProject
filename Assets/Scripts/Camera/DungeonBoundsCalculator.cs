using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Calcula os limites (bounds) da dungeon
/// </summary>
public class DungeonBoundsCalculator {
    private float boundsPadding;

    public DungeonBoundsCalculator(float padding) {
        boundsPadding = padding;
    }

    /// <summary>
    /// Calcula os bounds da dungeon baseado nas salas
    /// </summary>
    public Bounds CalculateBounds(List<RoomNode> rooms, float padding = 0f) {
        if (rooms == null || rooms.Count == 0) {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        float actualPadding = padding > 0f ? padding : boundsPadding;

        // Encontrar os limites extremos da dungeon
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (RoomNode room in rooms) {
            Rect roomRect = room.roomRect;

            // Usar os limites do retângulo diretamente
            minX = Mathf.Min(minX, roomRect.xMin);
            maxX = Mathf.Max(maxX, roomRect.xMax);
            minY = Mathf.Min(minY, roomRect.yMin);
            maxY = Mathf.Max(maxY, roomRect.yMax);
        }

        // Adicionar padding aos limites
        minX -= actualPadding;
        maxX += actualPadding;
        minY -= actualPadding;
        maxY += actualPadding;

        // Criar o objeto Bounds
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        
        return new Bounds(center, size);
    }

    /// <summary>
    /// Atualiza o padding
    /// </summary>
    public void SetPadding(float padding) {
        boundsPadding = padding;
    }
}




