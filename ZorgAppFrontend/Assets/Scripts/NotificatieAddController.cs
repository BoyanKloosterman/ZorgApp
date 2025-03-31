using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UI.Dates;

public class NotificatieAddController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField berichtInput;
    // public DatePicker datumVerloopPicker;
    public UI.Dates.DatePicker datumVerloopPicker;
    public TMP_Dropdown dropdownHour;
    public TMP_Dropdown dropdownMinute;
    public Button backButton;
    public Button saveButton;
    public Text statusMessage;

    [Header("API Connection")]
    public WebClient webClient;

    [Header("Error Popup")]
    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNotificatie);

        if (backButton != null)
            backButton.onClick.AddListener(ReturnToMainScene);

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);
    }

    private void ReturnToMainScene()
    {
        SceneManager.LoadScene("NotificatieScene");
    }

    public async void SaveNotificatie()
    {
        if (string.IsNullOrEmpty(berichtInput.text))
        {
            ShowErrorPopup("Bericht is verplicht");
            return;
        }

        if (!datumVerloopPicker.SelectedDate.HasValue)
        {
            ShowErrorPopup("Datum verloop is verplicht");
            return;
        }

        if (dropdownHour == null || dropdownMinute == null)
        {
            ShowErrorPopup("Uur en minuut zijn verplicht");
            return;
        }

        int selectedHour = dropdownHour.value;
        int selectedMinute = dropdownMinute.value;

        DateTime datumVerloop = datumVerloopPicker.SelectedDate.Date
            .AddHours(selectedHour)
            .AddMinutes(selectedMinute);

        Notificatie notificatie = new Notificatie
        {
            Bericht = berichtInput.text,
            IsGelezen = false,
            DatumAanmaak = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            DatumVerloop = datumVerloop.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        try
        {
            string notificatieJson = JsonUtility.ToJson(notificatie);
            Debug.Log("Verzonden JSON: " + notificatieJson);
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestResponse response = await webClient.SendPostRequest("api/Notificatie", notificatieJson);

                if (response is WebRequestError errorResponse)
                {
                    ShowErrorPopup("Fout bij opslaan van notificatie: " + (errorResponse as WebRequestError).ErrorMessage);
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notificatie succesvol opgeslagen!", false);
                    StartCoroutine(ReturnToMainAfterDelay(0.5f));
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
            Debug.LogError("Exception: " + ex.Message);
            ShowErrorPopup("Er is een fout opgetreden: " + ex.Message);
        }
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

    private IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("NotificatieScene");
    }
}
