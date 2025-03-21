using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NotitieController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField titleInput;
    public TMP_InputField textInput;
    public Button saveButton;
    public Button backNoteButton;
    public Button backRouteButton;
    public Text statusMessage;
    [Header("API Connection")]
    public WebClient webClient;

    [Header("Error Popup")]
    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNote);

        if (backNoteButton != null)
            backNoteButton.onClick.AddListener(ReturnToNoteScene);

        if (backRouteButton != null)
            backRouteButton.onClick.AddListener(ReturnToRouteScene);

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
        {
            ErrorPopup.SetActive(false);
        }
        if (popupCloseButton != null)
        {
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);
        }

        LoadNotes();
    }

    public void GoToNoteAddScene()
    {
        SceneManager.LoadScene("NoteAddScene");
    }

    public void ReturnToRouteScene()
    {
        SceneManager.LoadScene("Route13");
    }

    public void ReturnToNoteScene()
    {
        SceneManager.LoadScene("NoteScene");
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

                if (response is WebRequestError errorResponse)
                {
                    ShowErrorPopup("Fout bij opslaan van notitie");
                }
                else if (response is WebRequestData<string> dataResponse)
                {
                    ShowStatus("Notitie succesvol opgeslagen!", false);
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
            Debug.LogException(ex);
        }
    }

    private async void LoadNotes()
    {
        try
        {
            string token = SecureUserSession.Instance.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                ShowErrorPopup("Geen token beschikbaar");
                return;
            }

            webClient.SetToken(token);
            IWebRequestReponse response = await webClient.SendGetRequest("api/Notitie");

            if (response is WebRequestData<List<Notitie>> data)
            {
                List<Notitie> notes = data.Data;
                if (notes == null || notes.Count == 0)
                {
                    ShowStatus("Geen notities gevonden", true);
                    return;
                }

                foreach (var note in notes)
                {
                    AddNoteToUI(note);
                }
                ShowStatus($"{notes.Count} notities geladen", false);
            }
            else if (response is WebRequestError errorResponse)
            {
                string errorMessage = errorResponse?.ErrorMessage ?? "Unknown error";
                ShowErrorPopup("Fout bij laden van notities");
                Debug.LogError($"Failed to load notes: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            ShowErrorPopup("Er is een fout opgetreden");
            Debug.LogException(ex);
        }
    }

    private void AddNoteToUI(Notitie note)
    {
        Debug.Log($"Note loaded: {note.Titel}");
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
    }

    private void OnPopupCloseButtonClick()
    {
        ErrorPopup.SetActive(false);
    }

    private IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToNoteScene();
    }
}
