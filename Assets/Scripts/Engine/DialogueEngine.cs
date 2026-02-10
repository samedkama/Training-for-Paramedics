using System.Collections.Generic;
using System.Threading.Tasks;
using VR.Triage.Core;
using VR.Triage.Models;
using UnityEngine;



namespace VR.Triage.Engine
{
    public class DialogueEngine

    // DialogueEngine controls the flow through the case:
    // - loads the case definition
    // - tracks the current node
    // - applies options & actions
    // - manages the shared state dictionary
    {
        readonly IRepository _repo;
        readonly IActionRunner _actions;
        // Currently loaded case definition (all nodes, start node, etc.)
        public CaseDefinition Case { get; private set; }
        // ID of the node that is currently active
        public string CurrentNodeId { get; private set; }
        // Shared state for the dialogue (vital signs, flags, etc.)
        // will be used for triage evaluation
        public Dictionary<string, object> State { get; } = new();
        public Dictionary<string, string> ChosenOptionsByNodeId { get; } = new(); // Stores selected option keys per node id.


        public DialogueEngine(IRepository repo, IActionRunner actions)
        {
            _repo = repo; _actions = actions;
        }
        // Loads a case by its ID and sets the current node to the case's start node
        public async Task LoadAsync(string caseId)
        {
            Case = await _repo.LoadCaseAsync(caseId);
            CurrentNodeId = Case.start;
        }

        // Returns the currently active node from the case
        public Node GetCurrentNode() => Case.nodes[CurrentNodeId];

        // Processes an answer or advances an action node.
        // Returns true if the input was valid and the engine advanced.
        public async Task<bool> SubmitAnswerAsync(string optionKeyOrValue)
        {
            var node = GetCurrentNode();

            // 1) Question node: validate option and apply option effects to state.
            if (node.type == "question")
            {
                var opt = node.options.Find(o => o.key == optionKeyOrValue);
                if (opt == null) return false;
                ChosenOptionsByNodeId[CurrentNodeId] = opt.key;
               


                if (opt.effects != null)
                {
                    foreach (var kv in opt.effects)
                        State[kv.Key] = kv.Value;
                }

                CurrentNodeId = NextVisibleNodeId(opt.next);
                return true;
            }
            // 2) Action or info node: execute actions (if any) and continue.
            //    The UI layer handles panels and interactive controls.
            else if (node.type == "action" || node.type == "info")
            {
                // Optional action execution path.
                if (_actions != null && node.actions != null)
                {
                    foreach (var a in node.actions)
                    {
                        var val = await _actions.RunAsync(a);
                        State[a] = val;

                        if (a == "measure_resp_rate")
                            State["respRate"] = val;
                    }
                }

                CurrentNodeId = NextVisibleNodeId(node.next);
                return true;
            }
            else if (node.type == "end")
            {
                // End nodes may also contain options (for example triage selection).
                if (node.options != null && node.options.Count > 0)
                {
                    var opt = node.options.Find(o => o.key == optionKeyOrValue);
                    if (opt == null) return false;

                    // Record which option key was selected at this node.
                    ChosenOptionsByNodeId[CurrentNodeId] = opt.key;

                    // Apply option effects to state.
                    if (opt.effects != null)
                    {
                        foreach (var kv in opt.effects)
                            State[kv.Key] = kv.Value;
                    }

                    // Stay on the end node (no next transition).
                    return true;
                }

                return false;
            }


            // Unknown node type.
            return false;
        }


        string NextVisibleNodeId(string candidate)
        // Searches for the next node that passes its guard condition
        {
            while (true)
            {
                var n = Case.nodes[candidate]; // Candidate node referenced by current "next" pointer.
                bool ok = VR.Triage.Core.SimpleExpr.EvaluateBool(n.guard, State);
                if (ok) return candidate;
                if (!string.IsNullOrEmpty(n.next)) { candidate = n.next; continue; }
                return candidate;
            }
        }
    }
}
