using UnityEngine;
using System.Collections;

/// <summary>
/// Gerencia animações suaves da câmera (movimento e zoom)
/// </summary>
public class CameraAnimator : MonoBehaviour {
    private Camera cam;
    private Transform cameraTransform;

    private void Awake() {
        cam = GetComponent<Camera>();
        cameraTransform = transform;
    }

    /// <summary>
    /// Move a câmera suavemente para uma posição
    /// </summary>
    public Coroutine MoveCameraSmoothly(Vector3 targetPosition, float duration) {
        return StartCoroutine(MoveCameraCoroutine(targetPosition, duration));
    }

    /// <summary>
    /// Aplica zoom suavemente
    /// </summary>
    public Coroutine ZoomSmoothly(float targetSize, float duration) {
        return StartCoroutine(ZoomCoroutine(targetSize, duration));
    }

    /// <summary>
    /// Move e aplica zoom simultaneamente
    /// </summary>
    public void MoveAndZoomSmoothly(Vector3 targetPosition, float targetSize, float duration) {
        StartCoroutine(MoveCameraCoroutine(targetPosition, duration));
        StartCoroutine(ZoomCoroutine(targetSize, duration));
    }

    private IEnumerator MoveCameraCoroutine(Vector3 targetPosition, float duration) {
        Vector3 startPosition = cameraTransform.position;
        float elapsed = 0f;

        while (elapsed < duration) {
            cameraTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.position = targetPosition;
    }

    private IEnumerator ZoomCoroutine(float targetSize, float duration) {
        if (cam == null) yield break;

        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration) {
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }
}


