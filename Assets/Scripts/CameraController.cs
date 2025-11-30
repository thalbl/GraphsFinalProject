using UnityEngine;

/// <summary>
/// Controlador principal da câmera que orquestra todos os sistemas modulares
/// </summary>
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

    [Header("Quick Actions")]
    public KeyCode showAllKey = KeyCode.Space;

    // Referências
    private Camera cam;
    private DungeonGenerator dungeonGenerator;

    // Módulos modulares
    private CameraZoomHandler zoomHandler;
    private CameraPanHandler panHandler;
    private DungeonBoundsCalculator boundsCalculator;
    private CameraZoomCalculator zoomCalculator;
    private CameraAnimator cameraAnimator;
    private CameraVisibilityChecker visibilityChecker;
    private CameraBoundsClamper boundsClamper;

    // Estado
    private Bounds dungeonBounds;
    private bool boundsCalculated = false;

    // Menu Focus Mode
    private bool inMenuFocusMode = false;
    private Vector3 normalPosition;
    private float normalZoom;
    private RoomNode focusedRoom;

    void Start() {
        cam = GetComponent<Camera>();
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();

        // Garantir que a câmera está configurada como ortográfica
        if (cam != null && !cam.orthographic) {
            cam.orthographic = true;
            Debug.LogWarning("⚠️ Câmera configurada como ortográfica no Start().");
        }

        // Inicializar módulos
        InitializeModules();

        // Configurar zoom inicial
        if (cam != null) {
            cam.orthographicSize = defaultZoom;
        }

        // Calcular limites após a geração da dungeon
        if (dungeonGenerator != null) {
            dungeonGenerator.OnDungeonGenerated += CalculateDungeonBounds;
            
            // Se a dungeon já foi gerada, calcular limites imediatamente
            if (dungeonGenerator.allRooms != null && dungeonGenerator.allRooms.Count > 0) {
                CalculateDungeonBounds();
            }
        } else {
            Debug.LogError("❌ DungeonGenerator não encontrado! A câmera não poderá focar no grafo.");
        }
    }

    void Update() {
        // Não processar inputs de zoom/pan se estiver em modo menu
        if (!inMenuFocusMode)
        {
            // Processar inputs de zoom e pan
            zoomHandler?.HandleZoom();
            panHandler?.HandlePan();
            
            // Limitar posição da câmera dentro dos bounds
            if (boundsCalculated) {
                boundsClamper?.ClampPosition(dungeonBounds);
            }

            // Atalho para mostrar toda a dungeon
            if (Input.GetKeyDown(showAllKey)) {
                ShowEntireDungeon();
            }
        }
    }

    /// <summary>
    /// Inicializa todos os módulos
    /// </summary>
    private void InitializeModules() {
        if (cam == null) return;

        // Inicializar handlers
        zoomHandler = new CameraZoomHandler(cam, zoomSpeed, minZoom, maxZoom);
        panHandler = new CameraPanHandler(cam, panSpeed, invertPan);
        
        // Inicializar calculadores
        boundsCalculator = new DungeonBoundsCalculator(boundsPadding);
        zoomCalculator = new CameraZoomCalculator(cam);
        
        // Inicializar verificador de visibilidade
        visibilityChecker = new CameraVisibilityChecker(cam);
        
        // Inicializar clamp de bounds
        boundsClamper = new CameraBoundsClamper(cam, useBounds);
        
        // CameraAnimator precisa estar no GameObject
        cameraAnimator = GetComponent<CameraAnimator>();
        if (cameraAnimator == null) {
            cameraAnimator = gameObject.AddComponent<CameraAnimator>();
        }
    }

    /// <summary>
    /// Calcula os limites da dungeon
    /// </summary>
    private void CalculateDungeonBounds() {
        if (dungeonGenerator == null || dungeonGenerator.allRooms == null || dungeonGenerator.allRooms.Count == 0) {
            Debug.LogWarning("⚠️ CalculateDungeonBounds: DungeonGenerator ou allRooms está vazio!");
            return;
        }

        // Garantir que a câmera está ortográfica
        if (cam != null && !cam.orthographic) {
            cam.orthographic = true;
            Debug.LogWarning("⚠️ Câmera configurada como ortográfica no CalculateDungeonBounds.");
        }

        // Calcular bounds usando o calculador
        dungeonBounds = boundsCalculator.CalculateBounds(dungeonGenerator.allRooms);
        boundsCalculated = true;

        Debug.Log($"📐 Limites da dungeon calculados: Center={dungeonBounds.center}, Size={dungeonBounds.size}");

        // Calcular limites dinâmicos de zoom
        CalculateDynamicZoomLimits();

        // Posicionar a câmera na dungeon após calcular os limites
        if (dungeonGenerator.allRooms.Count > 0) {
            PositionCameraOnDungeon();
            AdjustZoomForDungeon();
            VerifyCameraVisibility();
        }
    }

    /// <summary>
    /// Posiciona a câmera no centro da dungeon
    /// </summary>
    private void PositionCameraOnDungeon() {
        // ═══ CORREÇÃO: Não reposiciona se já tiver um player ativo ═══
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Debug.Log("📷 Player detectado, pulando PositionCameraOnDungeon...");
            return;
        }

        // Manter o Z da câmera, mas garantir que seja negativo para câmeras ortográficas
        float cameraZ = transform.position.z;
        if (cameraZ >= 0) {
            cameraZ = -10f;
            Debug.LogWarning($"⚠️ Ajustando Z da câmera para {cameraZ} (câmeras ortográficas precisam de Z negativo).");
        }
        
        // Posicionar a câmera no centro da dungeon
        Vector3 targetPosition = new Vector3(dungeonBounds.center.x, dungeonBounds.center.y, cameraZ);
        transform.position = targetPosition;
        
        Debug.Log($"📷 Câmera reposicionada para centro da dungeon: {targetPosition}");
    }

    /// <summary>
    /// Ajusta o zoom para mostrar toda a dungeon
    /// </summary>
    private void AdjustZoomForDungeon() {
        float requiredSize = zoomCalculator.CalculateRequiredZoomSize(dungeonBounds, dungeonGenerator.allRooms);
        float clampedSize = Mathf.Clamp(requiredSize, minZoom, maxZoom);
        
        zoomHandler.SetZoom(clampedSize);
        
        Debug.Log($"📷 Zoom ajustado: Size={cam.orthographicSize:F2}, Required={requiredSize:F2}, Aspect={cam.aspect:F2}");
    }

    /// <summary>
    /// Verifica a visibilidade das salas
    /// </summary>
    private void VerifyCameraVisibility() {
        if (visibilityChecker == null || dungeonGenerator?.allRooms == null) return;

        VisibilityResult result = visibilityChecker.CheckVisibility(dungeonGenerator.allRooms);
        
        Debug.Log($"👁️ Salas visíveis: {result.FullyVisibleRooms} completas, {result.PartiallyVisibleRooms} parciais ({result.FullyVisibleRooms + result.PartiallyVisibleRooms}/{result.TotalRooms} total)");
        
        if (!result.AllRoomsVisible) {
            Debug.LogWarning($"⚠️ {result.TotalRooms - result.FullyVisibleRooms} sala(s) não estão completamente visíveis na viewport.");
            Debug.LogWarning($"   Viewport: X=[{result.ViewportBounds.xMin:F2}, {result.ViewportBounds.xMax:F2}], Y=[{result.ViewportBounds.yMin:F2}, {result.ViewportBounds.yMax:F2}]");
            Debug.LogWarning($"   Camera Size: {cam.orthographicSize:F2}, Aspect: {cam.aspect:F2}");
        }
    }

    /// <summary>
    /// Calcula os limites dinâmicos de zoom
    /// </summary>
    private void CalculateDynamicZoomLimits() {
        if (!boundsCalculated) return;

        // Calcular o zoom necessário usando o calculador
        float requiredZoom = zoomCalculator.CalculateRequiredZoomSize(dungeonBounds, dungeonGenerator.allRooms);
        
        // Definir maxZoom dinamicamente (com margem adicional para permitir zoom out)
        maxZoom = requiredZoom * 1.5f;
        
        // Garantir um mínimo e máximo razoável
        maxZoom = Mathf.Clamp(maxZoom, 5f, 50f);
        
        // Atualizar os limites no zoom handler
        zoomHandler?.UpdateZoomLimits(minZoom, maxZoom);
        
        Debug.Log($"🔍 Zoom dinâmico: Required={requiredZoom:F1}, MaxZoom={maxZoom:F1}, Aspect={cam.aspect:F2}");
    }

    /// <summary>
    /// Mostra toda a dungeon na tela
    /// </summary>
    public void ShowEntireDungeon(float duration = 0.3f) {
        if (!boundsCalculated) {
            CalculateDungeonBounds();
            return;
        }

        // Calcular zoom necessário usando o calculador
        float targetSize = zoomCalculator.CalculateRequiredZoomSize(dungeonBounds, dungeonGenerator.allRooms);
        targetSize = Mathf.Clamp(targetSize, minZoom, maxZoom);
        
        // Manter o Z da câmera atual (ou usar -10 se não for negativo)
        float cameraZ = transform.position.z;
        if (cameraZ >= 0) cameraZ = -10f;
        Vector3 targetPosition = new Vector3(dungeonBounds.center.x, dungeonBounds.center.y, cameraZ);
        
        // Animar câmera e zoom
        cameraAnimator?.MoveAndZoomSmoothly(targetPosition, targetSize, duration);
    }

    /// <summary>
    /// Foca em uma sala específica
    /// </summary>
    public void FocusOnRoom(RoomNode room, float duration = 1f) {
        if (room != null && cameraAnimator != null) {
            Vector3 targetPosition = new Vector3(room.roomRect.center.x, room.roomRect.center.y, transform.position.z);
            cameraAnimator.MoveCameraSmoothly(targetPosition, duration);
        }
    }

    /// <summary>
    /// Entra no modo "Menu Focus" - foca em uma sala e desabilita controles
    /// </summary>
    public void EnterMenuFocusMode(RoomNode targetRoom, Vector3 offset, float zoomSize, float duration = 0.4f)
    {
        if (inMenuFocusMode || targetRoom == null || cam == null) return;

        // Salva estado atual
        normalPosition = transform.position;
        normalZoom = cam.orthographicSize;
        focusedRoom = targetRoom;
        inMenuFocusMode = true;

        // Calcula posição alvo (sala + offset para menu)
        Vector3 roomWorldPos = targetRoom.GetWorldPosition();
        Vector3 targetPosition = new Vector3(
            roomWorldPos.x + offset.x,
            roomWorldPos.y + offset.y,
            transform.position.z
        );

        // Anima câmera para foco
        cameraAnimator?.MoveAndZoomSmoothly(targetPosition, zoomSize, duration);

        Debug.Log($"📷 Menu Focus Mode: Focando em sala {targetRoom.logicalPosition}, Zoom: {normalZoom:F1} → {zoomSize:F1}");
    }

    /// <summary>
    /// Sai do modo "Menu Focus" - volta à posição/zoom normal
    /// </summary>
    public void ExitMenuFocusMode(float duration = 0.3f)
    {
        if (!inMenuFocusMode || cam == null) return;

        // Anima volta para posição normal
        cameraAnimator?.MoveAndZoomSmoothly(normalPosition, normalZoom, duration);

        // Reseta estado
        inMenuFocusMode = false;
        focusedRoom = null;

        Debug.Log($"📷 Menu Focus Mode: Voltando ao normal - Pos: {normalPosition}, Zoom: {normalZoom:F1}");
    }

    // Propriedades públicas
    public bool IsInMenuFocusMode => inMenuFocusMode;

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

