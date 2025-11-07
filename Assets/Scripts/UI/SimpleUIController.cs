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

    DialogueEngine _dialogue;
    TriageEngine _triage;

    async void Start()
    {
        var repo = new TextAssetRepository();
        _dialogue = new DialogueEngine(repo, new DummyActions());
        _triage = new TriageEngine(repo);

        await _dialogue.LoadAsync("polytrauma-01");
        await _triage.LoadRulesAsync("basic-1");

        RenderNode();
    }

    void ClearOptions()
    {
        foreach (Transform c in optionsParent) Destroy(c.gameObject);
    }

    void RenderNode()
    {
        var node = _dialogue.GetCurrentNode();
        nodeText.text = node.text;

        ClearOptions();

        if (node.type == "question" && node.options != null)
        {
            foreach (var opt in node.options)
            {
                var btn = Instantiate(optionButtonPrefab, optionsParent);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = opt.label;
                btn.onClick.AddListener(async () =>
                {
                    await _dialogue.SubmitAnswerAsync(opt.key);
                    RenderNode();
                });
            }
        }
        else if (node.type == "action" || node.type == "info")
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
            var result = _triage.Evaluate(_dialogue.State);
            triageText.text = $"System Triage: <b>{result.category}</b>\n- " + string.Join("\n- ", result.reasons);
        }
    }

    class DummyActions : VR.Triage.Core.IActionRunner
    {
        public async Task<object> RunAsync(string actionKey)
        {
            await Task.Yield();
            return actionKey switch
            {
                "measure_resp_rate" => 12,
                _ => null
            };
        }
    }
}
