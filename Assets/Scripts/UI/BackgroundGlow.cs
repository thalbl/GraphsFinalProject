using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Anima o brilho de fundo do menu com efeito de pulso suave.
/// Simula o gradiente radial animado do design HTML.
/// </summary>
[RequireComponent(typeof(Image))]
public class BackgroundGlow : MonoBehaviour
{
    [Header("Configurações de Cor")]
    [ColorUsage(true, true)]
    [Tooltip("Cor do glow no pico (HDR vermelho)")]
    public Color glowColorMax = new Color(0.23f, 0f, 0f, 1f) * 1.5f;
    
    [ColorUsage(true, true)]
    [Tooltip("Cor do glow no mínimo (mais escuro)")]
    public Color glowColorMin = new Color(0.15f, 0f, 0f, 1f);

    [Header("Animação")]
    [Tooltip("Velocidade do pulso (menor = mais lento)")]
    public float pulseSpeed = 0.5f;
    
    [Tooltip("Amplitude do pulso (0-1)")]
    [Range(0f, 1f)]
    public float pulseAmplitude = 0.3f;

    private Image backgroundImage;
    private float pulseTimer = 0f;

    void Start()
    {
        backgroundImage = GetComponent<Image>();
        
        if (backgroundImage == null)
        {
            Debug.LogError("BackgroundGlow requer um componente Image!");
            enabled = false;
        }
    }

    void Update()
    {
        if (backgroundImage == null) return;

        // Incrementa o timer
        pulseTimer += Time.deltaTime * pulseSpeed;

        // Calcula o valor do pulso usando uma onda senoidal suave
        float pulse = Mathf.Sin(pulseTimer) * 0.5f + 0.5f; // Normaliza para 0-1
        pulse = pulse * pulseAmplitude + (1f - pulseAmplitude); // Ajusta amplitude

        // Interpola entre as cores
        Color currentColor = Color.Lerp(glowColorMin, glowColorMax, pulse);
        backgroundImage.color = currentColor;
    }

    /// <summary>
    /// Reseta o timer do pulso (útil para sincronização)
    /// </summary>
    public void ResetPulse()
    {
        pulseTimer = 0f;
    }
}
