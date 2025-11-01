using UnityEngine;

public class CameraController : MonoBehaviour {
    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    public float defaultZoom = 5f;

    [Header("Pan Settings")]
    public float panSpeed = 0.5f;
    public bool invertPan = false;

    [Header("Bounds Settings")]
    public bool useBounds = true;
    public float boundsPadding = 2f;

    // Referências
    private Camera cam;
    private DungeonGenerator dungeonGenerator;

    // Estado do pan
    private Vector3 panStartPosition;
    private Vector3 cameraStartPosition;
    private bool isPanning = false;

    // Limites da câmera
    private Bounds dungeonBounds;
    private bool boundsCalculated = false;

    void Start() {
        cam = GetComponent<Camera>();
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        // Configurar zoom inicial
        cam.orthographicSize = defaultZoom;

        // Calcular limites após a geração da dungeon
        if (dungeonGenerator != null) {
            dungeonGenerator.OnDungeonGenerated += CalculateDungeonBounds;
        }
    }

    void Update() {
        HandleZoom();
        HandlePan();
        ClampCameraPosition();
    }

    private void HandleZoom() {
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
            transform.position += positionAdjustment;
        }
    }

    private void HandlePan() {
        // Iniciar pan (botão direito do mouse)
        if (Input.GetMouseButtonDown(1)) // Botão direito
        {
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
        cameraStartPosition = transform.position;
    }

    private void UpdatePan() {
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 mouseDelta = currentMousePos - panStartPosition;

        // Converter delta de tela para mundo
        Vector3 worldDelta = cam.ScreenToWorldPoint(mouseDelta) - cam.ScreenToWorldPoint(Vector3.zero);

        // Aplicar inversão se configurado
        if (invertPan) worldDelta = -worldDelta;

        // Aplicar movimento
        Vector3 newPosition = cameraStartPosition - worldDelta * panSpeed;
        transform.position = newPosition;
    }

    private void EndPan() {
        isPanning = false;
    }

    private void CalculateDungeonBounds() {
        if (dungeonGenerator == null || dungeonGenerator.allRooms == null || dungeonGenerator.allRooms.Count == 0)
            return;

        // Encontrar os limites extremos da dungeon
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (RoomNode room in dungeonGenerator.allRooms) {
            Rect roomRect = room.roomRect;

            minX = Mathf.Min(minX, roomRect.xMin);
            maxX = Mathf.Max(maxX, roomRect.xMax);
            minY = Mathf.Min(minY, roomRect.yMin);
            maxY = Mathf.Max(maxY, roomRect.yMax);
        }

        // Aplicar padding
        minX -= boundsPadding;
        maxX += boundsPadding;
        minY -= boundsPadding;
        maxY += boundsPadding;

        // Criar bounds
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);

        dungeonBounds = new Bounds(center, size);
        boundsCalculated = true;

        Debug.Log($"Limites da dungeon calculados: {dungeonBounds}");
    }

    private void ClampCameraPosition() {
        if (!useBounds || !boundsCalculated) return;

        // Calcular os limites visíveis da câmera
        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        float minX = dungeonBounds.min.x + cameraWidth;
        float maxX = dungeonBounds.max.x - cameraWidth;
        float minY = dungeonBounds.min.y + cameraHeight;
        float maxY = dungeonBounds.max.y - cameraHeight;

        // Só clamp se a dungeon for maior que a viewport
        if (maxX >= minX && maxY >= minY) {
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
            transform.position = clampedPosition;
        }
        else {
            // Se a dungeon for menor que a viewport, centralizar
            transform.position = dungeonBounds.center;
        }
    }

    // Método público para focar em uma sala específica
    public void FocusOnRoom(RoomNode room, float duration = 1f) {
        if (room != null) {
            Vector3 targetPosition = new Vector3(room.roomRect.center.x, room.roomRect.center.y, transform.position.z);
            StartCoroutine(MoveCameraSmoothly(targetPosition, duration));
        }
    }

    // Método público para mostrar toda a dungeon
    public void ShowEntireDungeon(float duration = 1f) {
        if (boundsCalculated) {
            // Calcular zoom necessário para ver toda a dungeon
            float requiredSizeX = dungeonBounds.size.x / (2f * cam.aspect);
            float requiredSizeY = dungeonBounds.size.y / 2f;
            float targetSize = Mathf.Max(requiredSizeX, requiredSizeY) * 1.1f; // 10% de margem

            targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);

            StartCoroutine(ZoomSmoothly(targetSize, duration));
            StartCoroutine(MoveCameraSmoothly(dungeonBounds.center, duration));
        }
    }

    private System.Collections.IEnumerator MoveCameraSmoothly(Vector3 targetPosition, float duration) {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    private System.Collections.IEnumerator ZoomSmoothly(float targetSize, float duration) {
        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration) {
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }

    // Gizmos para debug (opcional)
    private void OnDrawGizmosSelected() {
        if (useBounds && boundsCalculated) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(dungeonBounds.center, dungeonBounds.size);
        }
    }

    void OnDestroy() {
        // Limpar event subscription
        if (dungeonGenerator != null) {
            dungeonGenerator.OnDungeonGenerated -= CalculateDungeonBounds;
        }
    }
}