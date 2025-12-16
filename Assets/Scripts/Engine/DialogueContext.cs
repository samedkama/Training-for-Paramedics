using System.Collections.Generic;

public class DialogueContext
{
    // Welcher Node ist gerade aktiv?
    public string CurrentNodeId;

    // Override-Ziel, falls eine Action den Flow umleitet
    public string ForcedNextNodeId;

    // Einfache State-Map für Effekte (z. B. hrStatus, rrStatus, spo2Status, bpStatus)
    public Dictionary<string, string> State = new Dictionary<string, string>();
}
