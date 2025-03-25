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
    public DatePicker datumVerloopPicker;
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
        SceneManager.LoadScene("MainScene");
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

        Notificatie newNotificatie = new Notificatie
        {
            Bericht = berichtInput.text,
            IsGelezen = false,
            DatumAanmaak = DateTime.Now,
            DatumVerloop = datumVerloopPicker.SelectedDate.Date,
        };

        try
        {
            string notificatieJson = JsonUtility.ToJson(newNotificatie);
            Debug.Log("Verzonden JSON: " + notificatieJson); // Add this line to log the JSON being sent
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestResponse response = await webClient.SendPostRequest("api/Notificatie", notificatieJson);

                if (response is WebRequestError)
                {
                    ShowErrorPopup("Fout bij opslaan van notificatie");
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
            ShowErrorPopup("Er is een fout opgetreden");
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
