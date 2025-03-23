using UnityEngine;
using UnityEngine.UI;

public class ZorgMomentButton : MonoBehaviour
{
    public int zorgMomentID;
    public string trajectNumber;

    private Button button;
    private Color completedColor = new Color(0f, 1f, 0f, 0.5f); 
    private Color availableColor = new Color(1f, 0.92f, 0.16f, 1f); 
    private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); 

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        TrajectManager.Instance.OnZorgMomentenUpdated += UpdateButtonState;
        UpdateButtonState();
    }

    private void OnButtonClick()
    {
        TrajectManager.Instance.LoadZorgMomentScene(zorgMomentID, trajectNumber);
    }

    private void UpdateButtonState()
    {
        int nextAvailableId = TrajectManager.Instance.HighestBehaaldId + 1;
        bool isCompleted = TrajectManager.Instance.BehaaldeZorgMomentIds.Contains(zorgMomentID);
        bool isAvailable = zorgMomentID == nextAvailableId;

        button.interactable = isCompleted || isAvailable;

        ColorBlock colors = button.colors;
        if (isCompleted)
        {
            colors.normalColor = completedColor;
            colors.highlightedColor = completedColor * 0.8f;
            colors.pressedColor = completedColor * 0.6f;
        }
        else if (isAvailable)
        {
            colors.normalColor = availableColor;
            colors.highlightedColor = availableColor * 0.8f;
            colors.pressedColor = availableColor * 0.6f;
        }
        else
        {
            colors.normalColor = disabledColor;
            colors.highlightedColor = disabledColor;
            colors.pressedColor = disabledColor;
        }
        button.colors = colors;
    }
    private void OnDestroy()
    {
        if (TrajectManager.Instance != null)
        {
            TrajectManager.Instance.OnZorgMomentenUpdated -= UpdateButtonState;
        }
    }
}