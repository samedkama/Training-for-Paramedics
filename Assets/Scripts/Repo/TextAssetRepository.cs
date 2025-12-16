using System.Threading.Tasks;
using UnityEngine;
using VR.Triage.Core;
using VR.Triage.Models;
using Newtonsoft.Json;

namespace VR.Triage.Repo
{
    public class TextAssetRepository : IRepository
    {
        public async Task<CaseDefinition> LoadCaseAsync(string caseId)
        {
            var ta = Resources.Load<TextAsset>($"Cases/{caseId}");
            var json = ta != null ? ta.text : throw new System.Exception($"Case not found: {caseId}"); // Error handling
            await Task.Yield(); // Simulate async operation, waits until the ui is loaded 
            return JsonConvert.DeserializeObject<CaseDefinition>(json); // Convert JSON to CaseDefinition object, kind of like parsing
        }
        /*
        public async Task<TriageRuleSet> LoadRuleSetAsync(string ruleSetId)
        // same thing here but for triage rules
        {
            var ta = Resources.Load<TextAsset>($"Cases/triage-rules-{ruleSetId}");
            if (ta == null) ta = Resources.Load<TextAsset>($"Cases/{ruleSetId}");
            var json = ta != null ? ta.text : throw new System.Exception($"RuleSet not found: {ruleSetId}");
            await Task.Yield();
            return JsonConvert.DeserializeObject<TriageRuleSet>(json);
        }
        */
    }
}
