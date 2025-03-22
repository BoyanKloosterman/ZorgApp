using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class NotitieEditController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField titleInput;
    public TMP_InputField textInput;
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

    private int currentNoteId;

    void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNote);

        if (backButton != null)
            backButton.onClick.AddListener(ReturnToNoteScene);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteNote);

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);

        LoadNoteData();
    }

    private void LoadNoteData()
    {
        currentNoteId = PlayerPrefs.GetInt("CurrentNoteId");
        titleInput.text = PlayerPrefs.GetString("CurrentNoteTitle");
        textInput.text = PlayerPrefs.GetString("CurrentNoteText");
    }

    public async void SaveNote()
    {
        if (string.IsNullOrEmpty(titleInput.text))
        {
            ShowErrorPopup("Titel is verplicht");
            return;
        }

        Notitie updatedNote = new Notitie
        {
            id = currentNoteId,
            Titel = titleInput.text,
            Tekst = textInput.text,
            DatumAanmaak = PlayerPrefs.GetString("CurrentNoteDate")
        };

        try
        {
            string noteJson = JsonUtility.ToJson(updatedNote);
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestReponse response = await webClient.SendPutRequest($"api/Notitie/{currentNoteId}", noteJson);

                if (response is WebRequestError)
                {
                    ShowErrorPopup("Fout bij opslaan van notitie");
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notitie succesvol bijgewerkt!", false);
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
            ShowErrorPopup("Er is een fout opgetreden");
        }
    }

    public async void DeleteNote()
    {
        try
        {
            string token = SecureUserSession.Instance.GetToken();

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestReponse response = await webClient.SendDeleteRequest($"api/Notitie/{currentNoteId}");

                if (response is WebRequestError)
                {
                    ShowErrorPopup("Fout bij verwijderen van notitie");
                }
                else if (response is WebRequestData<string>)
                {
                    ShowStatus("Notitie succesvol verwijderd!", false);
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
        ReturnToNoteScene();
    }

    private void ReturnToNoteScene()
    {
        SceneManager.LoadScene("NoteScene");
    }
}
