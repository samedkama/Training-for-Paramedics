using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TriageType
{
    Green,
    Yellow,
    Red,
    Black
}

// Sex and age-group enums are used to map a case to a matching avatar sprite.
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
    // Stable id used by other systems (for example AnswerKey loading).
    public string scenarioId;

    [TextArea(3, 10)]
    // Raw case prompt that will be appended to the base system prompt.
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
    // Fired whenever a new scenario is selected, so dependent UI can reload.
    public System.Action<string> OnScenarioChanged;

    [Header("Patient Card UI")]
    public PatientCardUI patientCardUI;

    // Target image where the selected patient avatar sprite is shown.
    [Header("Patient Avatar UI")]
    [SerializeField] private Image patientAvatarImage;

    // Avatar variants selected by age group and sex parsed from the prompt.
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

    // Case-only part of the prompt (without the base system prompt).
    private string currentCasePrompt;

    // Prompt actually sent to the model (base + case).
    private string currentFullPrompt;

    private static readonly HttpClient client = new HttpClient();

    private void Start()
    {
        if (sendButton != null) sendButton.onClick.AddListener(SendUserMessage);
        if (newChatButton != null) newChatButton.onClick.AddListener(StartNewCase);

        StartNewCase();
    }

    // Starts a new random case, rebuilds the prompt and refreshes patient UI.
    private void StartNewCase()
    {
        if (chatHistory != null) chatHistory.text = "";
        if (inputField != null) inputField.text = "";
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0f;

        if (triageGroups == null || triageGroups.Count == 0)
        {
            currentCasePrompt = "";
            currentFullPrompt = basePrompt ?? "";

            if (patientCardUI != null)
                patientCardUI.UpdatePatientFromPrompt(currentCasePrompt);

            return;
        }

        int groupIndex = Random.Range(0, triageGroups.Count);
        TriagePromptGroup group = triageGroups[groupIndex];
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

        // Determine the avatar from "Age" and "Sex" values inside casePrompt.
        int age = ExtractAgeFromPrompt(currentCasePrompt);
        Sex sex = ExtractSexFromPrompt(currentCasePrompt);
        Sprite avatar = GetAvatarSprite(age, sex);

        if (patientAvatarImage != null && avatar != null)
        {
            patientAvatarImage.sprite = avatar;
        }

        // Update patient card texts (name + vitals block).
        if (patientCardUI != null)
            patientCardUI.UpdatePatientFromPrompt(currentCasePrompt);
    }
    
    // Sends the user's message and appends the assistant response to chat history.
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

    // Appends one chat line and keeps the scroll view at the latest message.
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

    // Calls the Chat Completions API using the current scenario prompt.
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
    
    // Parses "Age: XX" from the case prompt text.
    private int ExtractAgeFromPrompt(string text)
    {
        Match match = Regex.Match(text, @"Age:\s*(\d+)");
        if (match.Success)
            return int.Parse(match.Groups[1].Value);

        return -1;
    }

    // Parses "Sex: male/female" from the case prompt; defaults to Male.
    private Sex ExtractSexFromPrompt(string text)
    {
        if (Regex.IsMatch(text, @"Sex:\s*male", RegexOptions.IgnoreCase)) return Sex.Male;
        if (Regex.IsMatch(text, @"Sex:\s*female", RegexOptions.IgnoreCase)) return Sex.Female;

        return Sex.Male;
    }

    // Converts numeric age into a coarse age group used for avatar selection.
    private AgeGroup GetAgeGroup(int age)
    {
        if (age <= 12) return AgeGroup.Child;
        if (age <= 17) return AgeGroup.Teen;
        if (age <= 59) return AgeGroup.Adult;
        return AgeGroup.Elderly;
    }

    // Returns the best matching avatar sprite for the parsed patient profile.
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
