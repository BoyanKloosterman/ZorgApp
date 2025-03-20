using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NotitieController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField titleInput;
    public TMP_InputField textInput;
    public Button saveButton;
    public Button backButton;
    public TMP_Text statusMessage;

    [Header("Note Data")]
    private string userId;

    [Header("API Connection")]
    public WebClient webClient;

    void Start()
    {
        if (!SecureUserSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("User not logged in. Redirecting to login screen.");
            SceneManager.LoadScene("Login");
            return;
        }

        PlayerPrefs.GetString("userId", userId);

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID is missing. This should not happen for logged-in users.");
            SceneManager.LoadScene("Login");
            return;
        }

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNote);

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);

        if (statusMessage != null)
            statusMessage.text = "";

        Debug.Log($"Using userId: {userId}");
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
            UserId = userId,
            Titel = titleInput.text,
            Tekst = textInput.text,
            DatumAanmaak = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        Debug.Log($"Saving note with userId {userId}: {JsonUtility.ToJson(newNote)}");

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