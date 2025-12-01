using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VR.Triage.Engine;
using VR.Triage.Repo;

public class SimpleUIController : MonoBehaviour
{
    [Header("Refs")]
    public TextMeshProUGUI nodeText;
    public Transform optionsParent;
    public Button optionButtonPrefab;
    public TextMeshProUGUI triageText;

    [Header("Vitals UI")]
    public GameObject vitalsPanel;
    public TMP_InputField respRateInput;
    public TMP_InputField painScaleInput;

    DialogueEngine _dialogue;
    TriageEngine _triage;

    async void Start()
    {
        var repo = new TextAssetRepository();
        _dialogue = new DialogueEngine(repo, null);
        _triage = new TriageEngine(repo);

        await _dialogue.LoadAsync("polytrauma-01");
        await _triage.LoadRulesAsync("basic-1");


        if (vitalsPanel != null)
            vitalsPanel.SetActive(false);
        RenderNode();
    }

    void ClearOptions()
    {
        foreach (Transform c in optionsParent) Destroy(c.gameObject);
    }

    void RenderNode() // render current dialogue node
    {
        var node = _dialogue.GetCurrentNode();
        nodeText.text = node.text; // set node text

        ClearOptions(); // remove old buttons
        vitalsPanel.SetActive(false); // hide vitals panel by default

        if (node.type == "question" && node.options != null)
        {
            foreach (var opt in node.options) // generate buttons for each question option
            {
                var btn = Instantiate(optionButtonPrefab, optionsParent);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = opt.label; // option label is what user sees
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
        else if (node.type == "end") // if end node, show triage result
        {
            var result = _triage.Evaluate(_dialogue.State);
            triageText.text = $"System Triage: <b>{result.category}</b>\n- " + string.Join("\n- ", result.reasons);
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
}
