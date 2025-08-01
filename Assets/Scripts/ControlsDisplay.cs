using UnityEngine;
using UnityEngine.UI;

public class ControlsDisplay : MonoBehaviour
{
    [Header("Controls UI")]
    public GameObject controlsPanel;
    public Text controlsText;

    void Start()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }

        if (controlsText != null)
        {
            controlsText.text = "Controls:\n" +
                              "WASD - Move\n" +
                              "Mouse - Look around\n" +
                              "Space - Jump\n" +
                              "L - Return to spawn / Start clone replay\n" +
                              "P - Play back sequence (alternative)\n" +
                              "R - Reset level completely\n\n" +
                              "Leave the start zone to begin recording!";
        }

        // Hide controls after 5 seconds
        Invoke(nameof(HideControls), 5f);
    }

    void HideControls()
    {
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }
} 