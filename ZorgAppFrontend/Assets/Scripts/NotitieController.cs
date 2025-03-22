using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NotitieController : MonoBehaviour
{
    [Header("UI Elements")]
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
            hasMissingComponents = true;
        }

        if (notePanel == null)
        {
            hasMissingComponents = true;
        }

        if (noNotesText == null)
        {
            hasMissingComponents = true;
        }

        if (webClient == null)
        {
            hasMissingComponents = true;
        }

        if (hasMissingComponents && noNotesText != null)
        {
            noNotesText.gameObject.SetActive(true);
            noNotesText.text = "Configuratiefout: Controleer de console en Inspector";
        }
    }

    public void ReturnToRouteScene()
    {
        SceneManager.LoadScene("Route13");
    }

    public void ReturnToNoteScene()
    {
        SceneManager.LoadScene("NoteScene");
    }

    private async void LoadNotes()
    {
        try
        {
            if (hasMissingComponents)
            {
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
                try
                {
                    if (json.TrimStart().StartsWith("["))
                    {
                        List<Notitie> notes = JsonHelper.ParseJsonArray<Notitie>(json);

                        if (notes != null && notes.Count > 0)
                        {
                            ProcessNotes(notes);
                        }
                        else
                        {
                            UpdateNoNotesText("Geen notities gevonden");
                        }
                    }
                    else
                    {
                        UpdateNoNotesText("Onverwacht JSON formaat");
                    }
                }
                catch (Exception)
                {
                    UpdateNoNotesText("Fout bij verwerken van notities");
                }
            }
            else if (response is WebRequestError errorResponse)
            {
                string errorMessage = errorResponse?.ErrorMessage ?? "Unknown error";
                UpdateNoNotesText("Fout bij laden van notities");
                ShowErrorPopup("Fout bij laden van notities");
            }
            else
            {
                UpdateNoNotesText("Onbekende fout bij laden");
                ShowErrorPopup("Onbekende respons bij laden van notities");
            }
        }
        catch (Exception)
        {
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
            UpdateNoNotesText("Geen notities gevonden");
            return;
        }

        // Sort notes by creation/update date in descending order
        notes.Sort((note1, note2) => DateTime.Parse(note2.DatumAanmaak).CompareTo(DateTime.Parse(note1.DatumAanmaak)));

        if (noNotesText != null)
        {
            noNotesText.gameObject.SetActive(false);
        }

        bool addedAtLeastOne = false;

        foreach (var note in notes)
        {
            if (note != null && !string.IsNullOrEmpty(note.Titel))
            {
                AddNoteToUI(note);
                addedAtLeastOne = true;
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
            Component[] textComponents = noteButtonObj.GetComponentsInChildren<Component>();

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
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        //
                    }
                }
            }
        }

        Button button = noteButtonObj.GetComponent<Button>();
        if (button != null)
        {
            Notitie capturedNote = note;
            button.onClick.AddListener(() => OpenEditNoteScene(capturedNote));
        }
    }

    private void OpenEditNoteScene(Notitie note)
    {
        PlayerPrefs.SetInt("CurrentNoteId", note.id);
        PlayerPrefs.SetString("CurrentNoteTitle", note.Titel);
        PlayerPrefs.SetString("CurrentNoteText", note.Tekst ?? "");
        PlayerPrefs.SetString("CurrentNoteDate", note.DatumAanmaak ?? DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
        SceneManager.LoadScene("NoteEditScene");
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
}
