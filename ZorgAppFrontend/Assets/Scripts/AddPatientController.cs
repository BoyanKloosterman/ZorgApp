using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Model;
using Assets.Scripts.ApiClient.ModelApiClient;
using Newtonsoft.Json;
using UI.Dates;

public class AddPatientController : MonoBehaviour
{
    [Header("Kind Informatie")]
    [SerializeField] private InputField kindNaamInput;
    public UI.Dates.DatePicker geboorteDatumInput;

    [Header("Arts Selectie")]
    public Dropdown artsDropdown;

    [Header("Behandelplan")]
    [SerializeField] private Dropdown behandelplanDropdown;

    [Header("Afspraak")]
    [SerializeField] private InputField afspraakTimeInput;
    [SerializeField] private Button openDateTimePickerButton;
    [SerializeField] private Button opslaanButton;
    public InputField Email;
    public InputField Password;

    // API Clients
    public PatientApiClient patientApiClient;
    public ArtsApiClient artsApiClient;
    public OuderVoogdApiClient ouderVoogdApiClient;
    public TrajectApiClient trajectApiClient;
    public AfspraakApiClient afspraakApiClient;

    // Lokale lijsten
    private List<Traject> beschikbareTraject = new List<Traject>();
    private List<Arts> beschikbareArtsen = new List<Arts>();
    private List<OuderVoogd> beschikbareOuders = new List<OuderVoogd>();

    private Arts geselecteerdeArts;
    private OuderVoogd geselecteerdeOuder;
    private DateTime? geselecteerdeAfspraakDatum;
    private DateTime huidigeDatum = DateTime.Now;
    private OuderVoogd huidigeOuderVoogd;

    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;
    public UserApiClient userApiClient;

    void Start()
    {
        // Initialiseer UI elementen
        InitialiseerUIElementen();

        if (ErrorPopup != null) ErrorPopup.SetActive(false);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(() => ErrorPopup.SetActive(false));

        if (opslaanButton != null)
        {
            opslaanButton.onClick.AddListener(OpslaanPatientGegevens);
        }

        // Koppel afspraak input om wijzigingen op te vangen
        if (afspraakTimeInput != null)
        {
            afspraakTimeInput.onEndEdit.AddListener(delegate { OnAfspraakTijdGewijzigd(); });
        }

        LaadBehandelplannen();
        LaadHuidigeOuderVoogd();
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
        Debug.Log($"Huidige ouder/voogd geladen: {response}");

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

        DateTime geboorteDatum = geboorteDatumInput.SelectedDate.Date;

        System.Random random = new System.Random();
        int randomAvatarId = random.Next(1, 31); 

        Patient newUser = new Patient
        {
            voornaam = kindNaamInput.text,
            achternaam = huidigeOuderVoogd?.achternaam ?? string.Empty,
            email = Email.text.Trim(),
            password = Password.text,
            trajectid = GetGeselecteerdBehandelplanId(),
            artsid = geselecteerdeArts?.id ?? 0,
            Geboortedatum = geboorteDatum,
            oudervoogdid = huidigeOuderVoogd?.userid ?? string.Empty,
            userid = huidigeOuderVoogd?.userid ?? string.Empty,
            avatarId = randomAvatarId
        };
        Debug.Log($"Patiënt gegevens: {JsonConvert.SerializeObject(newUser)}"); 
        IWebRequestResponse webRequestResponse = await patientApiClient.CreatePatient(newUser);
        Debug.Log($"Patiënt gegevens opgeslagen: {webRequestResponse}");

        switch (webRequestResponse)
        {
            case WebRequestData<Patient> dataResponse:
                Debug.Log($"✅ Patiënt succesvol opgeslagen: {dataResponse.Data}");
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

        string eersteGesprekNaam = "Kennismaking met " + kindNaamInput.text;
        DateTime eersteAfspraakDatum = geselecteerdeAfspraakDatum.Value;

        Afspraak eersteAfspraak = new Afspraak
        {
            UserId = patient.userid,
            ArtsId = geselecteerdeArts.id.ToString(),
            Datumtijd = eersteAfspraakDatum.ToString("yyyy-MM-ddTHH:mm:ss"),
            naam = eersteGesprekNaam,
        };

        IWebRequestResponse eersteResponse = await afspraakApiClient.CreateAfspraak(eersteAfspraak);

        if (eersteResponse is WebRequestData<Afspraak> eersteDataResponse)
        {
            Debug.Log($"✅ Eerste afspraak succesvol opgeslagen: {eersteDataResponse.Data.id}");

            string vervolgGesprekNaam = "Vervolgafspraak met " + kindNaamInput.text;
            DateTime vervolgAfspraakDatum = eersteAfspraakDatum.AddDays(14);

            Afspraak vervolgAfspraak = new Afspraak
            {
                UserId = patient.userid,
                ArtsId = geselecteerdeArts.id.ToString(),
                Datumtijd = vervolgAfspraakDatum.ToString("yyyy-MM-ddTHH:mm:ss"),
                naam = vervolgGesprekNaam,
            };

            IWebRequestResponse vervolgResponse = await afspraakApiClient.CreateAfspraak(vervolgAfspraak);

            if (vervolgResponse is WebRequestData<Afspraak> vervolgDataResponse)
            {
                Debug.Log($"✅ Vervolgafspraak succesvol opgeslagen: {vervolgDataResponse.Data.id}");
            }
            else if (vervolgResponse is WebRequestError vervolgErrorResponse)
            {
                Debug.LogError($"❌ Fout bij opslaan vervolgafspraak: {vervolgErrorResponse.ErrorMessage}");
            }
        }
        else if (eersteResponse is WebRequestError eersteErrorResponse)
        {
            Debug.LogError($"❌ Fout bij opslaan eerste afspraak: {eersteErrorResponse.ErrorMessage}");
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
        int index = behandelplanDropdown.value - 1; // -1 omdat index 0 de standaardoptie is
        if (index >= 0 && index < beschikbareTraject.Count)
        {
            return beschikbareTraject[index].id; // Haal de ID op van de correcte selectie
        }
        Debug.LogError("Geen geldig behandelplan geselecteerd.");
        return -1; // Of een andere standaardwaarde
    }

    bool ValideerFormulier()
    {
        if (string.IsNullOrWhiteSpace(kindNaamInput.text))
        {
            ShowErrorPopup("Naam van het kind is verplicht.");
            return false;
        }

        if (!geboorteDatumInput.SelectedDate.HasValue)
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

        if (string.IsNullOrWhiteSpace(Email.text) || !Email.text.Contains("@"))
        {
            ShowErrorPopup("Voer een geldig e-mailadres in.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(Password.text) || Password.text.Length < 8)
        {
            ShowErrorPopup("Wachtwoord moet minimaal 8 karakters lang zijn.");
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
        // Implementatie voor het laden van behandelplannen
        
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

    void ShowErrorPopup(string message)
    {
        if (ErrorPopup != null && popupMessageText != null)
        {
            popupMessageText.text = message;
            ErrorPopup.SetActive(true);
        }
        else
        {
            Debug.LogError($"Fout: {message}");
        }
    }
}
