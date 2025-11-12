using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class SimpleChatGPT : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inputField;
    public TMP_Text chatHistory;
    public Button sendButton;
    public Button newChatButton;
    public ScrollRect scrollRect;

    [Header("API Settings")]
    public string apiKey;
    [TextArea(3, 5)] public string systemPrompt = "You are a friendly AI chatbot.";

    private static readonly HttpClient client = new HttpClient();

    private void Start()
    {
        sendButton.onClick.AddListener(SendUserMessage);
        newChatButton.onClick.AddListener(ClearChat);
    }

    private void ClearChat()
    {
        chatHistory.text = "";
        inputField.text = "";
        ScrollToBottom();
    }

    private async void SendUserMessage()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage)) return;

        AppendMessage("Me:" + userMessage);
        inputField.text = "";

        string aiResponse = await GetChatGPTResponse(userMessage);
        AppendMessage("Patient: " + aiResponse);
    }

    private void AppendMessage(string message)
    {
        chatHistory.text += message + "\n\n";
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private async Task<string> GetChatGPTResponse(string userInput)
    {
        string apiUrl = "https://api.openai.com/v1/chat/completions";

        var requestBody = new
        {
            model = "gpt-4-turbo",
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userInput }
            }
        };

        var jsonBody = JObject.FromObject(requestBody).ToString();
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            var response = await client.PostAsync(apiUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString);
            return json["choices"][0]["message"]["content"].ToString().Trim();
        }
        catch
        {
            return "⚠️ Error: could not get response.";
        }
    }
}