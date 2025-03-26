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
    public DatePicker datumVerloopPicker;
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
        Debug.Log("NotificatieEditController Start method called");

        // Setup UI listeners
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNotificatie);
        else
            Debug.LogError("Save button is not assigned!");

        if (backButton != null)
            backButton.onClick.AddListener(ReturnToNotificatieScene);
        else
            Debug.LogError("Back button is not assigned!");

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteNotificatie);
        else
            Debug.LogError("Delete button is not assigned!");

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);
        else
            Debug.LogError("Popup close button is not assigned!");

        // Reset status message
        if (statusMessage != null)
            statusMessage.text = "";
        else
            Debug.LogError("Status message text is not assigned!");

        // Hide error popup
        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);
        else
            Debug.LogError("Error popup is not assigned!");

        LoadBasicData();
    }

    private void LoadBasicData()
    {
        Debug.Log("Loading basic notification data");

        try
        {
            // Get notification data
            currentNotificatieId = PlayerPrefs.GetInt("CurrentNotificatieId");
            string bericht = PlayerPrefs.GetString("CurrentNotificatieBericht");
            string datumAanmaak = PlayerPrefs.GetString("CurrentNotificatieDatumAanmaak");
            string datumVerloopStr = PlayerPrefs.GetString("CurrentNotificatieDatumVerloop");
            string userId = PlayerPrefs.GetString("CurrentNotificatieUserId", "");

            Debug.Log($"Loaded Notificatie ID: {currentNotificatieId}, Bericht: {bericht}, DatumAanmaak: {datumAanmaak}, DatumVerloop: {datumVerloopStr}, UserId: {userId}");

            // Set the bericht text
            if (berichtInput != null)
            {
                berichtInput.text = bericht;
                Debug.Log($"Loaded bericht: {berichtInput.text}");
            }
            else
            {
                Debug.LogError("berichtInput reference is null!");
            }

            // Parse the datum verloop and set the date picker and dropdowns
            DateTime datumVerloop;
            if (TryParseDateTime(datumVerloopStr, out datumVerloop))
            {
                originalDateTime = datumVerloop;
                Debug.Log($"Successfully parsed datumVerloop: {datumVerloop}");

                // Set the date picker
                if (datumVerloopPicker != null)
                {
                    datumVerloopPicker.DateSelectionMode = DateSelectionMode.SingleDate;
                    datumVerloopPicker.SelectedDate = new SerializableDate(datumVerloop.Date);
                    Debug.Log($"Set datepicker to: {datumVerloopPicker.SelectedDate}");
                }
                else
                {
                    Debug.LogError("datumVerloopPicker reference is null!");
                }

                // Set the hour dropdown
                if (dropdownHour != null)
                {
                    // Ensure the hour value is within the valid range for the dropdown
                    int hour = Mathf.Clamp(datumVerloop.Hour, 0, dropdownHour.options.Count - 1);
                    dropdownHour.value = hour;
                    Debug.Log($"Set hour dropdown to: {hour}");
                }
                else
                {
                    Debug.LogError("dropdownHour reference is null!");
                }

                // Set the minute dropdown
                if (dropdownMinute != null)
                {
                    // Ensure the minute value is within the valid range for the dropdown
                    int minute = Mathf.Clamp(datumVerloop.Minute, 0, dropdownMinute.options.Count - 1);
                    dropdownMinute.value = minute;
                    Debug.Log($"Set minute dropdown to: {minute}");
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

            // Set isInitialized to true after loading data
            isInitialized = true;
            Debug.Log("NotificatieEditController is initialized");
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

        // Define all the formats we want to try
        string[] formats = {
        "yyyy-MM-dd HH:mm:ss.fff",    // Matches: 2025-03-26 22:58:00.000
        "yyyy-MM-ddTHH:mm:ss.fff",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd H:mm:ss",
        "MM/dd/yyyy HH:mm:ss"
    };

        try
        {
            // First, try exactly with the known database format
            if (dateTimeStr.Length == 23 && dateTimeStr.Contains("-") && dateTimeStr.Contains(":") && dateTimeStr.Contains("."))
            {
                // This is likely the database format: 2025-03-26 22:58:00.000
                if (DateTime.TryParseExact(
                    dateTimeStr,
                    "yyyy-MM-dd HH:mm:ss.fff",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out result))
                {
                    Debug.Log($"Successfully parsed date using exact database format: {result}");
                    return true;
                }
            }

            // Try parsing with specific formats
            if (DateTime.TryParseExact(dateTimeStr, formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out result))
            {
                Debug.Log($"Successfully parsed date using format array: {result}");
                return true;
            }

            // Try general parsing with InvariantCulture
            if (DateTime.TryParse(dateTimeStr,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out result))
            {
                Debug.Log($"Successfully parsed date using InvariantCulture: {result}");
                return true;
            }

            // Last resort - try with current culture
            if (DateTime.TryParse(dateTimeStr, out result))
            {
                Debug.Log($"Successfully parsed date using current culture: {result}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            // If any exception occurs during parsing, log it and return false
            Debug.LogError($"Error during date parsing: {ex.Message}");
            return false;
        }
    }


    private void SetDefaultDateTimeValues()
    {
        // Set default date/time (today at current time)
        originalDateTime = DateTime.Now;

        // Set date picker to today
        if (datumVerloopPicker != null)
        {
            datumVerloopPicker.DateSelectionMode = DateSelectionMode.SingleDate;
            datumVerloopPicker.SelectedDate = new SerializableDate(DateTime.Today);
            Debug.Log($"Set default datumVerloop: {datumVerloopPicker.SelectedDate}");
        }

        // Set hour dropdown to current hour
        if (dropdownHour != null)
        {
            int hour = Mathf.Clamp(DateTime.Now.Hour, 0, dropdownHour.options.Count - 1);
            dropdownHour.value = hour;
            Debug.Log($"Set default hour: {dropdownHour.value}");
        }

        // Set minute dropdown to current minute
        if (dropdownMinute != null)
        {
            int minute = Mathf.Clamp(DateTime.Now.Minute, 0, dropdownMinute.options.Count - 1);
            dropdownMinute.value = minute;
            Debug.Log($"Set default minute: {dropdownMinute.value}");
        }
    }



    public async void SaveNotificatie()
    {
        Debug.Log("SaveNotificatie method called");

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

        // Get selected date from date picker and time from dropdowns
        DateTime datumVerloop;
        try
        {
            // Get date from datepicker
            DateTime selectedDate;
            if (datumVerloopPicker != null && datumVerloopPicker.SelectedDate.HasValue)
            {
                selectedDate = datumVerloopPicker.SelectedDate.Date;
                Debug.Log($"Using date from picker: {selectedDate}");
            }
            else if (originalDateTime != default)
            {
                selectedDate = originalDateTime.Date;
                Debug.Log($"Using original date: {selectedDate}");
            }
            else
            {
                selectedDate = DateTime.Today;
                Debug.Log($"Using today's date: {selectedDate}");
            }

            // Get time from dropdowns
            int hour = (dropdownHour != null) ? dropdownHour.value : 0;
            int minute = (dropdownMinute != null) ? dropdownMinute.value : 0;

            // Combine date and time
            datumVerloop = new DateTime(
                selectedDate.Year,
                selectedDate.Month,
                selectedDate.Day,
                hour,
                minute,
                0
            );
            Debug.Log($"Created datumVerloop: {datumVerloop}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating date: {ex.Message}");
            ShowErrorPopup("Fout met datum en tijd");
            return;
        }

        try
        {
            // Get the original DatumAanmaak or use current time if not available
            string datumAanmaak = PlayerPrefs.GetString("CurrentNotificatieDatumAanmaak");
            if (string.IsNullOrEmpty(datumAanmaak))
            {
                datumAanmaak = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            // Make sure both dates use the same format (using T instead of space)
            string formattedDatumVerloop = datumVerloop.ToString("yyyy-MM-ddTHH:mm:ss");

            // Get userId from PlayerPrefs or leave it null if not available
            string userId = PlayerPrefs.GetString("CurrentNotificatieUserId", null);

            // Create a dictionary for the JSON data to have more control over the structure
            var notificatieData = new Dictionary<string, object>
        {
            { "id", currentNotificatieId },
            { "bericht", berichtInput.text },
            { "isGelezen", false },
            { "datumAanmaak", datumAanmaak },
            { "datumVerloop", formattedDatumVerloop }
        };

            // Only add userId if it's not null or empty
            if (!string.IsNullOrEmpty(userId))
            {
                notificatieData.Add("userId", userId);
            }

            // Convert to JSON
            string notificatieJson = JsonUtility.ToJson(new Notificatie
            {
                id = currentNotificatieId,
                Bericht = berichtInput.text,
                IsGelezen = false,
                DatumAanmaak = datumAanmaak,
                DatumVerloop = formattedDatumVerloop,
                // Don't set UserId at all - let the server handle it
            });

            Debug.Log("Verzonden JSON: " + notificatieJson);
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestResponse response = await webClient.SendPutRequest($"api/Notificatie/{currentNotificatieId}", notificatieJson);

                if (response is WebRequestError errorResponse)
                {
                    Debug.LogError($"Error response: {errorResponse.ErrorMessage}");
                    ShowErrorPopup("Fout bij opslaan van notificatie: " + errorResponse.ErrorMessage);
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notificatie succesvol bijgewerkt!", false);
                    StartCoroutine(ReturnToMainAfterDelay(2f));
                }
                else
                {
                    Debug.LogError("Unknown response type");
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
        Debug.Log("DeleteNotificatie method called");

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
                    ShowErrorPopup("Fout bij verwijderen van notificatie: " + (errorResponse as WebRequestError).ErrorMessage);
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
        Debug.Log($"ShowStatus called with message: {message}, isError: {isError}");

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
        Debug.Log("Showing error popup: " + message);

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
        Debug.Log("Popup close button clicked");
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
        Debug.Log($"ReturnToMainAfterDelay called with delay: {delay}");
        yield return new WaitForSeconds(delay);
        ReturnToNotificatieScene();
    }

    private void ReturnToNotificatieScene()
    {
        Debug.Log("Returning to NotificatieScene");
        SceneManager.LoadScene("NotificatieScene");
    }
}
