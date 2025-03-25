using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NotificatieShowController : MonoBehaviour
{
    [Header("UI Elements")]
    public Text statusMessage;

    [Header("Notification Display")]
    public GameObject notificationButtonPrefab;
    public Transform notificationPanel;
    public Text noNotificationsText;

    [Header("API Connection")]
    public WebClient webClient;

    [Header("Error Popup")]
    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    private bool hasMissingComponents = false;

    void Start()
    {
        CheckRequiredComponents();

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);

        if (!hasMissingComponents)
            LoadNotificaties();
    }

    private void CheckRequiredComponents()
    {
        if (notificationButtonPrefab == null)
        {
            hasMissingComponents = true;
        }

        if (notificationPanel == null)
        {
            hasMissingComponents = true;
        }

        if (noNotificationsText == null)
        {
            hasMissingComponents = true;
        }

        if (webClient == null)
        {
            hasMissingComponents = true;
        }

        if (hasMissingComponents && noNotificationsText != null)
        {
            noNotificationsText.gameObject.SetActive(true);
            noNotificationsText.text = "Configuratiefout: Controleer de console en Inspector";
        }
    }

    private async void LoadNotificaties()
    {
        try
        {
            if (hasMissingComponents)
            {
                return;
            }

            ClearNotificationsFromUI();

            if (noNotificationsText != null)
            {
                noNotificationsText.gameObject.SetActive(true);
                noNotificationsText.text = "Notificaties worden geladen...";
            }

            string token = SecureUserSession.Instance.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                ShowErrorPopup("Geen token beschikbaar");
                UpdateNoNotificationsText("Geen toegang: Token ontbreekt");
                return;
            }

            webClient.SetToken(token);
            IWebRequestResponse response = await webClient.SendGetRequest("api/Notificatie");

            if (response is WebRequestData<string> stringData)
            {
                string json = stringData.Data;

                try
                {
                    if (json.TrimStart().StartsWith("["))
                    {
                        List<Notificatie> notifications = null;

                        try
                        {
                            notifications = JsonHelper.ParseJsonArray<Notificatie>(json);
                        }
                        catch (Exception)
                        {
                        }

                        if (notifications != null && notifications.Count > 0)
                        {
                            ProcessNotificaties(notifications);
                        }
                    }
                    else
                    {
                        UpdateNoNotificationsText("Onverwacht JSON formaat");
                    }
                }
                catch (Exception)
                {
                    UpdateNoNotificationsText("Fout bij verwerken van notificaties");
                }
            }
            else if (response is WebRequestData<List<Notificatie>> listData)
            {
                ProcessNotificaties(listData.Data);
            }
            else if (response is WebRequestData<Notificatie[]> arrayData)
            {
                ProcessNotificaties(new List<Notificatie>(arrayData.Data));
            }
            else if (response is WebRequestError errorResponse)
            {
                string errorMessage = errorResponse?.ErrorMessage ?? "Unknown error";
                UpdateNoNotificationsText("Fout bij laden van notificaties");
                ShowErrorPopup("Fout bij laden van notificaties");
            }
            else
            {
                UpdateNoNotificationsText("Onbekende fout bij laden");
                ShowErrorPopup("Onbekende respons bij laden van notificaties");
            }
        }
        catch (Exception)
        {
            UpdateNoNotificationsText("Fout bij laden van notificaties");
            ShowErrorPopup("Er is een fout opgetreden");
        }
    }

    private void UpdateNoNotificationsText(string message)
    {
        if (noNotificationsText != null)
        {
            noNotificationsText.gameObject.SetActive(true);
            noNotificationsText.text = message;
        }
    }

    private void ProcessNotificaties(List<Notificatie> notifications)
    {
        if (notifications == null || notifications.Count == 0)
        {
            UpdateNoNotificationsText("Geen notificaties gevonden");
            return;
        }

        List<Notificatie> tempNotifications = new List<Notificatie>();
        foreach (var notif in notifications)
        {
            if (notif != null)
            {
                if (notif.ID == 0 && string.IsNullOrEmpty(notif.Bericht))
                {
                    continue;
                }
                tempNotifications.Add(notif);
            }
        }
        notifications = tempNotifications;

        notifications = notifications.FindAll(notif =>
            notif != null &&
            notif.ID > 0 &&
            !string.IsNullOrEmpty(notif.Bericht)
        );

        if (notifications.Count == 0)
        {
            UpdateNoNotificationsText("Geen geldige notificaties gevonden");
            return;
        }

        List<Notificatie> validNotifications = new List<Notificatie>();
        foreach (var notif in notifications)
        {
            if (string.IsNullOrEmpty(notif.DatumAanmaak))
            {
                notif.DatumAanmaak = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            bool isValidDate = DateTime.TryParse(notif.DatumAanmaak, out DateTime parsedDate);
            if (!isValidDate)
            {
                string[] formats = new[] {
                    "yyyy-MM-ddTHH:mm:ss.fff",
                    "yyyy-MM-ddTHH:mm:ss",
                    "yyyy-MM-dd HH:mm:ss.fff",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-dd"
                };

                isValidDate = DateTime.TryParseExact(
                    notif.DatumAanmaak,
                    formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out parsedDate);

                if (isValidDate)
                {
                    notif.DatumAanmaak = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    notif.DatumAanmaak = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            validNotifications.Add(notif);
        }

        if (validNotifications.Count == 0)
        {
            UpdateNoNotificationsText("Geen geldige notificaties gevonden");
            return;
        }

        validNotifications.Sort((notif1, notif2) =>
        {
            try
            {
                DateTime date1 = DateTime.Parse(notif1.DatumAanmaak);
                DateTime date2 = DateTime.Parse(notif2.DatumAanmaak);
                return date2.CompareTo(date1);
            }
            catch (Exception)
            {
                return 0;
            }
        });

        if (noNotificationsText != null)
        {
            noNotificationsText.gameObject.SetActive(false);
        }

        bool addedAtLeastOne = false;

        foreach (var notification in validNotifications)
        {
            if (notification != null && !string.IsNullOrEmpty(notification.Bericht))
            {
                AddNotificationToUI(notification);
                addedAtLeastOne = true;
            }
        }

        if (addedAtLeastOne)
        {
            ShowStatus($"{validNotifications.Count} notificaties geladen", false);
        }
        else
        {
            UpdateNoNotificationsText("Geen geldige notificaties gevonden");
        }
    }

    private void ClearNotificationsFromUI()
    {
        if (notificationPanel != null)
        {
            foreach (Transform child in notificationPanel)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void AddNotificationToUI(Notificatie notification)
    {
        if (notificationButtonPrefab == null || notificationPanel == null)
        {
            return;
        }

        GameObject notificationButtonObj = Instantiate(notificationButtonPrefab, notificationPanel);

        SetTextComponentByName(notificationButtonObj, "BerichtText", notification.Bericht);

        string formattedCreationDate = !string.IsNullOrEmpty(notification.DatumAanmaak)
            ? FormatDateForDisplay(notification.DatumAanmaak)
            : "";
        SetTextComponentByName(notificationButtonObj, "DatumAanmaak", formattedCreationDate);

        string formattedExpiryDate = (!string.IsNullOrEmpty(notification.DatumVerloop) &&
                                      notification.DatumVerloop != "0001-01-01T00:00:00")
            ? FormatDateForDisplay(notification.DatumVerloop)
            : "";
        SetTextComponentByName(notificationButtonObj, "DatumVerloop", formattedExpiryDate);

        Button button = notificationButtonObj.GetComponent<Button>();
        if (button != null)
        {
            Notificatie capturedNotification = notification;
            button.onClick.AddListener(() => OpenNotificationDetailScene(capturedNotification));
        }
    }

    private void SetTextComponentByName(GameObject parent, string componentName, string text)
    {
        Transform textTransform = parent.transform.Find(componentName);

        if (textTransform == null)
        {
            foreach (Transform child in parent.GetComponentsInChildren<Transform>())
            {
                if (child.name == componentName)
                {
                    textTransform = child;
                    break;
                }
            }
        }

        if (textTransform != null)
        {
            TextMeshProUGUI tmpText = textTransform.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = text;
                return;
            }

            Text uiText = textTransform.GetComponent<Text>();
            if (uiText != null)
            {
                uiText.text = text;
                return;
            }
        }
    }

    private string FormatDateForDisplay(string dateString)
    {
        try
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date.ToString("dd-MM-yyyy HH:mm");
            }
            return dateString;
        }
        catch
        {
            return dateString;
        }
    }

    private void OpenNotificationDetailScene(Notificatie notification)
    {
        PlayerPrefs.SetInt("CurrentNotificationId", notification.ID);
        PlayerPrefs.SetString("CurrentNotificationMessage", notification.Bericht);
        PlayerPrefs.SetString("CurrentNotificationDate", notification.DatumAanmaak ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
        SceneManager.LoadScene("NotificationDetailScene");
    }

    private void ShowStatus(string message, bool isError)
    {
        if (statusMessage != null)
        {
            statusMessage.text = message;
            statusMessage.color = isError ? Color.red : Color.green;
        }
    }

    private void ShowErrorPopup(string message)
    {
        if (ErrorPopup != null)
        {
            if (popupMessageText != null)
            {
                popupMessageText.text = message;
            }

            ErrorPopup.SetActive(true);
        }
        else
        {
            ShowStatus(message, true);
        }
    }

    private void OnPopupCloseButtonClick()
    {
        if (ErrorPopup != null)
        {
            ErrorPopup.SetActive(false);
        }
    }
}
