using UnityEngine;
using UnityEngine.UI;

public class ZorgMomentButton : MonoBehaviour
{
    public int index;
    public string trajectNumber;

    private Button button;
    private Image buttonImage;
    private int zorgMomentID;

    private void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnButtonClick);
        TrajectManager.Instance.OnZorgMomentenUpdated += UpdateButtonState;
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (index >= TrajectManager.Instance.zorgMomentIds.Count)
        {
            button.interactable = false;
            return;
        }

        zorgMomentID = TrajectManager.Instance.zorgMomentIds[index];

        bool isCompleted = TrajectManager.Instance.behaaldeZorgMomentIds.Contains(zorgMomentID);
        buttonImage.color = isCompleted ? Color.green : Color.white;

        button.interactable = true;
    }

    private void OnButtonClick()
    {
        TrajectManager.Instance.LoadZorgMomentScene(zorgMomentID, trajectNumber);
    }

    private void OnDestroy()
    {
        if (TrajectManager.Instance != null)
        {
            TrajectManager.Instance.OnZorgMomentenUpdated -= UpdateButtonState;
        }
    }
}