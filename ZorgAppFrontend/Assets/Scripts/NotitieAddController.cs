using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NotitieAddController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField titleInput;
    public TMP_InputField textInput;
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
            saveButton.onClick.AddListener(SaveNote);

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);
    }

    public async void SaveNote()
    {
        if (string.IsNullOrEmpty(titleInput.text))
        {
            ShowErrorPopup("Titel is verplicht");
            return;
        }

        Notitie newNote = new Notitie
        {
            Titel = titleInput.text,
            Tekst = textInput.text,
            DatumAanmaak = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        try
        {
            string noteJson = JsonUtility.ToJson(newNote);
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestReponse response = await webClient.SendPostRequest("api/Notitie", noteJson);

                if (response is WebRequestError)
                {
                    ShowErrorPopup("Fout bij opslaan van notitie");
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notitie succesvol opgeslagen!", false);
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
        catch (Exception)
        {
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
        SceneManager.LoadScene("NoteScene");
    }
}
