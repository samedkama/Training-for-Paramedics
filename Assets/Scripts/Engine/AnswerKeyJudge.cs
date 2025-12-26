using System.Collections.Generic;
using System.Linq;
using VR.Triage.Models;

namespace VR.Triage.Engine
{
    public static class AnswerKeyJudge
    {
        // key = Soll (AnswerKey JSON)
        // chosen = Ist (User klicks aus DialogueEngine)
        public static AnswerCheckResult Compare(AnswerKeyDefinition key, Dictionary<string, string> chosen)
        {
            var result = new AnswerCheckResult();

            if (key == null || key.answers == null)
            {
                result.isPerfect = false;
                result.mismatches.Add(new AnswerMismatch
                {
                    nodeId = "(AnswerKey)",
                    expectedKey = "(loaded)",
                    actualKey = "(missing/invalid)"
                });
                return result;
            }

            if (chosen == null)
            {
                result.isPerfect = false;
                result.mismatches.Add(new AnswerMismatch
                {
                    nodeId = "(UserChoices)",
                    expectedKey = "(exists)",
                    actualKey = "(null)"
                });
                return result;
            }

            // Wir prüfen nur das, was im AnswerKey definiert ist
            foreach (var kv in key.answers)
            {
                var nodeId = kv.Key;
                var expected = kv.Value;

                chosen.TryGetValue(nodeId, out var actual);

                if (string.IsNullOrEmpty(actual) || actual != expected)
                {
                    result.mismatches.Add(new AnswerMismatch
                    {
                        nodeId = nodeId,
                        expectedKey = expected,
                        actualKey = actual ?? "(not answered)"
                    });
                }
            }

            result.isPerfect = !result.mismatches.Any();
            return result;
        }
    }
}
