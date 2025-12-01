using System.Collections.Generic;

namespace VR.Triage.Models
{
    public class CaseDefinition
    {
        public int schemaVersion; // version Controll for JSON Schema
        public string caseId; // for example polytrauma-01
        public string version; // case version for updates
        public string start; // starting node id for exmaple q_conscious
        public Dictionary<string, Node> nodes; // allows acces to every node
    }

    public class Node
    {
        public string type; // Type of the node f.e. questtion, action
        public string guard; // Condition if the node should be displayed f.e. 
        //only show if patient is conscious
        public string text; // Text to be displayed in the node
        public List<Option> options; // List of options shown as Buttons for user
        public List<string> actions; //
        public string next; // next node id for next node
    }

    public class Option
    {
        public string key; // internal key/name for the option
        public string label; // Text to be displayed on the button
        public Dictionary<string, object> effects; // State changes when option is selected
        public string next; // next node id when this option is selected
    }

    public class TriageRuleSet
    {
        // Logic for triage evaluation
        public int schemaVersion; // version Controll for JSON Schema
        public string ruleSetId; // for example basic-triage-v1
        public List<Rule> redFlags; // immidiate red flags
        public List<Rule> yellowFlags; // less severe yellow flags
        public string greenDefaultReason;
    }

    public class Rule
    // a rule is a medical Condition which gets evaluated, to choose triage category
    {
        public string id;
        public string expr; // a expression could be "bp_systolic < 90" if true the rule matches
        public string reason; // id and reason are used to explain the triage result
    }

    public class TriageResult
    {
        public string category; // "Red", "Yellow", "Green"
        public List<string> reasons = new(); // reasons for the triage category
        public List<string> matchedRules = new(); // matched rule ids
    }
}
