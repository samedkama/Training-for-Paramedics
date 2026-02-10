using System.Collections.Generic;

namespace VR.Triage.Models
{
    // Represents one AnswerKey JSON document for a scenario.
    public class AnswerKeyDefinition
    {
        public int schemaVersion;
        public string scenarioId;
        public string triageExpected; // Example: "Green"
        public Dictionary<string, string> answers; // nodeId -> expected option key
    }

    // One mismatch between expected and actual user choice.
    public class AnswerMismatch
    {
        public string nodeId;
        public string expectedKey;
        public string actualKey;
    }

    // Final comparison result with a mismatch list.
    public class AnswerCheckResult
    {
        public bool isPerfect;
        public List<AnswerMismatch> mismatches = new();
    }
}
