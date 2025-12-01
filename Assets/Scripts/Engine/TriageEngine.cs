using System.Collections.Generic;
using System.Threading.Tasks;
using VR.Triage.Core;
using VR.Triage.Models;

namespace VR.Triage.Engine
{
    public class TriageEngine
    {
        // TriageEngine uses a repository to load triage rules and evaluates patient state
        // against these rules to determine triage category.
        readonly IRepository _repo;
        TriageRuleSet _rules;

        public TriageEngine(IRepository repo) { _repo = repo; }

        public async Task LoadRulesAsync(string ruleSetId)
        {
            _rules = await _repo.LoadRuleSetAsync(ruleSetId);
        }

        public TriageResult Evaluate(IDictionary<string, object> state)
        {
            var res = new TriageResult();
            var redHits = Match(_rules.redFlags, state); // gets the info from dialouge if the user klicks on button 
            if (redHits.reasons.Count > 0)
            {
                res.category = "Red";
                res.reasons.AddRange(redHits.reasons);
                res.matchedRules.AddRange(redHits.ids);
                return res;
            }
            var yellowHits = Match(_rules.yellowFlags, state);
            if (yellowHits.reasons.Count > 0)
            {
                res.category = "Yellow";
                res.reasons.AddRange(yellowHits.reasons);
                res.matchedRules.AddRange(yellowHits.ids);
                return res;
            }
            res.category = "Green";
            res.reasons.Add(_rules.greenDefaultReason ?? "No issues found");
            return res;
        }

        // creates a list of matched rule ids and reasons based on evaluating the rules against the state
        (List<string> ids, List<string> reasons) Match(List<Rule> rules, IDictionary<string, object> state)
        {
            var ids = new List<string>();
            var reasons = new List<string>();
            if (rules == null) return (ids, reasons);
            foreach (var r in rules)
                if (SimpleExpr.EvaluateBool(r.expr, state)) { ids.Add(r.id); reasons.Add(r.reason); }
            return (ids, reasons);
        }
    }
}
