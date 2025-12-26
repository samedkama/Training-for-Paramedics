using System.Collections.Generic;

namespace VR.Triage.Models
{
    // Repräsentiert die JSON-Datei "alex-meyer.json"
    public class AnswerKeyDefinition
    {
        public int schemaVersion;
        public string scenarioId;
        public string triageExpected; // z.B. "Green"
        public Dictionary<string, string> answers; // nodeId -> expected optionKey
    }

    // Eine einzelne Abweichung: an welchem Node, was erwartet, was tatsächlich gewählt
    public class AnswerMismatch
    {
        public string nodeId;
        public string expectedKey;
        public string actualKey;
    }

    // Gesamtergebnis des Checks
    public class AnswerCheckResult
    {
        public bool isPerfect;
        public List<AnswerMismatch> mismatches = new();
    }
}
