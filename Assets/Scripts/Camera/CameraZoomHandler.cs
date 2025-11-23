using UnityEngine;

/// <summary>
/// Gerencia o zoom da câmera ortográfica
/// </summary>
public class CameraZoomHandler {
    private Camera cam;
    private float zoomSpeed;
    private float minZoom;
    private float maxZoom;

    public CameraZoomHandler(Camera camera, float speed, float min, float max) {
        cam = camera;
        zoomSpeed = speed;
        minZoom = min;
        maxZoom = max;
    }

    /// <summary>
    /// Processa o input de zoom do mouse
    /// </summary>
    public void HandleZoom() {
        if (cam == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (scrollInput != 0) {
            // Calcular novo tamanho ortográfico
            float newSize = cam.orthographicSize - scrollInput * zoomSpeed;
            newSize = Mathf.Clamp(newSize, minZoom, maxZoom);

            // Aplicar zoom mantendo a posição do mouse como ponto focal
            Vector3 mouseWorldPosBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            cam.orthographicSize = newSize;
            Vector3 mouseWorldPosAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);

            // Ajustar posição da câmera para manter o foco no mouse
            Vector3 positionAdjustment = mouseWorldPosBeforeZoom - mouseWorldPosAfterZoom;
            cam.transform.position += positionAdjustment;
        }
    }

    /// <summary>
    /// Define o zoom da câmera
    /// </summary>
    public void SetZoom(float size) {
        if (cam != null) {
            cam.orthographicSize = Mathf.Clamp(size, minZoom, maxZoom);
        }
    }

    /// <summary>
    /// Obtém o zoom atual
    /// </summary>
    public float GetZoom() {
        return cam != null ? cam.orthographicSize : 0f;
    }

    /// <summary>
    /// Atualiza os limites de zoom
    /// </summary>
    public void UpdateZoomLimits(float min, float max) {
        minZoom = min;
        maxZoom = max;
    }
}






