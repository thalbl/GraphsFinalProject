using UnityEngine;

/// <summary>
/// Limita a posição da câmera dentro dos bounds especificados
/// </summary>
public class CameraBoundsClamper {
    private Camera cam;
    private Transform cameraTransform;
    private bool enabled;

    public CameraBoundsClamper(Camera camera, bool enable) {
        cam = camera;
        cameraTransform = camera.transform;
        enabled = enable;
    }

    /// <summary>
    /// Limita a posição da câmera dentro dos bounds
    /// </summary>
    public void ClampPosition(Bounds dungeonBounds) {
        if (!enabled || cam == null || cameraTransform == null) return;

        // Calcular os limites visíveis da câmera
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        // Expandir os limites para garantir que a dungeon caiba na tela
        float expandedMinX = dungeonBounds.min.x;
        float expandedMaxX = dungeonBounds.max.x;
        float expandedMinY = dungeonBounds.min.y;
        float expandedMaxY = dungeonBounds.max.y;

        // Se a dungeon for menor que a viewport, usar limites expandidos
        if (dungeonBounds.size.x < cameraWidth * 2f) {
            expandedMinX = dungeonBounds.center.x - cameraWidth;
            expandedMaxX = dungeonBounds.center.x + cameraWidth;
        }

        if (dungeonBounds.size.y < cameraHeight * 2f) {
            expandedMinY = dungeonBounds.center.y - cameraHeight;
            expandedMaxY = dungeonBounds.center.y + cameraHeight;
        }

        float minX = expandedMinX + cameraWidth;
        float maxX = expandedMaxX - cameraWidth;
        float minY = expandedMinY + cameraHeight;
        float maxY = expandedMaxY - cameraHeight;

        // Só clamp se os limites forem válidos
        if (maxX >= minX && maxY >= minY) {
            Vector3 clampedPosition = cameraTransform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            cameraTransform.position = clampedPosition;
        } else {
            // Se os limites não forem válidos, centralizar na dungeon
            cameraTransform.position = new Vector3(dungeonBounds.center.x, dungeonBounds.center.y, cameraTransform.position.z);
        }
    }

    /// <summary>
    /// Habilita ou desabilita o clamp
    /// </summary>
    public void SetEnabled(bool enable) {
        enabled = enable;
    }
}




