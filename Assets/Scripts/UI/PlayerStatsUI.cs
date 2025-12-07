using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Atualiza a UI com os stats do jogador (Vida, Sanidade, Suprimentos).
/// Suporta barras visuais (Sliders) e valores numéricos (Text).
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Text References (Valores Numéricos)")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI sanityText;
    [SerializeField] private TextMeshProUGUI suppliesText;
    [SerializeField] private TextMeshProUGUI traitsText; // Lista de traits ativos

    [Header("Slider References (Barras Visuais)")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider sanitySlider;
    [SerializeField] private Slider suppliesSlider;

    [Header("Fallback: UI Text Legacy")]
    [SerializeField] private Text healthTextLegacy;
    [SerializeField] private Text sanityTextLegacy;
    [SerializeField] private Text suppliesTextLegacy;
    [SerializeField] private Text traitsTextLegacy;

    [Header("Max Values (para Sliders)")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float maxSupplies = 10f;

    private PlayerController playerController;
    private PlayerStats playerStats;

    void Start()
    {
        // Tenta encontrar o player na cena
        FindPlayer();

        // Se não encontrou, tenta novamente após um delay
        if (playerController == null)
        {
            Invoke(nameof(FindPlayer), 0.5f);
        }
    }

    /// <summary>
    /// Busca o PlayerController e conecta eventos.
    /// </summary>
    private void FindPlayer()
    {
        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            SetPlayer(playerController);
        }
        else
        {
            Debug.LogWarning("PlayerStatsUI não encontrou PlayerController na cena!");
        }
    }

    /// <summary>
    /// Define o player e conecta os eventos (pode ser chamado externamente).
    /// </summary>
    public void SetPlayer(PlayerController player)
    {
        // Remove eventos anteriores se existirem
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateStatsDisplay;
            playerStats.OnTraitGained -= UpdateTraitsDisplay;
        }

        playerController = player;
        playerStats = playerController.stats;

        // Sincroniza max values com os valores REAIS do player
        maxHealth = playerStats.maxHealth;
        maxSanity = playerStats.maxSanity;
        maxSupplies = playerStats.maxSupplies;

        // Configura sliders com os valores corretos
        ConfigureSliders();

        // Se inscreve nos eventos
        playerStats.OnStatsChanged += UpdateStatsDisplay;
        playerStats.OnTraitGained += UpdateTraitsDisplay;

        // Atualização inicial com valores corretos
        UpdateStatsDisplay(playerStats.currentHealth, playerStats.currentSanity, playerStats.currentSupplies);
        UpdateAllTraits();

        Debug.Log($"[PlayerStatsUI] Conectado ao Player! Max HP={maxHealth}, Sanity={maxSanity}, Supplies={maxSupplies}");
    }

    /// <summary>
    /// Configura os Sliders com valores mínimos e máximos.
    /// </summary>
    private void ConfigureSliders()
    {
        if (healthSlider != null)
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHealth;
            healthSlider.wholeNumbers = false;
            Debug.Log($"[PlayerStatsUI] Health Slider configurado: 0-{maxHealth}");
        }

        if (sanitySlider != null)
        {
            sanitySlider.minValue = 0;
            sanitySlider.maxValue = maxSanity;
            sanitySlider.wholeNumbers = false;
            Debug.Log($"[PlayerStatsUI] Sanity Slider configurado: 0-{maxSanity}");
        }

        if (suppliesSlider != null)
        {
            suppliesSlider.minValue = 0;
            suppliesSlider.maxValue = maxSupplies;
            suppliesSlider.wholeNumbers = false;
            Debug.Log($"[PlayerStatsUI] Supplies Slider configurado: 0-{maxSupplies}");
        }
    }

    /// <summary>
    /// Atualiza todos os elementos da UI (Texto + Sliders).
    /// </summary>
    private void UpdateStatsDisplay(float health, float sanity, float supplies)
    {
        // ═══ ATUALIZA TEXTOS (Apenas valores numéricos) ═══
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(health)}";
        if (sanityText != null)
            sanityText.text = $"{Mathf.CeilToInt(sanity)}";
        if (suppliesText != null)
            suppliesText.text = $"{Mathf.CeilToInt(supplies)}";

        // ═══ ATUALIZA SLIDERS (BARRAS) ═══
        if (healthSlider != null)
            healthSlider.value = health;
        if (sanitySlider != null)
            sanitySlider.value = sanity;
        if (suppliesSlider != null)
            suppliesSlider.value = supplies;

        // ═══ LEGACY TEXT (com labels) ═══
        if (healthTextLegacy != null)
            healthTextLegacy.text = $"Vida: {health:F0}/{maxHealth:F0}";
        if (sanityTextLegacy != null)
            sanityTextLegacy.text = $"Sanidade: {sanity:F0}/{maxSanity:F0}";
        if (suppliesTextLegacy != null)
            suppliesTextLegacy.text = $"Suprimentos: {supplies:F0}/{maxSupplies:F0}";
    }

    /// <summary>
    /// Atualiza display quando um novo trait é ganho.
    /// </summary>
    private void UpdateTraitsDisplay(Trait newTrait)
    {
        UpdateAllTraits();
        Debug.Log($"UI - Novo trait exibido: {newTrait.traitName}");
    }

    /// <summary>
    /// Atualiza a lista completa de traits.
    /// </summary>
    private void UpdateAllTraits()
    {
        if (playerStats == null || playerStats.activeTraits == null)
            return;

        string traitsString = "";
        
        if (playerStats.activeTraits.Count == 0)
        {
            traitsString = "Traits: Nenhum";
        }
        else
        {
            traitsString = $"Traits ({playerStats.activeTraits.Count}):\n";
            
            foreach (Trait trait in playerStats.activeTraits)
            {
                string color = trait.isAffliction ? "red" : "green";
                traitsString += $"<color={color}>• {trait.traitName}</color>\n";
            }
            
            // Remove última quebra de linha
            traitsString = traitsString.TrimEnd('\n');
        }

        // Atualiza TextMeshPro
        if (traitsText != null)
        {
            traitsText.text = traitsString;
            
            // Garante que o overflow está configurado
            traitsText.overflowMode = TMPro.TextOverflowModes.Overflow;
            traitsText.enableWordWrapping = true;
        }

        // Atualiza UI Text legacy
        if (traitsTextLegacy != null)
        {
            traitsTextLegacy.text = traitsString.Replace("<color=red>", "").Replace("<color=green>", "").Replace("</color>", "");
            traitsTextLegacy.horizontalOverflow = HorizontalWrapMode.Wrap;
            traitsTextLegacy.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    void OnDestroy()
    {
        // Remove inscrições para evitar memory leaks
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= UpdateStatsDisplay;
            playerStats.OnTraitGained -= UpdateTraitsDisplay;
        }
    }
}
