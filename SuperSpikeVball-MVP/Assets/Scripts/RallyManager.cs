using System;
using UnityEngine;

/// <summary>
/// Manages rally progression and toggles the appropriate serve‐zone blocker.
/// </summary>
public class RallyManager : MonoBehaviour
{
    public static RallyManager Instance { get; private set; }

    [Header("Server Selection")]
    [Tooltip("1 = left side serves first; 2 = right side")]
    public int servingTeam = 1;

    [Header("Serve Blockers")]
    [Tooltip("GameObject with a BoxCollider covering Team 1's serve zone")]
    public GameObject serveBlockerTeam1;
    [Tooltip("GameObject with a BoxCollider covering Team 2's serve zone")]
    public GameObject serveBlockerTeam2;

    [Header("Initial State")]
    public RallyState initialState = RallyState.PreServe;

    public RallyState State { get; private set; }

    public event Action<RallyState> OnStateChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        State = initialState;
        UpdateBlockers();
        Debug.Log($"RallyManager initialized: ServingTeam={servingTeam}, State={State}");
    }

    /// <summary>
    /// Change to a new rally state.
    /// </summary>
    public void SetState(RallyState newState)
    {
        if (newState == State) return;
        State = newState;
        Debug.Log($"RallyManager → State changed to {State}");
        UpdateBlockers();
        OnStateChanged?.Invoke(State);
    }

    /// <summary>
    /// Enable only the current server’s serve‐zone blocker during PreServe.
    /// </summary>
    void UpdateBlockers()
    {
        bool blockPreServe = (State == RallyState.PreServe);
        serveBlockerTeam1.SetActive(blockPreServe && servingTeam == 1);
        serveBlockerTeam2.SetActive(blockPreServe && servingTeam == 2);
    }

    public void BeginTossCharge()
    {
        if (State != RallyState.PreServe) return;
        SetState(RallyState.TossCharging);
    }

    public void ReleaseToss()
    {
        if (State != RallyState.TossCharging) return;
        SetState(RallyState.Tossed);
    }

    public void SpikeInFlight()
    {
        if (State != RallyState.Tossed) return;
        SetState(RallyState.InRally);
    }

    public void BallFirstContact()
    {
        if (State == RallyState.Tossed || State == RallyState.InRally)
            SetState(RallyState.PointOver);
    }

    /// <summary>
    /// Reset the rally for the next server.
    /// </summary>
    public void ResetRally(int nextServer)
    {
        servingTeam = nextServer;
        SetState(RallyState.PreServe);
    }
}
