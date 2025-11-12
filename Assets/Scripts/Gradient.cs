using UnityEngine;
using UnityEngine.UI;

public class Gradient : MonoBehaviour
{
    public Image background;       // UI Image для градиента
    public Color colorTop = Color.white;    // цвет при верхней прокрутке
    public Color colorBottom = Color.blue;  // цвет при нижней прокрутке
    public ScrollRect scrollRect;  // Scroll View

    private float smoothSpeed = 5f; // скорость плавного перехода
    private Color targetColor;

    void Update()
    {
        // Получаем текущую позицию скролла (0–1)
        float scrollValue = scrollRect.verticalNormalizedPosition;

        // Вычисляем целевой цвет в зависимости от скролла
        targetColor = Color.Lerp(colorBottom, colorTop, scrollValue);

        // Плавно меняем цвет фона
        background.color = Color.Lerp(background.color, targetColor, Time.deltaTime * smoothSpeed);
    }
}