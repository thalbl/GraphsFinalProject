using UnityEngine;

/// <summary>
/// Gerencia o pan (arrastar) da câmera
/// </summary>
public class CameraPanHandler {
    private Camera cam;
    private Transform cameraTransform;
    private float panSpeed;
    private bool invertPan;

    // Estado do pan
    private Vector3 panStartPosition;
    private Vector3 cameraStartPosition;
    private bool isPanning = false;

    public CameraPanHandler(Camera camera, float speed, bool invert) {
        cam = camera;
        cameraTransform = camera.transform;
        panSpeed = speed;
        invertPan = invert;
    }

    /// <summary>
    /// Processa o input de pan
    /// </summary>
    public void HandlePan() {
        // Iniciar pan (botão direito do mouse)
        if (Input.GetMouseButtonDown(1)) {
            StartPan();
        }

        // Durante o pan
        if (isPanning) {
            UpdatePan();
        }

        // Finalizar pan
        if (Input.GetMouseButtonUp(1)) {
            EndPan();
        }
    }

    private void StartPan() {
        isPanning = true;
        panStartPosition = Input.mousePosition;
        cameraStartPosition = cameraTransform.position;
    }

    private void UpdatePan() {
        if (cam == null || cameraTransform == null) return;

        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - panStartPosition;

        // Converter delta de tela para mundo
        Vector3 worldDelta = cam.ScreenToWorldPoint(mouseDelta) - cam.ScreenToWorldPoint(Vector3.zero);

        // Aplicar inversão se configurado
        if (invertPan) worldDelta = -worldDelta;

        // Aplicar movimento
        Vector3 newPosition = cameraStartPosition - worldDelta * panSpeed;
        cameraTransform.position = newPosition;
    }

    private void EndPan() {
        isPanning = false;
    }

    /// <summary>
    /// Verifica se está em pan
    /// </summary>
    public bool IsPanning() {
        return isPanning;
    }

    /// <summary>
    /// Atualiza as configurações de pan
    /// </summary>
    public void UpdateSettings(float speed, bool invert) {
        panSpeed = speed;
        invertPan = invert;
    }
}




