using UnityEngine;
using TMPro;
using System.Collections;

public class NotificatieController : MonoBehaviour
{
    public TextMeshProUGUI notificationText;
    public GameObject notificationPanel;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }
        notificationPanel.SetActive(false);
        canvasGroup.alpha = 0;
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        StopAllCoroutines();
        notificationText.text = message;
        notificationPanel.SetActive(true);
        StartCoroutine(ShowAndHideNotification(duration));
    }

    private IEnumerator ShowAndHideNotification(float duration)
    {
        // Fade-in
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * 2;
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        // Fade-out
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 2;
            yield return null;
        }

        notificationPanel.SetActive(false);
    }
}
