using UnityEngine;
using UnityEngine.UI;

public class Gradient : MonoBehaviour
{
    public Image background;       // UI image that receives the dynamic gradient color.
    public Color colorTop = Color.white;    // Target color when the scroll is at the top.
    public Color colorBottom = Color.blue;  // Target color when the scroll is at the bottom.
    public ScrollRect scrollRect;  // Scroll view used to read the current vertical position.

    private float smoothSpeed = 5f; // Controls how quickly the background transitions to the target color.
    private Color targetColor;

    void Update()
    {
        // Read current vertical scroll position in the [0..1] range.
        float scrollValue = scrollRect.verticalNormalizedPosition;

        // Interpolate between bottom and top colors based on scroll position.
        targetColor = Color.Lerp(colorBottom, colorTop, scrollValue);

        // Smoothly blend the background color to avoid abrupt visual jumps.
        background.color = Color.Lerp(background.color, targetColor, Time.deltaTime * smoothSpeed);
    }
}
