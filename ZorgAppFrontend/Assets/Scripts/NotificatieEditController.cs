using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI.Dates;

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
            // Get notification ID and text only
            currentNotificatieId = PlayerPrefs.GetInt("CurrentNotificatieId");
            string bericht = PlayerPrefs.GetString("CurrentNotificatieBericht");
            string datumAanmaak = PlayerPrefs.GetString("CurrentNotificatieDatumAanmaak");

            Debug.Log($"Loaded Notificatie ID: {currentNotificatieId}, Bericht: {bericht}, DatumAanmaak: {datumAanmaak}");

            if (berichtInput != null)
            {
                berichtInput.text = bericht;
                Debug.Log($"Loaded bericht: {berichtInput.text}");
            }
            else
            {
                Debug.LogError("berichtInput reference is null!");
            }

            // Don't attempt to load or set the date

            // Set isInitialized to true after loading data
            isInitialized = true;
            Debug.Log("NotificatieEditController is initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in LoadBasicData: {ex.Message}");
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

        // Use the original date info for now
        DateTime datumVerloop;
        try
        {
            // Try to use the dropdowns for time if available
            int hour = (dropdownHour != null) ? dropdownHour.value : originalDateTime.Hour;
            int minute = (dropdownMinute != null) ? dropdownMinute.value : originalDateTime.Minute;

            // Use the original date
            datumVerloop = new DateTime(
                originalDateTime.Year,
                originalDateTime.Month,
                originalDateTime.Day,
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
            Notificatie updatedNotificatie = new Notificatie
            {
                id = currentNotificatieId,
                Bericht = berichtInput.text,
                IsGelezen = false,
                DatumAanmaak = PlayerPrefs.GetString("CurrentNotificatieDatumAanmaak"),
                DatumVerloop = datumVerloop.ToString("yyyy-MM-ddTHH:mm:ss")
            };

            string notificatieJson = JsonUtility.ToJson(updatedNotificatie);
            Debug.Log("Verzonden JSON: " + notificatieJson);
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestResponse response = await webClient.SendPutRequest($"api/Notificatie/{currentNotificatieId}", notificatieJson);

                if (response is WebRequestError errorResponse)
                {
                    ShowErrorPopup("Fout bij opslaan van notificatie: " + (errorResponse as WebRequestError).ErrorMessage);
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
