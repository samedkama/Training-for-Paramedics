using System.Threading.Tasks;
using VR.Triage.Models;

namespace VR.Triage.Core
{
    public interface IRepository
    {
        Task<CaseDefinition> LoadCaseAsync(string caseId);
        Task<TriageRuleSet> LoadRuleSetAsync(string ruleSetId);
    }

    public interface IActionRunner
    {
        Task<object> RunAsync(string actionKey);
    }

    public interface IPatientAdapter
    {
        Task<string> AskAsync(string userText);
    }
}
