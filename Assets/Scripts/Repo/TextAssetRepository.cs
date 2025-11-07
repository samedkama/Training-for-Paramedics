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
            var json = ta != null ? ta.text : throw new System.Exception($"Case not found: {caseId}");
            await Task.Yield();
            return JsonConvert.DeserializeObject<CaseDefinition>(json);
        }

        public async Task<TriageRuleSet> LoadRuleSetAsync(string ruleSetId)
        {
            var ta = Resources.Load<TextAsset>($"Cases/triage-rules-{ruleSetId}");
            if (ta == null) ta = Resources.Load<TextAsset>($"Cases/{ruleSetId}");
            var json = ta != null ? ta.text : throw new System.Exception($"RuleSet not found: {ruleSetId}");
            await Task.Yield();
            return JsonConvert.DeserializeObject<TriageRuleSet>(json);
        }
    }
}
