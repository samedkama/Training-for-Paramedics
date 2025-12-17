using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PatientCardUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text patientNameText;   // имя рядом с иконкой
    public TMP_Text patientInfoText;   // текст внутри карточки
    public Button cardButton;

    [Header("Panel Animation")]
    public Animator panelAnimator;     // Animator панели
    public string isOpenParam = "IsOpen";

    private bool isOpen = false;

    // храним данные пациента
    private Dictionary<string, string> patientData = new Dictionary<string, string>();

    private void Awake()
    {
        if (cardButton != null)
            cardButton.onClick.AddListener(TogglePanel);
            if (panelAnimator!=null)
            panelAnimator.SetBool(isOpenParam, false);

        ClosePanelImmediate();
    }

    /// <summary>
    /// Вызывается ChatManager при создании нового пациента.
    /// ВАЖНО: сюда приходит ТОЛЬКО кейс-промт (без basePrompt).
    /// </summary>
    public void UpdatePatientFromPrompt(string casePrompt)
    {
        ParsePatientData(casePrompt);

        // -------- ИМЯ (строка 1) --------
        if (patientNameText != null)
        {
            patientNameText.text = patientData.TryGetValue("Name", out var name)
                ? name
                : "Unknown patient";
        }

        // -------- КАРТОЧКА (строки 2–7) --------
        if (patientInfoText != null)
            patientInfoText.text = BuildInfoText();

        // при новом пациенте карточка всегда закрыта
        ClosePanelImmediate();
    }

    // ---------------- UI ----------------

    private void TogglePanel()
    {
        isOpen = !isOpen;

        if (panelAnimator != null)
            panelAnimator.SetBool(isOpenParam, isOpen);
    }

    private void ClosePanelImmediate()
    {
        isOpen = false;

        if (panelAnimator != null)
        {
            panelAnimator.SetBool(isOpenParam, false);
        }
    }

    
    private void ParsePatientData(string text)
    {
        patientData.Clear();
        if (string.IsNullOrEmpty(text)) return;

        var lines = text.Split('\n');

        // ---------- строка 1: ИМЯ ----------
        if (lines.Length > 0 && lines[0].Contains(":"))
        {
            var parts = lines[0].Split(':', 2);
            patientData["Name"] = parts[1].Trim();
        }

        // ---------- строки 2–7: ДАННЫЕ ----------
        for (int i = 1; i < lines.Length && i <= 6; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (!line.Contains(":")) continue;

            var parts = line.Split(':', 2);
            var key = parts[0].Trim();
            var value = parts[1].Trim();

            patientData[key] = value;
        }
    }

    private string BuildInfoText()
    {
        // порядок гарантирован
        string[] order =
        {
            "Sex",
            "Age",
            "Temperature",
            "Oxygen saturation",
            "Blood pressure",
            "Heart rate"
        };

        string result = "";

        foreach (var key in order)
        {
            if (patientData.TryGetValue(key, out var value))
                result += $"{key}: {value}\n";
        }

        return result.Trim();
    }
}