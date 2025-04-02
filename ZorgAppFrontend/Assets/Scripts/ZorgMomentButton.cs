using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ZorgMomentButton : MonoBehaviour
{
    public int index;
    public string trajectNumber;

    private Button button;
    private Image buttonImage;
    private int zorgMomentID;

    public GameObject eventSystem;

    private void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(OnButtonClick);
        eventSystem = GameObject.Find("EventSystem");
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

        Color semiTransparentGreen = new Color(0f, 1f, 0f, 0.5f);

        Color opaqueWhite = new Color(0f, 0f, 0f, 1f);

        buttonImage.color = isCompleted ? semiTransparentGreen : opaqueWhite;

        button.interactable = true;
    }

    private async void OnButtonClick()
    {
        float duration = 1f;
        int currentIndex = TrajectManager.Instance.GetCurrentAvatarIndex();

        if (index == currentIndex + 1)
        {
            await eventSystem.GetComponent<TrajectAvatarManager>().MoveAvatarTo(index, duration);
        }

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