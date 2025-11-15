using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Verifica a visibilidade dos objetos na viewport da câmera
/// </summary>
public class CameraVisibilityChecker {
    private Camera cam;

    public CameraVisibilityChecker(Camera camera) {
        cam = camera;
    }

    /// <summary>
    /// Verifica quantas salas estão visíveis na viewport
    /// </summary>
    public VisibilityResult CheckVisibility(List<RoomNode> rooms) {
        if (cam == null || rooms == null) {
            return new VisibilityResult { TotalRooms = 0 };
        }

        // Calcular os limites da viewport da câmera
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;
        Vector3 cameraPos = cam.transform.position;

        float viewportMinX = cameraPos.x - cameraWidth;
        float viewportMaxX = cameraPos.x + cameraWidth;
        float viewportMinY = cameraPos.y - cameraHeight;
        float viewportMaxY = cameraPos.y + cameraHeight;

        int fullyVisibleRooms = 0;
        int partiallyVisibleRooms = 0;

        foreach (RoomNode room in rooms) {
            if (room.visualInstance != null) {
                Rect roomRect = room.roomRect;

                // Verificar se a sala está completamente visível
                bool fullyVisible = roomRect.xMin >= viewportMinX &&
                                    roomRect.xMax <= viewportMaxX &&
                                    roomRect.yMin >= viewportMinY &&
                                    roomRect.yMax <= viewportMaxY;

                // Verificar se a sala está parcialmente visível
                bool partiallyVisible = roomRect.xMax >= viewportMinX &&
                                        roomRect.xMin <= viewportMaxX &&
                                        roomRect.yMax >= viewportMinY &&
                                        roomRect.yMin <= viewportMaxY;

                if (fullyVisible) {
                    fullyVisibleRooms++;
                } else if (partiallyVisible) {
                    partiallyVisibleRooms++;
                }
            }
        }

        return new VisibilityResult {
            TotalRooms = rooms.Count,
            FullyVisibleRooms = fullyVisibleRooms,
            PartiallyVisibleRooms = partiallyVisibleRooms,
            ViewportBounds = new Rect(viewportMinX, viewportMinY, viewportMaxX - viewportMinX, viewportMaxY - viewportMinY)
        };
    }
}

/// <summary>
/// Resultado da verificação de visibilidade
/// </summary>
public struct VisibilityResult {
    public int TotalRooms;
    public int FullyVisibleRooms;
    public int PartiallyVisibleRooms;
    public Rect ViewportBounds;

    public bool AllRoomsVisible => FullyVisibleRooms == TotalRooms;
}

