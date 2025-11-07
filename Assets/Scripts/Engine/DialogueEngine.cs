using System.Collections.Generic;
using System.Threading.Tasks;
using VR.Triage.Core;
using VR.Triage.Models;

namespace VR.Triage.Engine
{
    public class DialogueEngine
    {
        readonly IRepository _repo;
        readonly IActionRunner _actions;
        public CaseDefinition Case { get; private set; }
        public string CurrentNodeId { get; private set; }
        public Dictionary<string, object> State { get; } = new();

        public DialogueEngine(IRepository repo, IActionRunner actions)
        {
            _repo = repo; _actions = actions;
        }

        public async Task LoadAsync(string caseId)
        {
            Case = await _repo.LoadCaseAsync(caseId);
            CurrentNodeId = Case.start;
        }

        public Node GetCurrentNode() => Case.nodes[CurrentNodeId];

        public async Task<bool> SubmitAnswerAsync(string optionKeyOrValue)
        {
            var node = GetCurrentNode();
            if (node.type == "question")
            {
                var opt = node.options.Find(o => o.key == optionKeyOrValue);
                if (opt == null) return false;
                if (opt.effects != null)
                    foreach (var kv in opt.effects) State[kv.Key] = kv.Value;

                CurrentNodeId = NextVisibleNodeId(opt.next);
                return true;
            }
            else if (node.type == "action")
            {
                if (node.actions != null)
                {
                    foreach (var a in node.actions)
                    {
                        var val = await _actions.RunAsync(a);
                        State[a] = val;
                        if (a == "measure_resp_rate") State["respRate"] = val;
                    }
                }
                CurrentNodeId = NextVisibleNodeId(node.next);
                return true;
            }
            return false;
        }

        string NextVisibleNodeId(string candidate)
        {
            while (true)
            {
                var n = Case.nodes[candidate];
                bool ok = VR.Triage.Core.SimpleExpr.EvaluateBool(n.guard, State);
                if (ok) return candidate;
                if (!string.IsNullOrEmpty(n.next)) { candidate = n.next; continue; }
                return candidate;
            }
        }
    }
}
