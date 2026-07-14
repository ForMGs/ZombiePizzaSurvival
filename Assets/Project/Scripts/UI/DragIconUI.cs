using UnityEngine;
using UnityEngine.UI;

public class DragIconUI : MonoBehaviour
{
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image iconImage;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
            rootCanvas = rootCanvas.rootCanvas;

        if (rootCanvas != null)
        {
            canvasRectTransform =
                rootCanvas.transform as RectTransform;
        }

        if (iconImage == null)
            iconImage = GetComponent<Image>();

        if (iconImage != null)
        {
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;
        }

        Hide();
    }

    public void Show(
        Sprite icon,
        Vector2 screenPosition)
    {
        if (iconImage == null || icon == null)
            return;

        iconImage.sprite = icon;
        iconImage.enabled = true;

        transform.SetAsLastSibling();

        SetPosition(screenPosition);
    }

    public void SetPosition(Vector2 screenPosition)
    {
        if (rectTransform == null ||
            rootCanvas == null ||
            canvasRectTransform == null)
        {
            return;
        }

        Camera eventCamera =
            rootCanvas.renderMode ==
            RenderMode.ScreenSpaceOverlay
                ? null
                : rootCanvas.worldCamera;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPosition,
                eventCamera,
                out Vector2 localPosition))
        {
            rectTransform.anchoredPosition =
                localPosition;
        }
    }

    public void Hide()
    {
        if (iconImage == null)
            return;

        iconImage.sprite = null;
        iconImage.enabled = false;
    }
}