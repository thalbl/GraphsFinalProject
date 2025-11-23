using UnityEngine;

/// <summary>
/// Delega controle de zoom/foco para o CameraController durante menus.
/// Camada de abstração entre menu UI e CameraController.
/// </summary>
public class GraphViewController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private CameraController cameraController;

    [Header("Configurações de Menu Focus")]
    [Tooltip("Offset da càmera ao focar (para dar espaço ao menu)")]
    [SerializeField] private Vector3 menuOffset = new Vector3(-5f, 0f, 0f);
    
    [Tooltip("Tamanho da câmera no zoom (menor = mais zoom)")]
    [SerializeField] private float focusZoomSize = 5f;
    
    [Tooltip("Duração da transição")]
    [SerializeField] private float transitionDuration = 0.4f;

    // Estado
    private bool isInMenuMode = false;
    private RoomNode focusedRoom;

    void Start()
    {
        // Auto-detecta CameraController
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            
            if (cameraController == null)
            {
                Debug.LogError("GraphViewController: CameraController não encontrado!");
            }
            else
            {
                Debug.Log("GraphViewController: CameraController detectado!");
            }
        }
    }

    /// <summary>
    /// Transiciona para o modo "menu aberto" com zoom na sala especificada.
    /// </summary>
    public void TransitionToMenuMode(RoomNode targetRoom = null)
    {
        if (isInMenuMode || cameraController == null) return;

        isInMenuMode = true;
        focusedRoom = targetRoom;

        if (targetRoom != null)
        {
            // Usa CameraController para fazer zoom na sala
            cameraController.EnterMenuFocusMode(targetRoom, menuOffset, focusZoomSize, transitionDuration);
            Debug.Log($"GraphViewController: Focando em sala {targetRoom.logicalPosition}");
        }
        else
        {
            Debug.LogWarning("GraphViewController: TransitionToMenuMode chamado sem sala alvo!");
        }
    }

    /// <summary>
    /// Transiciona de volta para o modo normal.
    /// </summary>
    public void TransitionToNormalMode()
    {
        if (!isInMenuMode || cameraController == null) return;

        isInMenuMode = false;
        focusedRoom = null;
        
        // Usa CameraController para voltar ao normal
        cameraController.ExitMenuFocusMode(transitionDuration * 0.75f); // Um pouco mais rápido na volta
        Debug.Log("GraphViewController: Voltando ao modo normal");
    }

    /// <summary>
    /// Força atualização do estado normal (se necessário).
    /// </summary>
    public void RefreshNormalState()
    {
        // Não precisa fazer nada - CameraController gerencia isso
    }

    // Propriedades públicas
    public bool IsInMenuMode => isInMenuMode;
    public bool IsTransitioning => cameraController != null && cameraController.IsInMenuFocusMode;
    public RoomNode FocusedRoom => focusedRoom;
}
