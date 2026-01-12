using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PatientCardUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text patientNameText;   // имя пациента
    public TMP_Text patientInfoText;   // информация о пациенте

    // данные пациента
    private Dictionary<string, string> patientData = new Dictionary<string, string>();

    /// <summary>
    /// Вызывается ChatManager при создании нового пациента.
    /// Принимает ТОЛЬКО casePrompt (без basePrompt).
    /// </summary>
    public void UpdatePatientFromPrompt(string casePrompt)
    {
        ParsePatientData(casePrompt);

        // ---- ИМЯ ----
        if (patientNameText != null)
        {
            patientNameText.text = patientData.TryGetValue("Name", out var name)
                ? name
                : "Unknown patient";
        }

        // ---- ИНФОРМАЦИЯ ----
        if (patientInfoText != null)
        {
            patientInfoText.text = BuildInfoText();
        }
    }

    // ---------------- DATA ----------------

    private void ParsePatientData(string text)
    {
        patientData.Clear();
        if (string.IsNullOrEmpty(text)) return;

        var lines = text.Split('\n');

        // строка 1 — имя
        if (lines.Length > 0 && lines[0].Contains(":"))
        {
            var parts = lines[0].Split(':', 2);
            patientData["Name"] = parts[1].Trim();
        }

        // строки 2–7 — параметры
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

        return result.TrimEnd();
    }
}