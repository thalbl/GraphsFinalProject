using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Calcula o zoom necessário para mostrar a dungeon com margens adequadas
/// </summary>
public class CameraZoomCalculator {
    private Camera cam;

    public CameraZoomCalculator(Camera camera) {
        cam = camera;
    }

    /// <summary>
    /// Calcula o tamanho de zoom necessário para mostrar toda a dungeon
    /// </summary>
    public float CalculateRequiredZoomSize(Bounds dungeonBounds, List<RoomNode> rooms) {
        if (cam == null || rooms == null || rooms.Count == 0) {
            return cam != null ? cam.orthographicSize : 5f;
        }

        // Para resoluções menores (aspect ratio menor), precisamos de mais espaço
        float aspect = cam.aspect;
        float requiredSizeX = dungeonBounds.size.x / (2f * aspect);
        float requiredSizeY = dungeonBounds.size.y / 2f;

        // Calcular o tamanho da maior sala para adicionar margem extra
        float maxRoomSize = 0f;
        foreach (RoomNode room in rooms) {
            float roomMaxSize = Mathf.Max(room.roomRect.width, room.roomRect.height);
            maxRoomSize = Mathf.Max(maxRoomSize, roomMaxSize);
        }

        // Margem baseada no aspect ratio: resoluções menores precisam de mais margem
        // Aspect ratio menor (< 1.5) = mais margem vertical necessária
        float marginMultiplier = aspect < 1.5f ? 1.4f : (aspect < 1.7f ? 1.3f : 1.25f);

        // Adicionar margem extra baseada no tamanho das salas
        float roomMargin = maxRoomSize * 0.5f;

        // Calcular o tamanho necessário com margens adequadas
        float requiredSize = Mathf.Max(requiredSizeX, requiredSizeY);
        requiredSize = requiredSize * marginMultiplier + roomMargin;

        return requiredSize;
    }
}






