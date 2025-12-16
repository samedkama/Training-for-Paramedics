using System.Threading.Tasks;
using VR.Triage.Models;

namespace VR.Triage.Core
{
    public interface IRepository
    {
        Task<CaseDefinition> LoadCaseAsync(string caseId); // Loads the Dialog Case Definition by its ID
        // Task<TriageRuleSet> LoadRuleSetAsync(string ruleSetId); // Loads the Triage Rule Set by its ID
    }

    public interface IActionRunner
    {
        Task<object> RunAsync(string actionKey); // Executes an action based on the provided action key might be 
        // unused depending on the implementation
    }

    public interface IPatientAdapter
    {
        Task<string> AskAsync(string userText); // Sends user input to the patient adapter and returns the response
                                                // might be unused depending on the implementation
    }
}
