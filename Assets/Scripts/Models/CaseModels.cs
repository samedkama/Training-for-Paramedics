using System.Collections.Generic;

namespace VR.Triage.Models
{
    // Root model for one dialogue-case JSON document.
    public class CaseDefinition
    {
        public int schemaVersion; // Version control field for the JSON schema.
        public string caseId; // Example: polytrauma-01.
        public string version; // case version for updates
        public string start; // Starting node id (for example q_conscious).
        public Dictionary<string, Node> nodes; // Random-access map of all nodes by id.
    }

    // One dialogue node (question, action, info, end).
    public class Node
    {
        public string type; // Node type, for example question or action.
        public string guard; // Optional condition that must evaluate to true for visibility.
        public string text; // Text to be displayed in the node
        public List<Option> options; // List of options shown as Buttons for user
        public List<string> actions; // Action keys to execute on action/info nodes.
        public string next; // Default next node id.
    }

    // Selectable answer option for a question/end node.
    public class Option
    {
        public string key; // Internal key/name for the option.
        public string label; // Text to be displayed on the button
        public Dictionary<string, object> effects; // State changes applied when this option is selected.
        public string next; // Next node id when this option is selected.
    }

   /* public class TriageRuleSet
    {
        // Logic for triage evaluation
        public int schemaVersion; // Version control field for the JSON schema.
        public string ruleSetId; // Example: basic-triage-v1
        public List<Rule> redFlags; // Immediate red flags
        public List<Rule> yellowFlags; // less severe yellow flags
        public string greenDefaultReason;
    }

    public class Rule
    // A rule is a medical condition that is evaluated to pick a triage category.
    {
        public string id;
        public string expr; // Example expression: "bp_systolic < 90"
        public string reason; // Used to explain why the rule matched.
    }

    public class TriageResult
    {
        public string category; // "Red", "Yellow", "Green"
        public List<string> reasons = new(); // reasons for the triage category
        public List<string> matchedRules = new(); // matched rule ids
    }
   */
}
