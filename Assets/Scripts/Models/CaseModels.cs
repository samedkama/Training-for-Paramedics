using System.Collections.Generic;

namespace VR.Triage.Models
{
    public class CaseDefinition
    {
        public int schemaVersion;
        public string caseId;
        public string version;
        public string start;
        public Dictionary<string, Node> nodes;
    }

    public class Node
    {
        public string type;
        public string guard;
        public string text;
        public List<Option> options;
        public List<string> actions;
        public string next;
    }

    public class Option
    {
        public string key;
        public string label;
        public Dictionary<string, object> effects;
        public string next;
    }

    public class TriageRuleSet
    {
        public int schemaVersion;
        public string ruleSetId;
        public List<Rule> redFlags;
        public List<Rule> yellowFlags;
        public string greenDefaultReason;
    }

    public class Rule
    {
        public string id;
        public string expr;
        public string reason;
    }

    public class TriageResult
    {
        public string category;
        public List<string> reasons = new();
        public List<string> matchedRules = new();
    }
}
