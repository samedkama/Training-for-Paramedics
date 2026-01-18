using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions; // ADDED: for parsing Age from text

public enum TriageType
{
    Green,
    Yellow,
    Red,
    Black
}

// ===============================
// ADDED: enums for avatar logic
// ===============================
public enum Sex
{
    Male,
    Female
}

public enum AgeGroup
{
    Child,
    Teen,
    Adult,
    Elderly
}

[System.Serializable]
public class ScenarioEntry
{
    public string scenarioId;

    [TextArea(3, 10)]
    public string casePrompt;
}

[System.Serializable]
public class TriagePromptGroup
{
    public TriageType triageType;
    public List<ScenarioEntry> scenarios = new List<ScenarioEntry>();
}

public class ChatManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inputField;
    public TMP_Text chatHistory;
    public Button sendButton;
    public Button newChatButton;
    public ScrollRect scrollRect;

    [Header("Scenario")]
    public string CurrentScenarioId { get; private set; }
    public System.Action<string> OnScenarioChanged;

    [Header("Check Button")]
    public Button checkAnswerButton;

    [Header("Patient Card UI")]
    public PatientCardUI patientCardUI;

    // =====================================================
    // ADDED: Patient avatar Image (assigned via Inspector)
    // =====================================================
    [Header("Patient Avatar UI")]
    [SerializeField] private Image patientAvatarImage;

    // =====================================================
    // ADDED: Avatar sprites (assigned via Inspector)
    // =====================================================
    [Header("Patient Avatars (Age + Sex)")]
    [SerializeField] private Sprite childMale;
    [SerializeField] private Sprite childFemale;
    [SerializeField] private Sprite teenMale;
    [SerializeField] private Sprite teenFemale;
    [SerializeField] private Sprite adultMale;
    [SerializeField] private Sprite adultFemale;
    [SerializeField] private Sprite elderlyMale;
    [SerializeField] private Sprite elderlyFemale;

    [Header("API Settings")]
    public string apiKey;

    [Header("Base System Prompt")]
    [TextArea(3, 10)]
    public string basePrompt;

    [Header("Triage Prompt Groups")]
    public List<TriagePromptGroup> triageGroups;

    private TriageType currentTriage;

    // only the patient case text
    private string currentCasePrompt;

    // full prompt (base + case)
    private string currentFullPrompt;

    private static readonly HttpClient client = new HttpClient();

    private void Start()
    {
        if (sendButton != null) sendButton.onClick.AddListener(SendUserMessage);
        if (newChatButton != null) newChatButton.onClick.AddListener(StartNewCase);
        if (checkAnswerButton != null) checkAnswerButton.onClick.AddListener(OnCheckAnswer);

        StartNewCase();
    }

    // -----------------------------------------------------
    // NEW PATIENT CASE (RANDOM TRIAGE)
    // -----------------------------------------------------
    private void StartNewCase()
    {
        if (chatHistory != null) chatHistory.text = "";
        if (inputField != null) inputField.text = "";
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;

        ResetCheckButton();

        if (triageGroups == null || triageGroups.Count == 0)
        {
            currentTriage = TriageType.Green;
            currentCasePrompt = "";
            currentFullPrompt = basePrompt ?? "";

            if (patientCardUI != null)
                patientCardUI.UpdatePatientFromPrompt(currentCasePrompt);

            return;
        }

        int groupIndex = Random.Range(0, triageGroups.Count);
        TriagePromptGroup group = triageGroups[groupIndex];
        currentTriage = group.triageType;
        if (group.scenarios != null && group.scenarios.Count > 0)
        {
            int idx = Random.Range(0, group.scenarios.Count);
            var entry = group.scenarios[idx];

            CurrentScenarioId = entry.scenarioId;
            currentCasePrompt = entry.casePrompt ?? "";
        }
        else
        {
            CurrentScenarioId = "";
            currentCasePrompt = "";
        }

        OnScenarioChanged?.Invoke(CurrentScenarioId);

        if (!string.IsNullOrEmpty(basePrompt))
            currentFullPrompt = basePrompt + "\n" + currentCasePrompt;
        else
            currentFullPrompt = currentCasePrompt;

        // =====================================================
        // ADDED: determine avatar from Age + Sex in case prompt
        // =====================================================
        int age = ExtractAgeFromPrompt(currentCasePrompt);
        Sex sex = ExtractSexFromPrompt(currentCasePrompt);
        Sprite avatar = GetAvatarSprite(age, sex);

        if (patientAvatarImage != null && avatar != null)
        {
            patientAvatarImage.sprite = avatar;
        }

        // existing behavior (unchanged)
        if (patientCardUI != null)
            patientCardUI.UpdatePatientFromPrompt(currentCasePrompt);
    }

    private void ResetCheckButton()
    {
        if (checkAnswerButton == null) return;

        TMP_Text label = checkAnswerButton.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = "Check yourself";

        checkAnswerButton.interactable = true;
    }

    // -----------------------------------------------------
    // CHAT SENDING
    // -----------------------------------------------------
    private async void SendUserMessage()
    {
        if (inputField == null || chatHistory == null) return;

        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        AppendMessage("You: " + msg);
        inputField.text = "";

        string reply = await GetChatGPTResponse(msg);
        AppendMessage("Patient: " + reply);
    }

    private void AppendMessage(string msg)
    {
        if (chatHistory == null) return;

        chatHistory.text += msg + "\n\n";
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // -----------------------------------------------------
    // OPENAI REQUEST
    // -----------------------------------------------------
    private async Task<string> GetChatGPTResponse(string userInput)
    {
        string url = "https://api.openai.com/v1/chat/completions";

        var body = new
        {
            model = "gpt-4o-mini",
            messages = new object[]
            {
                new { role = "system", content = currentFullPrompt },
                new { role = "user", content = userInput }
            }
        };

        string json = JObject.FromObject(body).ToString();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(responseString);

            return data["choices"]?[0]?["message"]?["content"]?.ToString().Trim()
                   ?? "Error: Empty response.";
        }
        catch
        {
            return "Error: Could not get response.";
        }
    }

    // -----------------------------------------------------
    // CHECK ANSWER BUTTON
    // -----------------------------------------------------
    private void OnCheckAnswer()
    {
        if (checkAnswerButton == null) return;
       TMP_Text label = checkAnswerButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = currentTriage.ToString();

        Color c = Color.white;

        switch (currentTriage)
        {
            case TriageType.Green:  c = new Color(0.29f, 0.69f, 0.31f); break;
            case TriageType.Yellow: c = new Color(1f, 0.92f, 0.23f); break;
            case TriageType.Red:    c = new Color(0.96f, 0.26f, 0.21f); break;
            case TriageType.Black:  c = new Color(0.13f, 0.13f, 0.13f); break;
        }

        checkAnswerButton.image.color = c;
    }

    // =====================================================
    // ADDED: parsing and avatar selection helpers
    // =====================================================

    // Parses "Age: XX" from the case prompt
    private int ExtractAgeFromPrompt(string text)
    {
        Match match = Regex.Match(text, @"Age:\s*(\d+)");
        if (match.Success)
            return int.Parse(match.Groups[1].Value);

        return -1;
    }

    // Parses "Sex: male / female" from the case prompt
    private Sex ExtractSexFromPrompt(string text)
    {
        if (Regex.IsMatch(text, @"Sex:\s*male", RegexOptions.IgnoreCase)) return Sex.Male;
        if (Regex.IsMatch(text, @"Sex:\s*female", RegexOptions.IgnoreCase)) return Sex.Female;

        return Sex.Male;
    }

    // Converts numeric age into age group
    private AgeGroup GetAgeGroup(int age)
    {
        if (age <= 12) return AgeGroup.Child;
        if (age <= 17) return AgeGroup.Teen;
        if (age <= 59) return AgeGroup.Adult;
        return AgeGroup.Elderly;
    }

    // Returns the correct avatar sprite based on age group and sex
    private Sprite GetAvatarSprite(int age, Sex sex)
    {
        AgeGroup group = GetAgeGroup(age);

        switch (group)
        {
            case AgeGroup.Child:
                return sex == Sex.Male ? childMale : childFemale;
            case AgeGroup.Teen:
                return sex == Sex.Male ? teenMale : teenFemale;
            case AgeGroup.Adult:
                return sex == Sex.Male ? adultMale : adultFemale;
            case AgeGroup.Elderly:
                return sex == Sex.Male ? elderlyMale : elderlyFemale;
        }

        return null;
    }
} 
