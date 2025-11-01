using UnityEngine;
using TMPro; // Importe TextMeshPro

public class RoomVisual : MonoBehaviour {
    public TextMeshPro roomInfoText;
    public SpriteRenderer mainSprite;

    private RoomNode roomData;
    private Color originalColor;

    public void Initialize(RoomNode room) {
        roomData = room;

        mainSprite = GetComponent<SpriteRenderer>();
        originalColor = mainSprite.color;

        // --- Configura o Texto (TextMeshPro) ---
        GameObject textGO = new GameObject("RoomInfo");
        textGO.transform.SetParent(transform);
        textGO.transform.localPosition = Vector3.zero;

        roomInfoText = textGO.AddComponent<TextMeshPro>();
        roomInfoText.text = $"{room.roomType}\n(Dist: {room.distanceFromStart})";
        roomInfoText.fontSize = 2; // Ajuste o tamanho conforme necessário
        roomInfoText.alignment = TextAlignmentOptions.Center;
        roomInfoText.sortingOrder = 2; // Na frente da sala
        roomInfoText.rectTransform.sizeDelta = new Vector2(room.roomRect.width, room.roomRect.height);
    }

    // Highlight de mouse-over
    void OnMouseEnter() {
        mainSprite.color = Color.cyan; // Feedback visual
        // Mostra as conexões
        Debug.Log($"Sala: {roomData.roomType} | Conexões: {roomData.connections.Count}");
    }

    void OnMouseExit() {
        mainSprite.color = originalColor; // Retorna à cor original
    }

    // Mostra info no clique
    void OnMouseDown() {
        Debug.Log($"Clicou na sala: {roomData.logicalPosition} | Tipo: {roomData.roomType} | Distância: {roomData.distanceFromStart}");
    }
}