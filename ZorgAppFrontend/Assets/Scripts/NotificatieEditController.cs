using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI.Dates;
using System.Collections.Generic;

public class NotificatieEditController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField berichtInput;
    // public DatePicker datumVerloopPicker;
    public UI.Dates.DatePicker datumVerloopPicker;
    public TMP_Dropdown dropdownHour;
    public TMP_Dropdown dropdownMinute;
    public Button saveButton;
    public Button backButton;
    public Button deleteButton;
    public Text statusMessage;

    [Header("API Connection")]
    public WebClient webClient;

    [Header("Error Popup")]
    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    private int currentNotificatieId;
    private DateTime originalDateTime;
    private bool isInitialized = false;

    void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNotificatie);

        if (backButton != null)
            backButton.onClick.AddListener(ReturnToNotificatieScene);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteNotificatie);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);

        LoadBasicData();
    }

    private void LoadBasicData()
    {
        try
        {
            currentNotificatieId = PlayerPrefs.GetInt("CurrentNotificatieId");
            string bericht = PlayerPrefs.GetString("CurrentNotificatieBericht");
            string datumAanmaak = PlayerPrefs.GetString("CurrentNotificatieDatumAanmaak");
            string datumVerloopStr = PlayerPrefs.GetString("CurrentNotificatieDatumVerloop");
            string userId = PlayerPrefs.GetString("CurrentNotificatieUserId", "");

            if (berichtInput != null)
            {
                berichtInput.text = bericht;
            }
            else
            {
                Debug.LogError("berichtInput reference is null!");
            }

            DateTime datumVerloop;
            if (TryParseDateTime(datumVerloopStr, out datumVerloop))
            {
                originalDateTime = datumVerloop;

                if (datumVerloopPicker != null)
                {
                    datumVerloopPicker.DateSelectionMode = DateSelectionMode.SingleDate;
                    datumVerloopPicker.SelectedDate = new SerializableDate(datumVerloop.Date);
                }
                else
                {
                    Debug.LogError("datumVerloopPicker reference is null!");
                }

                if (dropdownHour != null)
                {
                    int hour = Mathf.Clamp(datumVerloop.Hour, 0, dropdownHour.options.Count - 1);
                    dropdownHour.value = hour;
                }
                else
                {
                    Debug.LogError("dropdownHour reference is null!");
                }

                if (dropdownMinute != null)
                {
                    int minute = Mathf.Clamp(datumVerloop.Minute, 0, dropdownMinute.options.Count - 1);
                    dropdownMinute.value = minute;
                }
                else
                {
                    Debug.LogError("dropdownMinute reference is null!");
                }
            }
            else
            {
                Debug.LogError($"Failed to parse datumVerloop! Value: '{datumVerloopStr}'");
                SetDefaultDateTimeValues();
            }

            isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in LoadBasicData: {ex.Message}");
            SetDefaultDateTimeValues();
        }
    }

    private bool TryParseDateTime(string dateTimeStr, out DateTime result)
    {
        result = DateTime.Now;

        if (string.IsNullOrEmpty(dateTimeStr))
            return false;

        string[] formats = {
            "yyyy-MM-dd HH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd H:mm:ss",
            "MM/dd/yyyy HH:mm:ss"
        };

        try
        {
            if (dateTimeStr.Length == 23 && dateTimeStr.Contains("-") && dateTimeStr.Contains(":") && dateTimeStr.Contains("."))
            {
                if (DateTime.TryParseExact(
                    dateTimeStr,
                    "yyyy-MM-dd HH:mm:ss.fff",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out result))
                {
                    return true;
                }
            }

            if (DateTime.TryParseExact(dateTimeStr, formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out result))
            {
                return true;
            }

            if (DateTime.TryParse(dateTimeStr,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out result))
            {
                return true;
            }

            if (DateTime.TryParse(dateTimeStr, out result))
            {
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during date parsing: {ex.Message}");
            return false;
        }
    }

    private void SetDefaultDateTimeValues()
    {
        originalDateTime = DateTime.Now;

        if (datumVerloopPicker != null)
        {
            datumVerloopPicker.DateSelectionMode = DateSelectionMode.SingleDate;
            datumVerloopPicker.SelectedDate = new SerializableDate(DateTime.Today);
        }

        if (dropdownHour != null)
        {
            int hour = Mathf.Clamp(DateTime.Now.Hour, 0, dropdownHour.options.Count - 1);
            dropdownHour.value = hour;
        }

        if (dropdownMinute != null)
        {
            int minute = Mathf.Clamp(DateTime.Now.Minute, 0, dropdownMinute.options.Count - 1);
            dropdownMinute.value = minute;
        }
    }

    public async void SaveNotificatie()
    {
        if (!isInitialized)
        {
            ShowErrorPopup("Systeem is nog niet gereed, probeer opnieuw");
            return;
        }

        if (string.IsNullOrEmpty(berichtInput.text))
        {
            ShowErrorPopup("Bericht is verplicht");
            return;
        }

        DateTime datumVerloop;
        try
        {
            DateTime selectedDate;
            if (datumVerloopPicker != null && datumVerloopPicker.SelectedDate.HasValue)
            {
                selectedDate = datumVerloopPicker.SelectedDate.Date;
            }
            else if (originalDateTime != default)
            {
                selectedDate = originalDateTime.Date;
            }
            else
            {
                selectedDate = DateTime.Today;
            }

            int hour = (dropdownHour != null) ? dropdownHour.value : 0;
            int minute = (dropdownMinute != null) ? dropdownMinute.value : 0;

            datumVerloop = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                hour,
                minute,
                0
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating date: {ex.Message}");
            ShowErrorPopup("Fout met datum en tijd");
            return;
        }

        try
        {
            string datumAanmaak = PlayerPrefs.GetString("CurrentNotificatieDatumAanmaak");
            if (string.IsNullOrEmpty(datumAanmaak))
            {
                datumAanmaak = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            string formattedDatumVerloop = datumVerloop.ToString("yyyy-MM-ddTHH:mm:ss");
            string userId = PlayerPrefs.GetString("CurrentNotificatieUserId", null);

            var notificatieData = new Dictionary<string, object>
            {
                { "id", currentNotificatieId },
                { "bericht", berichtInput.text },
                { "isGelezen", false },
                { "datumAanmaak", datumAanmaak },
                { "datumVerloop", formattedDatumVerloop }
            };

            if (!string.IsNullOrEmpty(userId))
            {
                notificatieData.Add("userId", userId);
            }

            string notificatieJson = JsonUtility.ToJson(new Notificatie
            {
                id = currentNotificatieId,
                Bericht = berichtInput.text,
                IsGelezen = false,
                DatumAanmaak = datumAanmaak,
                DatumVerloop = formattedDatumVerloop,
            });

            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestResponse response = await webClient.SendPutRequest($"api/Notificatie/{currentNotificatieId}", notificatieJson);

                if (response is WebRequestError errorResponse)
                {
                    ShowErrorPopup("Fout bij opslaan van notificatie: " + errorResponse.ErrorMessage);
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notificatie succesvol bijgewerkt!", false);
                    StartCoroutine(ReturnToMainAfterDelay(2f));
                }
                else
                {
                    ShowErrorPopup("Onbekende respons");
                }
            }
            else
            {
                ShowErrorPopup("Geen token beschikbaar");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving notification: {ex.Message}");
            ShowErrorPopup("Er is een fout opgetreden bij het opslaan");
        }
    }

    public async void DeleteNotificatie()
    {
        if (!isInitialized)
        {
            ShowErrorPopup("Systeem is nog niet gereed, probeer opnieuw");
            return;
        }

        try
        {
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestResponse response = await webClient.SendDeleteRequest($"api/Notificatie/{currentNotificatieId}");

                if (response is WebRequestError errorResponse)
                {
                    ShowErrorPopup("Fout bij verwijderen van notificatie: " + errorResponse.ErrorMessage);
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notificatie succesvol verwijderd!", false);
                    StartCoroutine(ReturnToMainAfterDelay(2f));
                }
                else
                {
                    ShowErrorPopup("Onbekende respons");
                }
            }
            else
            {
                ShowErrorPopup("Geen token beschikbaar");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting notification: {ex.Message}");
            ShowErrorPopup("Er is een fout opgetreden bij het verwijderen");
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        if (statusMessage != null)
        {
            statusMessage.text = message;
            statusMessage.color = isError ? Color.red : Color.green;
        }
        else
        {
            Debug.LogError("statusMessage reference is null!");
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
            else
            {
                Debug.LogError("popupMessageText reference is null!");
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
        else
        {
            Debug.LogError("ErrorPopup reference is null!");
        }
    }

    private IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToNotificatieScene();
    }

    private void ReturnToNotificatieScene()
    {
        SceneManager.LoadScene("NotificatieScene");
    }
}
