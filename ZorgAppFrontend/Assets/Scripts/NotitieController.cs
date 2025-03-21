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

    [Header("Note Display")]
    public GameObject noteButtonPrefab;
    public Transform notePanel;
    public Text noNotesText;

    [Header("API Connection")]
    public WebClient webClient;

    [Header("Error Popup")]
    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    private bool hasMissingComponents = false;

    void Start()
    {
        CheckRequiredComponents();

        if (saveButton != null)
            saveButton.onClick.AddListener(SaveNote);

        if (backNoteButton != null)
            backNoteButton.onClick.AddListener(ReturnToNoteScene);

        if (backRouteButton != null)
            backRouteButton.onClick.AddListener(ReturnToRouteScene);

        if (statusMessage != null)
            statusMessage.text = "";

        if (ErrorPopup != null)
            ErrorPopup.SetActive(false);

        if (popupCloseButton != null)
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);

        if (!hasMissingComponents)
            LoadNotes();
    }

    private void CheckRequiredComponents()
    {
        if (noteButtonPrefab == null)
        {
            Debug.LogError("NoteButtonPrefab is niet ingesteld! Stel dit in in de Inspector.");
            hasMissingComponents = true;
        }

        if (notePanel == null)
        {
            Debug.LogError("NotePanel is niet ingesteld! Stel dit in in de Inspector.");
            hasMissingComponents = true;
        }

        if (noNotesText == null)
        {
            Debug.LogError("NoNotesText is niet ingesteld! Stel dit in in de Inspector.");
        }

        if (webClient == null)
        {
            Debug.LogError("WebClient is niet ingesteld! Stel dit in in de Inspector.");
            hasMissingComponents = true;
        }

        if (hasMissingComponents && noNotesText != null)
        {
            noNotesText.gameObject.SetActive(true);
            noNotesText.text = "Configuratiefout: Controleer de console en Inspector";
        }
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

                if (response is WebRequestError)
                {
                    ShowErrorPopup("Fout bij opslaan van notitie");
                }
                else if (response is WebRequestData<string>)
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
            if (hasMissingComponents)
            {
                Debug.LogError("Kan notities niet laden: essentiële componenten ontbreken");
                return;
            }

            ClearNotesFromUI();

            if (noNotesText != null)
            {
                noNotesText.gameObject.SetActive(true);
                noNotesText.text = "Notities worden geladen...";
            }

            string token = SecureUserSession.Instance.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                ShowErrorPopup("Geen token beschikbaar");
                UpdateNoNotesText("Geen toegang: Token ontbreekt");
                return;
            }

            webClient.SetToken(token);
            IWebRequestReponse response = await webClient.SendGetRequest("api/Notitie");

            if (response is WebRequestData<List<Notitie>> listData)
            {
                ProcessNotes(listData.Data);
            }
            else if (response is WebRequestData<Notitie[]> arrayData)
            {
                ProcessNotes(new List<Notitie>(arrayData.Data));
            }
            else if (response is WebRequestData<string> stringData)
            {
                string json = stringData.Data;
                Debug.Log($"Received JSON: {json}");

                try
                {
                    if (json.TrimStart().StartsWith("["))
                    {
                        string wrappedJson = "{\"items\":" + json + "}";
                        NotitiesWrapper wrapper = JsonUtility.FromJson<NotitiesWrapper>(wrappedJson);

                        if (wrapper != null && wrapper.items != null && wrapper.items.Count > 0)
                        {
                            ProcessNotes(wrapper.items);
                        }
                        else
                        {
                            Debug.LogWarning("Geen notities gevonden in de JSON data");
                            UpdateNoNotesText("Geen notities gevonden");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Unexpected JSON format - expected array");
                        UpdateNoNotesText("Onverwacht JSON formaat");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"JSON parsing error: {ex.Message}");
                    UpdateNoNotesText("Fout bij verwerken van notities");
                }
            }
            else if (response is WebRequestError errorResponse)
            {
                string errorMessage = errorResponse?.ErrorMessage ?? "Unknown error";
                Debug.LogError($"Failed to load notes: {errorMessage}");
                UpdateNoNotesText("Fout bij laden van notities");
                ShowErrorPopup("Fout bij laden van notities");
            }
            else
            {
                Debug.LogError("Unknown response type");
                UpdateNoNotesText("Onbekende fout bij laden");
                ShowErrorPopup("Onbekende respons bij laden van notities");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            UpdateNoNotesText("Fout bij laden van notities");
            ShowErrorPopup("Er is een fout opgetreden");
        }
    }

    private void UpdateNoNotesText(string message)
    {
        if (noNotesText != null)
        {
            noNotesText.gameObject.SetActive(true);
            noNotesText.text = message;
        }
    }

    private void ProcessNotes(List<Notitie> notes)
    {
        if (notes == null || notes.Count == 0)
        {
            Debug.Log("No notes found or notes is null");
            UpdateNoNotesText("Geen notities gevonden");
            return;
        }

        Debug.Log($"Processing {notes.Count} notes");

        if (noNotesText != null)
        {
            noNotesText.gameObject.SetActive(false);
        }

        bool addedAtLeastOne = false;

        foreach (var note in notes)
        {
            if (note != null && !string.IsNullOrEmpty(note.titel))
            {
                Debug.Log($"Adding note to UI: {note.titel}");
                AddNoteToUI(note);
                addedAtLeastOne = true;
            }
            else
            {
                Debug.LogWarning($"Ongeldige notitie gevonden: {note?.id}, titel: {note?.titel}, is null: {note == null}");
            }
        }

        if (addedAtLeastOne)
        {
            ShowStatus($"{notes.Count} notities geladen", false);
        }
        else
        {
            UpdateNoNotesText("Geen geldige notities gevonden");
        }
    }

    private void ClearNotesFromUI()
    {
        if (notePanel != null)
        {
            foreach (Transform child in notePanel)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void AddNoteToUI(Notitie note)
    {
        if (noteButtonPrefab == null || notePanel == null)
        {
            Debug.LogError("Note button prefab of note panel is niet ingesteld!");
            return;
        }

        GameObject noteButtonObj = Instantiate(noteButtonPrefab, notePanel);

        TextMeshProUGUI titleTMP = noteButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        Text titleText = noteButtonObj.GetComponentInChildren<Text>();

        if (titleTMP != null)
        {
            titleTMP.text = note.Titel;
        }
        else if (titleText != null)
        {
            titleText.text = note.Titel;
        }
        else
        {
            Debug.LogWarning("Geen TextMeshProUGUI of Text component gevonden op de note button!");

            Component[] textComponents = noteButtonObj.GetComponentsInChildren<Component>();
            bool foundTextComponent = false;

            foreach (var component in textComponents)
            {
                if (component.GetType().Name.Contains("Text"))
                {
                    try
                    {
                        var property = component.GetType().GetProperty("text");
                        if (property != null)
                        {
                            property.SetValue(component, note.Titel);
                            foundTextComponent = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Fout bij het instellen van tekst via reflectie: {ex.Message}");
                    }
                }
            }

            if (!foundTextComponent)
            {
                Debug.LogError("Geen geschikt tekstcomponent gevonden op de note button!");
            }
        }

        Button button = noteButtonObj.GetComponent<Button>();
        if (button != null)
        {
            Notitie capturedNote = note;
            button.onClick.AddListener(() => OpenNoteDetail(capturedNote));
        }
        else
        {
            Debug.LogWarning("Geen Button component gevonden op de note button!");
        }

        Debug.Log($"Note toegevoegd aan UI: {note.Titel}");
    }

    private void OpenNoteDetail(Notitie note)
    {
        PlayerPrefs.SetString("CurrentNoteTitle", note.Titel);
        PlayerPrefs.SetString("CurrentNoteText", note.Tekst ?? "");
        PlayerPrefs.SetString("CurrentNoteDate", note.DatumAanmaak ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

        Debug.Log($"Opening note detail: {note.Titel}");

        ShowStatus($"Notitie: {note.Titel}", false);
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
            Debug.LogError($"Fout: {message}");
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

    [Serializable]
    private class NotitiesWrapper
    {
        public List<Notitie> items;
    }
}
