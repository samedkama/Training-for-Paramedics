using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PatientCardUI : MonoBehaviour
{
    [Header("UI")]
    // Main label for the patient display name.
    public TMP_Text patientNameText;
    // Multiline block with demographics and vital signs.
    public TMP_Text patientInfoText;

    // Parsed key/value pairs extracted from the case prompt.
    private Dictionary<string, string> patientData = new Dictionary<string, string>();

    /// <summary>
    /// Called by ChatManager when a new patient case starts.
    /// Expects only the case prompt (without the global base prompt).
    /// </summary>
    public void UpdatePatientFromPrompt(string casePrompt)
    {
        ParsePatientData(casePrompt);

        // Patient name is shown separately from the vitals/info block.
        if (patientNameText != null)
        {
            patientNameText.text = patientData.TryGetValue("Name", out var name)
                ? name
                : "Unknown patient";
        }

        // Build a clean, ordered list of clinical attributes.
        if (patientInfoText != null)
        {
            patientInfoText.text = BuildInfoText();
        }
    }

    // Parses "Key: Value" lines from the case prompt into patientData.
    // Expected format:
    // Line 1   -> Name
    // Lines 2+ -> Sex, Age, and vitals
    // Unknown keys are still kept, but the UI prints only known keys.

    private void ParsePatientData(string text)
    {
        patientData.Clear();
        if (string.IsNullOrEmpty(text)) return;

        var lines = text.Split('\n');

        // First line usually contains "Name: ...".
        if (lines.Length > 0 && lines[0].Contains(":"))
        {
            var parts = lines[0].Split(':', 2);
            patientData["Name"] = parts[1].Trim();
        }

        // Next lines contain the key clinical parameters used in the card.
        for (int i = 1; i < lines.Length && i <= 7; i++)
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

    // Builds the patient info text in a stable order, so UI layout is predictable.
    private string BuildInfoText()
    {
        string[] order =
        {
            "Sex",
            "Age",
            "Temperature",
            "Oxygen saturation",
            "Blood pressure",
            "Heart rate",
            "Respiratory rate"
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
