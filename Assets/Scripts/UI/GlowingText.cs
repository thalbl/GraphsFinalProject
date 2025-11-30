using UnityEngine;
using TMPro;

/// <summary>
/// Adiciona um outline preto estático ao texto.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class GlowingText : MonoBehaviour
{
    [Header("Configurações de Outline")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineWidth = 0.2f;

    private TextMeshProUGUI textComponent;
    private Material textMaterial;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        
        // Cria uma instância do material para não afetar outros textos
        if (textComponent.fontMaterial != null)
        {
            textMaterial = new Material(textComponent.fontMaterial);
            textComponent.fontMaterial = textMaterial;
        }
    }

    void Start()
    {
        ApplyOutline();
    }

    /// <summary>
    /// Aplica o outline preto ao texto.
    /// </summary>
    private void ApplyOutline()
    {
        if (textMaterial == null) return;

        // Ativa outline se disponível
        if (textMaterial.HasProperty("_OutlineWidth"))
        {
            textMaterial.SetFloat("_OutlineWidth", outlineWidth);
        }

        if (textMaterial.HasProperty("_OutlineColor"))
        {
            textMaterial.SetColor("_OutlineColor", outlineColor);
        }
    }

    void OnDestroy()
    {
        // Limpa material instanciado
        if (textMaterial != null)
        {
            Destroy(textMaterial);
        }
    }
}
