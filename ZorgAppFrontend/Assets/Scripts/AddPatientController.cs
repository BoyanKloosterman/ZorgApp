using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.ApiClient.ModelApiClient;
using Newtonsoft.Json;

public class AddPatientController : MonoBehaviour
{
    [Header("Kind Informatie")]
    [SerializeField] private InputField kindNaamInput;
    [SerializeField] private InputField geboorteDatumInput;

    [Header("Arts Selectie")]
    public Dropdown artsDropdown;

    [Header("Behandelplan")]
    [SerializeField] private Dropdown behandelplanDropdown;

    [Header("Afspraak")]
    [SerializeField] private InputField afspraakTimeInput; // Voeg dit toe aan je UI
    [SerializeField] private Button openDateTimePickerButton; // Knop om de datumpicker te openen

    [SerializeField] private Button opslaanButton;

    // DateTimePicker referentie
    [SerializeField] private DateTimePicker dateTimePicker;

    // API Clients
    public PatientApiClient patientApiClient;
    public ArtsApiClient artsApiClient;
    public OuderVoogdApiClient ouderVoogdApiClient;
    public TrajectApiClient trajectApiClient;
    public AfspraakApiClient afspraakApiClient; // Nieuwe AfspraakApiClient

    // Lokale lijsten
    private List<Traject> beschikbareTraject = new List<Traject>();
    private List<Arts> beschikbareArtsen = new List<Arts>();
    private List<OuderVoogd> beschikbareOuders = new List<OuderVoogd>();

    // Geselecteerde gegevens
    private DateTime geselecteerdeGeboorteDatum;
    private Arts geselecteerdeArts;
    private OuderVoogd geselecteerdeOuder;
    private DateTime? geselecteerdeAfspraakDatum; // Nieuwe variabele voor afspraakdatum

    // Kalender variabelen
    private DateTime huidigeDatum = DateTime.Now;
    private OuderVoogd huidigeOuderVoogd;

    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;


    void Start()
    {
        // Initialiseer UI elementen
        InitialiseerUIElementen();
        if (ErrorPopup != null) ErrorPopup.SetActive(false);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(() => ErrorPopup.SetActive(false));
        if (geboorteDatumInput != null)
        {
            geboorteDatumInput.onEndEdit.AddListener(delegate { OnGeboorteDatumGewijzigd(); });
        }

        if (opslaanButton != null)
        {
            opslaanButton.onClick.AddListener(OpslaanPatientGegevens);
        }

        // Koppel de DateTimePicker-knop
        if (openDateTimePickerButton != null && dateTimePicker != null)
        {
            openDateTimePickerButton.onClick.AddListener(OpenDateTimePicker);
        }

        // Koppel afspraak input om wijzigingen op te vangen
        if (afspraakTimeInput != null)
        {
            afspraakTimeInput.onEndEdit.AddListener(delegate { OnAfspraakTijdGewijzigd(); });
        }

        LaadBehandelplannen();
        LaadHuidigeOuderVoogd();
    }
    
    void OpenDateTimePicker()
    {
        if (dateTimePicker != null)
        {
            dateTimePicker.OpenDateTimePicker();
        }
    }

    void OnAfspraakTijdGewijzigd()
    {
        string inputText = afspraakTimeInput.text;
        Debug.Log($"Ingevoerde afspraakdatum: {inputText}");

        // Probeer de datum te parsen
        string[] dateFormats = {
            "dd-MM-yyyy HH:mm",
            "d-M-yyyy H:mm"
        };

        bool datumGevonden = false;
        foreach (string format in dateFormats)
        {
            if (DateTime.TryParseExact(inputText, format, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                geselecteerdeAfspraakDatum = parsedDate;
                Debug.Log($"✅ Geldige afspraakdatum geparseerd: {geselecteerdeAfspraakDatum:dd-MM-yyyy HH:mm}");
                datumGevonden = true;
                break;
            }
        }

        if (!datumGevonden)
        {
            Debug.LogError($"❌ Kan afspraakdatum niet parsen: {inputText}");
            geselecteerdeAfspraakDatum = null;
        }
    }

    async System.Threading.Tasks.Task LaadHuidigeOuderVoogd()
    {
        if (ouderVoogdApiClient == null) return;
        
        IWebRequestResponse response = await ouderVoogdApiClient.GetCurrentOuderInfo();
        
        switch (response)
        {
            case WebRequestData<OuderVoogd> dataResponse:
                huidigeOuderVoogd = dataResponse.Data;
                Debug.Log($"Huidige ouder/voogd geladen: {huidigeOuderVoogd.voornaam} {huidigeOuderVoogd.achternaam}");
                break;
                
            case WebRequestError errorResponse:
                Debug.LogError($"Fout bij laden huidige ouder/voogd: {errorResponse.ErrorMessage}");
                break;
        }
    }

    void InitialiseerUIElementen()
    {
        // Configureer arts dropdown listener
        if (artsDropdown != null)
            artsDropdown.onValueChanged.AddListener(OnArtsGeselecteerd);
    }

    async System.Threading.Tasks.Task LaadBehandelplannen()
    {
        await LaadArtsen();
        await LaadEnPopuleerBehandelplannen();
    }

    async System.Threading.Tasks.Task LaadArtsen()
    {
        if (artsApiClient == null || artsDropdown == null) return;

        IWebRequestResponse response = await artsApiClient.GetArtsen();

        switch (response)
        {
            case WebRequestData<List<Arts>> dataResponse:
                beschikbareArtsen = dataResponse.Data;
                PopuleerArtsDropdown();
                VergrootDropdownTekst(artsDropdown);
                break;
            case WebRequestError errorResponse:
                Debug.LogError($"Fout bij laden artsen: {errorResponse.ErrorMessage}");
                break;
        }
    }

    void PopuleerArtsDropdown()
    {
        artsDropdown.ClearOptions();

        // Voeg standaard optie toe
        List<string> artsenNamen = new List<string> { "Selecteer arts" };

        // Voeg artsen toe
        artsenNamen.AddRange(beschikbareArtsen.Select(arts =>
            $"{arts.voornaam} {arts.achternaam}"));

        artsDropdown.AddOptions(artsenNamen);
    }

    void OnArtsGeselecteerd(int index)
    {
        // Controleer of index geldig is en niet de standaard optie is
        if (index > 0 && beschikbareArtsen.Count >= index)
        {
            geselecteerdeArts = beschikbareArtsen[index - 1];
        }
        else
        {
            geselecteerdeArts = null;
        }
    }

    public async void OpslaanPatientGegevens()
    {
        if (!ValideerFormulier()) return;

        // Valideer geboortedatum
        if (!DateTime.TryParseExact(geboorteDatumInput.text, "dd-MM-yyyy", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out geselecteerdeGeboorteDatum))
        {
            Debug.LogWarning("Ongeldige geboortedatum ingevoerd.");
            return;
        }

        Debug.Log($"Geldige geboortedatum: {geselecteerdeGeboorteDatum.ToShortDateString()}");

        // Maak een nieuwe patiënt aan
        Patient nieuwePatient = new Patient
        {
            voornaam = kindNaamInput.text,
            achternaam = huidigeOuderVoogd?.achternaam ?? string.Empty,
            trajectid = GetGeselecteerdBehandelplanId(),
            artsid = geselecteerdeArts?.id ?? 0,
            Geboortedatum = geselecteerdeGeboorteDatum,
            oudervoogdid = huidigeOuderVoogd?.id ?? 0,
            userid = huidigeOuderVoogd?.userid ?? string.Empty,
        };

        // Debug JSON output
        string jsonData = JsonConvert.SerializeObject(nieuwePatient, Formatting.Indented);
        Debug.Log("Verzonden JSON: " + jsonData);

        // Stuur naar API
        IWebRequestResponse response = await patientApiClient.CreatePatient(nieuwePatient);

        switch (response)
        {
            case WebRequestData<Patient> dataResponse:
                Debug.Log($"✅ Patiënt succesvol opgeslagen: {dataResponse.Data}");
                
                // Nadat patiënt is opgeslagen, maak een afspraak aan als er een datum is geselecteerd
                if (geselecteerdeAfspraakDatum.HasValue && afspraakApiClient != null)
                {
                    await MaakAfspraakAan(dataResponse.Data);
                }
                break;
                
            case WebRequestError errorResponse:
                Debug.LogError($"❌ Fout bij opslaan patiënt: {errorResponse.ErrorMessage}");
                break;
        }
    }

    private async System.Threading.Tasks.Task MaakAfspraakAan(Patient patient)
    {
        if (!geselecteerdeAfspraakDatum.HasValue || geselecteerdeArts == null)
        {
            Debug.LogWarning("Geen afspraakdatum of arts geselecteerd voor het maken van een afspraak.");
            return;
        }

        // Maak een nieuwe afspraak aan
        Afspraak nieuweAfspraak = new Afspraak
        {
            UserId = patient.userid,
            ArtsId = geselecteerdeArts.id.ToString(),
            Datumtijd = geselecteerdeAfspraakDatum.Value.ToString("yyyy-MM-ddTHH:mm:ss")
        };

        // Debug JSON output
        string jsonData = JsonConvert.SerializeObject(nieuweAfspraak, Formatting.Indented);
        Debug.Log("Afspraak JSON: " + jsonData);

        // Stuur naar API
        IWebRequestResponse response = await afspraakApiClient.CreateAfspraak(nieuweAfspraak);

        switch (response)
        {
            case WebRequestData<Afspraak> dataResponse:
                Debug.Log($"✅ Afspraak succesvol opgeslagen: {dataResponse.Data.id}");
                break;
                
            case WebRequestError errorResponse:
                Debug.LogError($"❌ Fout bij opslaan afspraak: {errorResponse.ErrorMessage}");
                break;
        }
    }

    public void OnGeboorteDatumGewijzigd()
    {
        string inputText = geboorteDatumInput.text;
        Debug.Log($"Ingevoerde geboortedatum: {inputText}");

        // Probeer de datum te parsen met meerdere formaten
        string[] dateFormats = {
            "dd-MM-yyyy",   // Standaard formaat
            "d-MM-yyyy",    // Enkele cijfers zonder voorloop nul
            "dd/MM/yyyy",   // Slash separator
            "yyyy-MM-dd"    // ISO formaat
        };

        bool datumGevonden = false;
        foreach (string format in dateFormats)
        {
            if (DateTime.TryParseExact(inputText, format, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                geselecteerdeGeboorteDatum = parsedDate;
                Debug.Log($"✅ Geldige geboortedatum geparseerd: {geselecteerdeGeboorteDatum:dd-MM-yyyy}");
                datumGevonden = true;
                break;
            }
        }

        if (!datumGevonden)
        {
            Debug.LogError($"❌ Kan datum niet parsen: {inputText}");
            geselecteerdeGeboorteDatum = default;
        }
    }

    public void SetGeboorteDatum(string datum)
    {
        Debug.Log($"SetGeboorteDatum aangeroepen met: {datum}");
        
        if (geboorteDatumInput != null)
        {
            geboorteDatumInput.text = datum;
            
            // Forceer het aanroepen van de wijzigingsmethode
            OnGeboorteDatumGewijzigd();
        }
        else
        {
            Debug.LogError("geboorteDatumInput is null!");
        }
    }

    public void SetAfspraakDatum(string datum)
    {
        Debug.Log($"SetAfspraakDatum aangeroepen met: {datum}");
        
        if (afspraakTimeInput != null)
        {
            afspraakTimeInput.text = datum;
            
            // Forceer het aanroepen van de wijzigingsmethode
            OnAfspraakTijdGewijzigd();
        }
        else
        {
            Debug.LogError("afspraakTimeInput is null!");
        }
    }

    int GetGeselecteerdBehandelplanId()
    {
        return behandelplanDropdown.value;
    }

    bool ValideerFormulier()
    {
        if (string.IsNullOrWhiteSpace(kindNaamInput.text))
        {
            ShowErrorPopup("Naam van het kind is verplicht.");
            return false;
        }

        if (geselecteerdeGeboorteDatum == default)
        {
            ShowErrorPopup("Selecteer een geldige geboortedatum.");
            return false;
        }

        if (artsDropdown.value <= 0)
        {
            ShowErrorPopup("Selecteer een arts.");
            return false;
        }

        if (behandelplanDropdown.value <= 0)
        {
            ShowErrorPopup("Selecteer een behandelplan.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(afspraakTimeInput.text))
        {
            ShowErrorPopup("Selecteer een afspraakdatum en tijd.");
            return false;
        }

        return true;
    }

    async System.Threading.Tasks.Task LaadEnPopuleerBehandelplannen()
    {
        if (trajectApiClient == null || behandelplanDropdown == null) return;

        IWebRequestResponse response = await trajectApiClient.GetBehandelplannen();
        Debug.Log($"Behandelplannen geladen: {response}");

        switch (response)
        {
           case WebRequestData<List<Traject>> dataResponse:
                if (dataResponse.Data != null)
                {
                    Debug.Log($"Behandelplannen geladen: {dataResponse.Data.Count} items");
                    beschikbareTraject = dataResponse.Data;
                }
                else
                {
                    Debug.LogWarning("dataResponse.Data is null, geen behandelplannen ontvangen.");
                    beschikbareTraject = new List<Traject>(); // Lege lijst instellen om fouten te voorkomen
                }
                PopuleerBehandelplanDropdown();
                VergrootDropdownTekst(behandelplanDropdown);
                break;
            case WebRequestError errorResponse:
                Debug.Log($"Fout bij laden behandelplannen: {errorResponse.ErrorMessage}");
                break;
        }
    }
    void PopuleerBehandelplanDropdown()
    {
        behandelplanDropdown.ClearOptions();

        List<string> behandelplanNamen = new List<string> { "Selecteer behandelplan" };

        behandelplanNamen.AddRange(beschikbareTraject.Select(plan => plan.naam));

        behandelplanDropdown.AddOptions(behandelplanNamen);
    }
    void VergrootDropdownTekst(Dropdown dropdown)
    {
        // Zorg ervoor dat de Template actief is zodat we bij de items kunnen
        Transform template = dropdown.template;
        if (template == null) return;

        Transform content = template.Find("Viewport/Content");
        if (content != null)
        {
            foreach (Transform item in content)
            {
                // Probeer Text component eerst
                Text textComponent = item.GetComponentInChildren<Text>();
                
                // Als Text component niet werkt, probeer TextMeshProUGUI
                if (textComponent == null)
                {
                    TextMeshProUGUI tmpTextComponent = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpTextComponent != null)
                    {
                        tmpTextComponent.fontSize = 22;
                    }
                }
                else
                {
                    // Oude Text component aanpassen
                    textComponent.fontSize = 22;
                }

                RectTransform itemRect = item.GetComponent<RectTransform>();
                if (itemRect != null)
                {
                    itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, 50);
                }
            }
        }
    }
    private void ShowErrorPopup(string message)
    {
        if (ErrorPopup != null)
        {
            if (popupMessageText != null) popupMessageText.text = message;
            ErrorPopup.SetActive(true);
        }
    }
}