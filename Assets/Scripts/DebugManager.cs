using UnityEngine;
using UnityEngine.UI;
using Duet.Managers;

public class DebugManager : MonoBehaviour
{
    public Text debugText;

    void Update()
    {
        if (debugText != null && GameManager.Instance != null)
        {
            debugText.text = $"Game State: {GameManager.Instance.CurrentState}\n" +
                           $"Input Direction: {InputManager.Instance.GetRotationDirection()}\n" +
                           $"Time: {Time.time:F1}s";
        }
    }
}
