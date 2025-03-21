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
    public GameObject noteButtonPrefab;  // Het prefab voor de NoteButton
    public Transform notePanel;          // Het panel waar de notes in komen
    public TextMeshProUGUI noNotesText;  // Text die "Geen notities" toont

    [Header("API Connection")]
    public WebClient webClient;

    [Header("Error Popup")]
    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    // Flag to track if critical components are missing
    private bool hasMissingComponents = false;

    void Start()
    {
        // Check for critical components
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
        {
            ErrorPopup.SetActive(false);
        }
        if (popupCloseButton != null)
        {
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);
        }

        // Only load notes if we have the necessary components
        if (!hasMissingComponents)
        {
            LoadNotes();
        }
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

        // Display error message if components are missing
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
            // Check if critical components are missing before proceeding
            if (hasMissingComponents)
            {
                Debug.LogError("Kan notities niet laden: essentiële componenten ontbreken");
                return;
            }

            // Verwijder bestaande notities in UI als ze er zijn
            ClearNotesFromUI();

            // Toon "Laden..." tijdens het ophalen van de notities
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

            // Verwerk verschillende soorten responses
            if (response is WebRequestData<List<Notitie>> listData)
            {
                // Direct ontvangen als List<Notitie>
                ProcessNotes(listData.Data);
            }
            else if (response is WebRequestData<Notitie[]> arrayData)
            {
                // Ontvangen als Notitie[]
                ProcessNotes(new List<Notitie>(arrayData.Data));
            }
            else if (response is WebRequestData<string> stringData)
            {
                // Ontvangen als string (JSON)
                string json = stringData.Data;
                Debug.Log($"Received JSON: {json}");

                try
                {
                    // Direct deserialize to array since the JSON starts with [
                    if (json.TrimStart().StartsWith("["))
                    {
                        // Unity's JsonUtility can't directly deserialize arrays, so we need to wrap it
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

        // We hebben notities, dus verberg de "Geen notities" tekst
        if (noNotesText != null)
        {
            noNotesText.gameObject.SetActive(false);
        }

        bool addedAtLeastOne = false;

        // Voeg elke notitie toe aan de UI
        foreach (var note in notes)
        {
            if (note != null && !string.IsNullOrEmpty(note.titel))  // Changed from note.Titel to note.titel
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
        // Verwijder alle bestaande note buttons uit het panel
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
        // Check of de essentiële componenten aanwezig zijn
        if (noteButtonPrefab == null || notePanel == null)
        {
            Debug.LogError("Note button prefab of note panel is niet ingesteld!");
            return;
        }

        // Instantieer een nieuwe note button
        GameObject noteButtonObj = Instantiate(noteButtonPrefab, notePanel);

        // Zoek naar het TitleText component (TextMeshProUGUI of Text)
        TextMeshProUGUI titleTMP = noteButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        Text titleText = noteButtonObj.GetComponentInChildren<Text>();

        // Stel de titel in op de juiste component
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

            // Probeer een generic UI component te vinden om de titel te tonen
            Component[] textComponents = noteButtonObj.GetComponentsInChildren<Component>();
            bool foundTextComponent = false;

            foreach (var component in textComponents)
            {
                if (component.GetType().Name.Contains("Text"))
                {
                    // Gebruik reflectie om de text property te zetten
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

        // Voeg een click handler toe die de volledige notitie opent
        Button button = noteButtonObj.GetComponent<Button>();
        if (button != null)
        {
            // Maak een lokale kopie van de notitie voor de delegate
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
        // Sla de huidige notitie op in een statische variabele of PlayerPrefs
        // om deze te kunnen openen in het detail scherm
        PlayerPrefs.SetString("CurrentNoteTitle", note.Titel);
        PlayerPrefs.SetString("CurrentNoteText", note.Tekst ?? ""); // Voorkom null reference
        PlayerPrefs.SetString("CurrentNoteDate", note.DatumAanmaak ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

        // Laad het detail scherm
        // SceneManager.LoadScene("NoteDetailScene");
        Debug.Log($"Opening note detail: {note.Titel}");

        // Als je nog geen detail scene hebt, kun je de notitie informatie tonen in een popup
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
            // Fallback als de popup niet beschikbaar is
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

    // Helper class voor JSON deserialisatie
    [Serializable]
    private class NotitiesWrapper
    {
        public List<Notitie> items;
    }
}