using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VR.Triage.Engine;
using VR.Triage.Repo;
using VR.Triage.Models;
using System.Linq;

public class SimpleUIController : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI nodeText;
    public Transform optionsParent;
    public Button optionButtonPrefab;
    public TextMeshProUGUI triageText;
    public ChatManager chatManager;

    [Header("Vitals UI")]
    public GameObject vitalsPanel;
    public TMP_InputField respRateInput;
    public TMP_InputField painScaleInput;

    [Header("AnswerKey")]
    public string scenarioId;
    private AnswerKeyDefinition _answerKey;

    [Header("Result Overlay")]
    public GameObject resultPanel;
    public Image resultHeaderImage;
    public TextMeshProUGUI resultTitleText;
    public TextMeshProUGUI resultBodyText;
    public Button resultCloseButton;

    [Header("Optional")]
    public GameObject dialogueRoot;


    DialogueEngine _dialogue;
    // TriageEngine _triage; // not used at the moment

    async void Start()
    {
        if (resultCloseButton != null)
        {
            resultCloseButton.onClick.RemoveAllListeners();
            resultCloseButton.onClick.AddListener(CloseResultOverlay);
        }
        else
        {
            Debug.LogWarning("[UI] resultCloseButton not linked.");
        }


        if (chatManager != null)
        {
            chatManager.OnScenarioChanged += HandleScenarioChanged;
        }
        else
        {
            Debug.LogWarning("[SimpleUI] chatManager is NULL. Link it in Inspector.");
        }

        var repo = new TextAssetRepository();
        _dialogue = new DialogueEngine(repo, null);
     

        await _dialogue.LoadAsync("polytrauma-01");
        Debug.Log($"[SimpleUI] chatManager ref null? {chatManager == null}");
        Debug.Log($"[SimpleUI] CurrentScenarioId BEFORE wait = '{(chatManager != null ? chatManager.CurrentScenarioId : "NULL")}'");


        await Task.Yield();
        if (chatManager != null && !string.IsNullOrEmpty(chatManager.CurrentScenarioId))
        {
            scenarioId = chatManager.CurrentScenarioId;
            Debug.Log($"[SimpleUI] scenarioId AFTER set = '{scenarioId}'");
        }


        var akRepo = new TextAssetAnswerKeyRepository();
        try
        {
            _answerKey = await akRepo.LoadAnswerKeyAsync(scenarioId);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"AnswerKey not found for scenarioId='{scenarioId}'. {e.Message}");
            _answerKey = null;
        }



        if (vitalsPanel != null)
            vitalsPanel.SetActive(false);
        RenderNode();
        // Overlay am Anfang verstecken
        if (resultPanel != null)
            resultPanel.SetActive(false);

    }

    void ClearOptions()
    {
        foreach (Transform c in optionsParent) Destroy(c.gameObject);
    }

    void RenderNode() // render current dialogue node
    {
        var node = _dialogue.GetCurrentNode();
        Debug.Log($"[UI] NodeId={_dialogue.CurrentNodeId}, type={node.type}");
        nodeText.text = node.text; // set node text

        ClearOptions(); // remove old buttons
        vitalsPanel.SetActive(false); // hide vitals panel by default

        if (node.type == "question" && node.options != null)
        {
            bool isTriageSelect = _dialogue.CurrentNodeId == "triage_select";

            foreach (var opt in node.options)
            {
                var btn = Instantiate(optionButtonPrefab, optionsParent);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = opt.label;

                // ✅ nur bei der Triage-Frage: Buttons färben
                if (isTriageSelect)
                    ApplyTriageColor(btn, opt.key);

                btn.onClick.AddListener(async () =>
                {
                    await _dialogue.SubmitAnswerAsync(opt.key);
                    RenderNode();
                });
            }
        }

        else if (node.type == "action" ) // if action or info node, just add a "continue" button
        {
            // Beispiel: wenn diese Action "check_vitals" enthält → Vitalpanel öffnen
            if (node.actions != null && node.actions.Contains("check_vitals"))
            {
                vitalsPanel.SetActive(true);

                // Optional: vorhandene Werte aus State ins UI laden
                if (_dialogue.State.TryGetValue("respRate", out var rr))
                    respRateInput.text = rr.ToString();
                if (_dialogue.State.TryGetValue("painScale", out var ps))
                    painScaleInput.text = ps.ToString();
            }
            else
            {
                // Fallback: nur "Weiter"-Button wie vorher
                var btn = Instantiate(optionButtonPrefab, optionsParent);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = "Weiter";
                btn.onClick.AddListener(async () =>
                {
                    await _dialogue.SubmitAnswerAsync("next");
                    RenderNode();
                });
            }
        }
        else if (node.type == "info")
        {
            var btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = "Weiter";
            btn.onClick.AddListener(async () =>
            {
                await _dialogue.SubmitAnswerAsync("next");
                RenderNode();
            });
        }
        else if (node.type == "end")
        {
            ShowResultOverlay();
            return;
        }

    }
    // Wird vom Button im VitalsPanel aufgerufen
    public async void OnVitalsConfirmed()
    {
        // Werte aus den Eingabefeldern in den State schreiben
        if (int.TryParse(respRateInput.text, out var rr))
            _dialogue.State["respRate"] = rr;

        if (int.TryParse(painScaleInput.text, out var ps))
        {
            _dialogue.State["painScale"] = ps;
            _dialogue.State["pain"] = ps >= 1; // pain = true, wenn Skala ≥ 1
        }

        if (vitalsPanel != null)
            vitalsPanel.SetActive(false);

        // Action-Node „abschließen“ und weitergehen
        await _dialogue.SubmitAnswerAsync("next");
        RenderNode();
    }
    string GetNodeText(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return "(unknown node)";
        if (_dialogue?.Case?.nodes == null) return nodeId;

        if (_dialogue.Case.nodes.TryGetValue(nodeId, out var node) && node != null)
        {
            if (!string.IsNullOrWhiteSpace(node.text))
                return node.text;
        }

        return nodeId;
    }
    private async void HandleScenarioChanged(string newScenarioId)
    {
        if (string.IsNullOrWhiteSpace(newScenarioId))
        {
            Debug.LogWarning("[SimpleUI] Scenario changed but id is empty.");
            return;
        }

        scenarioId = newScenarioId;
        Debug.Log($"[SimpleUI] Scenario changed -> scenarioId='{scenarioId}'");

        // AnswerKey neu laden
        var akRepo = new TextAssetAnswerKeyRepository();
        try
        {
            _answerKey = await akRepo.LoadAnswerKeyAsync(scenarioId);
            Debug.Log($"[SimpleUI] Reloaded AnswerKey for {scenarioId}");
        }
        catch (System.Exception e)
        {
            _answerKey = null;
            Debug.LogWarning($"[SimpleUI] No AnswerKey for {scenarioId}: {e.Message}");
        }

        // Dialog neu starten und UI neu rendern
        await _dialogue.LoadAsync("polytrauma-01");
        RenderNode();
    }
    void ApplyTriageColor(Button btn, string triageKey)
    {
        if (btn == null) return;

        var img = btn.image;
        if (img == null) return;

        switch (triageKey)
        {
            case "Green":
                img.color = new Color(0.29f, 0.69f, 0.31f);
                break;
            case "Yellow":
                img.color = new Color(1f, 0.92f, 0.23f);
                break;
            case "Red":
                img.color = new Color(0.96f, 0.26f, 0.21f);
                break;
            default:
                img.color = Color.white;
                break;
        }
    }
    void CloseResultOverlay()
    {
        Debug.Log("[UI] CloseResultOverlay clicked");

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        // WICHTIG: NICHT RenderNode(); sonst öffnet sich das Overlay sofort wieder (weil current node == "end")
    }

    void ShowResultOverlay()
    {
        if (resultPanel == null || resultHeaderImage == null || resultTitleText == null || resultBodyText == null)
        {
            Debug.LogWarning("[UI] Result overlay references not set in Inspector.");
            return;
        }

        // normales UI ausblenden
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        resultPanel.SetActive(true);

        // Falls AnswerKey fehlt
        if (_answerKey == null)
        {
            resultHeaderImage.color = new Color(0.5f, 0.5f, 0.5f);
            resultTitleText.text = "CASE COMPLETED";
            resultBodyText.text = "No AnswerKey loaded.\n\nMake sure an AnswerKey JSON exists for this scenario.";
            return;
        }

        var check = AnswerKeyJudge.Compare(_answerKey, _dialogue.ChosenOptionsByNodeId);

        // user triage aus den Klicks holen
        var yourTriage = _dialogue.ChosenOptionsByNodeId.TryGetValue("triage_select", out var t) ? t : "(not selected)";

        if (check.isPerfect)
        {
            resultHeaderImage.color = new Color(0.29f, 0.69f, 0.31f);
            resultTitleText.text = "CORRECT TRIAGE";
            resultBodyText.text =
                $"Expected triage: {_answerKey.triageExpected}\n" +
                $"Your triage: {yourTriage}\n\n" +
                "Nice job. All answers matched the predefined key.";
        }
        else
        {
            resultHeaderImage.color = new Color(0.96f, 0.26f, 0.21f);
            resultTitleText.text = "WRONG TRIAGE / DEVIATIONS";

            var lines = check.mismatches
                .Take(20)
                .Select((m, i) =>
                {
                    var question = GetNodeText(m.nodeId);
                    return $"{i + 1}) {question}\n   expected: {m.expectedKey}\n   got: {m.actualKey}\n";
                });

            resultBodyText.text =
                $"Expected triage: {_answerKey.triageExpected}\n" +
                $"Your triage: {yourTriage}\n\n" +
                $"Mismatches ({check.mismatches.Count}):\n\n" +
                string.Join("\n", lines);
        }
    }


}
