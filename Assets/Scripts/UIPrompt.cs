using UnityEngine;
using UnityEngine.UI;

public class UIPrompt : MonoBehaviour
{
    [Header("UI References")]
    public Text promptText;
    public GameObject promptPanel;

    private static UIPrompt instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void ShowPrompt(string message)
    {
        if (instance != null && instance.promptText != null)
        {
            instance.promptText.text = message;
            if (instance.promptPanel != null)
            {
                instance.promptPanel.SetActive(true);
            }
        }
        Debug.Log(message);
    }

    public static void HidePrompt()
    {
        if (instance != null && instance.promptPanel != null)
        {
            instance.promptPanel.SetActive(false);
        }
    }
} 