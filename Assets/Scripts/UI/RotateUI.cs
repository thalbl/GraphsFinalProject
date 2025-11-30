using UnityEngine;

/// <summary>
/// Rotaciona continuamente um elemento UI (ex: estrela decorativa)
/// </summary>
public class RotateUI : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20f; // Graus por segundo
    [SerializeField] private bool clockwise = false;
    
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (rectTransform != null)
        {
            float direction = clockwise ? -1f : 1f;
            rectTransform.Rotate(0, 0, direction * rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}
