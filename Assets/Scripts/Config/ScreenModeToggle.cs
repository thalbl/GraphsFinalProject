using UnityEngine;

/// <summary>
/// Permite alternar entre tela cheia e modo janela pressionando uma tecla.
/// Adicione este script a qualquer GameObject persistente na cena.
/// </summary>
public class ScreenModeToggle : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tecla para alternar entre tela cheia e janela")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F3;
    
    [Tooltip("Resolução da janela (largura)")]
    [SerializeField] private int windowedWidth = 1280;
    
    [Tooltip("Resolução da janela (altura)")]
    [SerializeField] private int windowedHeight = 720;

    private bool isFullscreen;

    void Start()
    {
        isFullscreen = Screen.fullScreen;
        Debug.Log($"[ScreenModeToggle] Ativo. Pressione {toggleKey} para alternar tela cheia/janela.");
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleScreenMode();
        }
    }

    /// <summary>
    /// Alterna entre tela cheia e modo janela.
    /// </summary>
    public void ToggleScreenMode()
    {
        isFullscreen = !isFullscreen;

        if (isFullscreen)
        {
            // Muda para tela cheia usando resolução nativa do monitor
            Resolution nativeRes = Screen.currentResolution;
            Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.FullScreenWindow);
            Debug.Log($"[ScreenModeToggle] Modo: TELA CHEIA ({nativeRes.width}x{nativeRes.height})");
        }
        else
        {
            // Muda para modo janela com resolução configurada
            Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);
            Debug.Log($"[ScreenModeToggle] Modo: JANELA ({windowedWidth}x{windowedHeight})");
        }
    }
}
