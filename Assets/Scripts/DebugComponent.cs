using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Debug component for toggling debug mode.
/// </summary>
public class DebugComponent : MonoBehaviour {
	public bool IsDebugModeEnabled;

    public void Assert(bool check) {
        if (IsDebugModeEnabled) {
            Debug.Assert(check);
        }
    }
    
    public void LogMessage(string message) {
        if (IsDebugModeEnabled) {
            Debug.Log(message);
        }
    }

    public void LogWarning(string message) {
        if (IsDebugModeEnabled) {
            Debug.LogWarning(message);
        }
    }

    public void LogError(string message) {
        if (IsDebugModeEnabled) {
            Debug.LogError(message);
        }
    }
}
