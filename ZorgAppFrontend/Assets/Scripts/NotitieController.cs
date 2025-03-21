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
    public Button backButton;
    public TMP_Text statusMessage;

    [Header("API Connection")]
    public WebClient webClient;

    void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNote);

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);

        if (statusMessage != null)
            statusMessage.text = "";

        LoadNotes();
    }

    public void GoToNoteAddScene()
    {
        SceneManager.LoadScene("NoteAddScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("Route13");
    }

    public async void SaveNote()
    {
        if (string.IsNullOrEmpty(titleInput.text))
        {
            ShowStatus("Titel is verplicht", true);
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
            Debug.Log($"Token: {token}");

            if (!string.IsNullOrEmpty(token))
            {
                webClient.SetToken(token);
                IWebRequestReponse response = await webClient.SendPostRequest("api/Notitie", noteJson);

                if (response is WebRequestError errorResponse)
                {
                    string errorMessage = errorResponse?.ErrorMessage ?? "Unknown error";
                    ShowStatus("Fout bij opslaan van notitie", true);
                    Debug.LogError($"Failed to save note: {errorMessage}");
                }
                else if (response is WebRequestData<string> dataResponse)
                {
                    Debug.Log($"Note saved successfully: {dataResponse.Data}");
                    ShowStatus("Notitie succesvol opgeslagen!", false);
                    StartCoroutine(ReturnToMainAfterDelay(2f));
                }
                else
                {
                    ShowStatus("Onbekende respons", true);
                    Debug.LogWarning($"Unknown response type: {response?.GetType().Name}");
                }
            }
            else
            {
                ShowStatus("Geen token beschikbaar", true);
                Debug.LogError("Cannot save note: No auth token available");
            }
        }
        catch (Exception ex)
        {
            ShowStatus("Er is een fout opgetreden", true);
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
                ShowStatus("Geen token beschikbaar", true);
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

                if (notes.Count >= 0)
                {
                    PlayerPrefs.SetString("userId", notes[0].UserId);
                    PlayerPrefs.Save();
                    Debug.Log($"Stored userId: {notes[0].UserId}");
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
                ShowStatus("Fout bij laden van notities", true);
                Debug.LogError($"Failed to load notes: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            ShowStatus("Er is een fout opgetreden", true);
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

    private IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        GoBack();
    }
}
