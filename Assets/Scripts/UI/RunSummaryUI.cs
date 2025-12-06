using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RunSummaryUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject contentPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI summaryText;
    
    [Header("Path Visualization")]
    [SerializeField] private LineRenderer pathLineRenderer;
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private float lineWidth = 0.2f;

    private void Awake()
    {
        // Garante que o painel comece escondido
        if (contentPanel != null) contentPanel.SetActive(false);
        
        // Configura LineRenderer se não estiver configurado
        if (pathLineRenderer != null)
        {
            pathLineRenderer.positionCount = 0;
            pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    public void ShowSummary(GameMetrics metrics, DungeonGenerator generator, List<int> pathHistory)
    {
        if (contentPanel != null) contentPanel.SetActive(true);

        // 1. Atualiza Textos
        if (titleText != null)
        {
            titleText.text = metrics.victory ? "VITÓRIA!" : "FIM DE JOGO";
            titleText.color = metrics.victory ? Color.green : Color.red;
        }

        if (summaryText != null)
        {
            summaryText.text = metrics.GetFormattedSummary();
        }

        // 2. Desenha o Caminho
        DrawPath(generator, pathHistory);
    }

    private void DrawPath(DungeonGenerator generator, List<int> pathHistory)
    {
        if (pathLineRenderer == null || generator == null || pathHistory == null) return;
        
        // Configuração visual
        pathLineRenderer.startWidth = lineWidth;
        pathLineRenderer.endWidth = lineWidth;
        pathLineRenderer.startColor = pathColor;
        pathLineRenderer.endColor = pathColor;
        pathLineRenderer.positionCount = pathHistory.Count;
        pathLineRenderer.useWorldSpace = true;

        var allRooms = generator.allRooms;

        for (int i = 0; i < pathHistory.Count; i++)
        {
            int index = pathHistory[i];
            if (index >= 0 && index < allRooms.Count)
            {
                Vector3 pos = allRooms[index].GetWorldPosition();
                
                // Z = -5f garante que a linha fique SOBRE os sprites (que geralmente estão em Z=0)
                pos.z = -5f; 
                
                pathLineRenderer.SetPosition(i, pos);
            }
        }
    }
}
