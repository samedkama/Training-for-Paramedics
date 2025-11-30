using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

/// <summary>
/// Enum representing triage categories.
/// </summary>
public enum TriageType
{
    Green,
    Yellow,
    Red,
    Black
}

[System.Serializable]
public class TriagePromptGroup
{
    public TriageType triageType;       // Selected from dropdown
    [TextArea(3, 10)]
    public List<string> prompts = new List<string>();
}

public class ChatManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inputField;
    public TMP_Text chatHistory;
    public Button sendButton;
    public Button newChatButton;
    public ScrollRect scrollRect;

    [Header("Check Button")]
    public Button checkAnswerButton;

    [Header("API Settings")]
    public string apiKey;

    [Header("Base System Prompt")]
    [TextArea(3, 10)]
    public string basePrompt;

    [Header("Triage Prompt Groups")]
    public List<TriagePromptGroup> triageGroups;

    private TriageType currentTriage;
    private string currentFullPrompt;

    private static readonly HttpClient client = new HttpClient();

    private void Start()
    {
        sendButton.onClick.AddListener(SendUserMessage);
        newChatButton.onClick.AddListener(StartNewCase);
        checkAnswerButton.onClick.AddListener(OnCheckAnswer);

        StartNewCase();
    }

    // -----------------------------------------------------
    // NEW PATIENT CASE (RANDOM TRIAGE)
    // -----------------------------------------------------
    private void StartNewCase()
    {
        chatHistory.text = "";
        inputField.text = "";
        scrollRect.verticalNormalizedPosition = 0f;

        ResetCheckButton();

        // Pick random triage group
        int groupIndex = Random.Range(0, triageGroups.Count);
        TriagePromptGroup group = triageGroups[groupIndex];

        currentTriage = group.triageType;

        // Pick random prompt
        if (group.prompts.Count > 0)
        {
            int promptIndex = Random.Range(0, group.prompts.Count);
            currentFullPrompt = basePrompt + "\n" + group.prompts[promptIndex];
        }
        else
        {
            currentFullPrompt = basePrompt;
        }

        
    }

    private void ResetCheckButton()
    {
        // Reset text back to default
        TMP_Text label = checkAnswerButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = "Check yourself";

        checkAnswerButton.interactable = true;
    }
    // -----------------------------------------------------
    // CHAT SENDING
    // -----------------------------------------------------
    private async void SendUserMessage()
    {
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        AppendMessage("You: " + msg);
        inputField.text = "";

        string reply = await GetChatGPTResponse(msg);
        AppendMessage("Patient: " + reply);
    }

    private void AppendMessage(string msg)
    {
        chatHistory.text += msg + "\n\n";
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
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

            return data["choices"][0]["message"]["content"].ToString().Trim();
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
        // Update button text
        TMP_Text label = checkAnswerButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
            label.text = currentTriage.ToString();

        // Change button color based on triage
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
