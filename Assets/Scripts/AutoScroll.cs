using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ScrollRect))]
public class AutoScroll : MonoBehaviour
{
    // Reference to the ScrollRect component
    public ScrollRect scrollRect;

    // Internal references
    private RectTransform contentRect; // Content of the ScrollRect
    private Text innerText;            // Text component inside Content
    private float previousHeight;      // Last recorded height of the text

    private void Awake()
    {
        // If scrollRect is not assigned in the Inspector, try to get it from the same GameObject
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        contentRect = scrollRect.content;

        // Automatically find the first Text component inside Content
        innerText = contentRect.GetComponentInChildren<Text>();
        if (innerText == null)
        {
            Debug.LogError("No Text component found inside Content!");
            enabled = false;
            return;
        }

        // Record the initial height of the text
        previousHeight = innerText.rectTransform.rect.height;
    }

    private void OnEnable()
    {
        // Start checking text height continuously
        StartCoroutine(CheckContentHeight());
    }

    private IEnumerator CheckContentHeight()
    {
        while (true)
        {
            // Get current height of the text
            float currentHeight = innerText.rectTransform.rect.height;

            // If text grew, adjust Content size and scroll to bottom
            if (currentHeight > previousHeight)
            {
                Vector2 size = contentRect.sizeDelta;
                size.y = currentHeight;
                contentRect.sizeDelta = size;

                // Wait a frame for layout to update
                yield return new WaitForEndOfFrame();

                // Scroll to the bottom
                ScrollToBottom();

                previousHeight = currentHeight;
            }

            yield return null; // Continue checking every frame
        }
    }

    private void ScrollToBottom()
    {
        // Force Canvas to update before and after changing scroll position
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f; // Scroll to bottom
        Canvas.ForceUpdateCanvases();
    }
}