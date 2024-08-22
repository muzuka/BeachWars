using UnityEngine;

/// <summary>
/// Represents team.
/// </summary>
public class Team : MonoBehaviour {

    [Tooltip("-1 is neutral, 0 is player, 1+ is AI")]
    public int team;

    public bool OnTeam(int queryTeam)
    {
        return team == queryTeam;
    }

    public void ChangeTeam(int newTeam)
    {
        team = newTeam;
    }
}
