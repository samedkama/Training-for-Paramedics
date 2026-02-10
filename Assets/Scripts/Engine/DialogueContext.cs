using System.Collections.Generic;

public class DialogueContext
{
    // ID of the node currently active in the dialogue flow.
    public string CurrentNodeId;

    // Optional forced next node if an action overrides normal navigation.
    public string ForcedNextNodeId;

    // Lightweight state map for effect flags (for example hrStatus, rrStatus, spo2Status, bpStatus).
    public Dictionary<string, string> State = new Dictionary<string, string>();
}
