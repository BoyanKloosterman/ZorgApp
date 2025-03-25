using UnityEngine;
using UnityEngine.UI;

public class ZorgMomentButton : MonoBehaviour
{
    public int index; 
    public string trajectNumber;

    private Button button;
    private int zorgMomentID;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
        TrajectManager.Instance.OnZorgMomentenUpdated += UpdateButtonId;

        if (TrajectManager.Instance.zorgMomentIds.Count > 0)
        {
            UpdateButtonId();
        }
    }

    private void UpdateButtonId()
    {
        if (index < TrajectManager.Instance.zorgMomentIds.Count)
        {
            zorgMomentID = TrajectManager.Instance.zorgMomentIds[index];
            button.interactable = true;
        }
        else
        {
            Debug.LogError($"Ongeldige index: {index}");
            button.interactable = false;
        }
    }

    private void OnButtonClick()
    {
        TrajectManager.Instance.LoadZorgMomentScene(zorgMomentID, trajectNumber);
    }

    private void OnDestroy()
    {
        if (TrajectManager.Instance != null)
        {
            TrajectManager.Instance.OnZorgMomentenUpdated -= UpdateButtonId;
        }
    }
}