#if false

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public enum TriageType_BACKUP_beforeScenarioRefactor
{
    Green,
    Yellow,
    Red,
    Black
}

[System.Serializable]
public class TriagePromptGroup_BACKUP_beforeScenarioRefactor
{
    public TriageType triageType;
    [TextArea(3, 10)]
    public List<string> prompts = new List<string>();
}

public class ChatManager_BACKUP_beforeScenarioRefactor : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inputField;
    public TMP_Text chatHistory;
    public Button sendButton;
    public Button newChatButton;
    public ScrollRect scrollRect;

    [Header("Check Button")]
    public Button checkAnswerButton;

    [Header("Patient Card UI")]
    public PatientCardUI patientCardUI;

    [Header("API Settings")]
    public string apiKey;

    [Header("Base System Prompt")]
    [TextArea(3, 10)]
    public string basePrompt;

    [Header("Triage Prompt Groups")]
    public List<TriagePromptGroup> triageGroups;

    private TriageType currentTriage;

    // ✅ только кейс (первые строки с пациентом и т.п.)
    private string currentCasePrompt;

    // ✅ полный промпт для GPT (base + кейс)
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

        // safety
        if (triageGroups == null || triageGroups.Count == 0)
        {
            currentTriage = TriageType.Green;
            currentCasePrompt = "";
            currentFullPrompt = basePrompt ?? "";

            if (patientCardUI != null)
                patientCardUI.UpdatePatientFromPrompt(currentCasePrompt);

            return;
        }

        // Pick random triage group
        int groupIndex = Random.Range(0, triageGroups.Count);
        TriagePromptGroup group = triageGroups[groupIndex];
        currentTriage = group.triageType;

        // Pick random case prompt (without basePrompt)
        if (group.prompts != null && group.prompts.Count > 0)
        {
            int promptIndex = Random.Range(0, group.prompts.Count);
            currentCasePrompt = group.prompts[promptIndex] ?? "";
        }
        else
        {
            currentCasePrompt = "";
        }

        // Build full prompt for GPT
        if (!string.IsNullOrEmpty(basePrompt))
            currentFullPrompt = basePrompt + "\n" + currentCasePrompt;
        else
            currentFullPrompt = currentCasePrompt;

        // ✅ Update Patient UI ONLY with the case prompt
        if (patientCardUI != null)
            patientCardUI.UpdatePatientFromPrompt(currentCasePrompt);
        else
            Debug.LogWarning("ChatManager: patientCardUI reference is missing.");
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
}
#endif
