/// <summary>
/// The distinct phases of a volleyball rally.
/// </summary>
public enum RallyState
{
    PreServe,       // Server may move inside their serve zone only
    TossCharging,   // Server is charging their toss (no movement)
    Tossed,         // Ball has left the hand; awaiting first contact
    InRally,        // Ball in play; free movement
    PointOver       // Rally ended; point awarded
}
