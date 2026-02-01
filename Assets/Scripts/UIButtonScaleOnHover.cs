using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonScaleOnHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Scale Values")]
    public float hoverScale = 1.1f;
    public float pressedScale = 0.95f;
    public float scaleSpeed = 10f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private bool isHovered;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        StartScale(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        StartScale(originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartScale(originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Return to hover size if still hovering, otherwise normal
        if (isHovered)
            StartScale(originalScale * hoverScale);
        else
            StartScale(originalScale);
    }

    void StartScale(Vector3 targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleTo(targetScale));
    }

    IEnumerator ScaleTo(Vector3 target)
    {
        while (Vector3.Distance(transform.localScale, target) > 0.001f)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                target,
                Time.deltaTime * scaleSpeed
            );
            yield return null;
        }

        transform.localScale = target;
    }
}
