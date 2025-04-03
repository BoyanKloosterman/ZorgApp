using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;
using System.Collections.Generic;

public class DateTimePicker : MonoBehaviour
{
    [Header("UI Elementen")]
    public GameObject datumTijdPanel;
    public Button openDateTimePickerButton;
    public InputField afspraakTimeInput;
    public Button bevestigSelectieButton;

    [Header("Datum Selectie")]
    public Text huidigeMaandLabel;
    public Button vorigeMaandButton;
    public Button volgendeMaandButton;
    public GridLayoutGroup datumGrid;

    [Header("Prefab")]
    public GameObject datumButtonPrefab;

    [Header("Tijd Selectie")]
    public Dropdown uurDropdown;

    [Header("Controller Referentie")]
    public AddPatientController patientController;

    private DateTime geselecteerdeDatum;
    private DateTime huidigeMaand;

    void Awake()
    {
        InitialiseerComponenten();
    }

    void InitialiseerComponenten()
    {
        if (datumTijdPanel == null)
            Debug.LogWarning("DateTimePicker: Geen DateTimePaneel gekoppeld!");

        if (openDateTimePickerButton == null)
            Debug.LogWarning("DateTimePicker: Geen OpenDateTimePicker Button gekoppeld!");

        if (afspraakTimeInput == null)
            Debug.LogWarning("DateTimePicker: Geen AfspraakTime Input gekoppeld!");
    }

    void Start()
    {
        // Verberg panel bij start
        if (datumTijdPanel != null)
            datumTijdPanel.SetActive(false);

        // Configureer buttons
        if (openDateTimePickerButton != null)
            openDateTimePickerButton.onClick.AddListener(OpenDateTimePicker);
        
        if (vorigeMaandButton != null)
            vorigeMaandButton.onClick.AddListener(GaNaarVorigeMaand);
        
        if (volgendeMaandButton != null)
            volgendeMaandButton.onClick.AddListener(GaNaarVolgendeMaand);

        // Voeg bevestig selectie button toe
        if (bevestigSelectieButton != null)
            bevestigSelectieButton.onClick.AddListener(BevestigSelectie);

        // Initialiseer uur dropdown
        InitialiseerUurDropdown();

        // Eerste initialisatie van de maand
        if (datumGrid != null && huidigeMaandLabel != null)
            SetMaand(DateTime.Now);
    }

    void InitialiseerUurDropdown()
    {
        if (uurDropdown != null)
        {
            uurDropdown.ClearOptions();

            List<Dropdown.OptionData> uurOpties = new List<Dropdown.OptionData>();
            for (int i = 9; i <= 17; i++)
            {
                uurOpties.Add(new Dropdown.OptionData($"{i}:00"));
            }

            uurDropdown.AddOptions(uurOpties);

            int huidigUur = DateTime.Now.Hour;
            if (huidigUur >= 9 && huidigUur <= 17)
            {
                uurDropdown.value = huidigUur - 9;
            }
            else
            {
                uurDropdown.value = 0;
            }

            // Vergroot dropdown
            VergrootDropdownTekst(uurDropdown);
        }
    }

    public void OpenDateTimePicker()
    {
        if (datumTijdPanel != null)
        {
            datumTijdPanel.SetActive(true);
        }
    }

    public void BevestigSelectie()
    {
        // Controleer of er een datum is geselecteerd
        if (geselecteerdeDatum == default)
        {
            Debug.LogWarning("Selecteer eerst een datum!");
            return;
        }

        // Controleer of de datum in de toekomst ligt
        if (geselecteerdeDatum.Date < DateTime.Today)
        {
            Debug.LogWarning("Je kunt geen datum in het verleden selecteren");
            return;
        }

        // Haal geselecteerde tijd op
        int geselecteerdUur = uurDropdown != null ? (uurDropdown.value + 9) : 9;

        // Combineer datum en tijd
        DateTime volledigeDatum = new DateTime(
            geselecteerdeDatum.Year, 
            geselecteerdeDatum.Month, 
            geselecteerdeDatum.Day, 
            geselecteerdUur, 
            0, 
            0
        );

        // Extra validatie voor tijdstip
        if (volledigeDatum < DateTime.Now)
        {
            return; // Stilzwijgend afbreken (geen warning zoals gevraagd)
        }

        // Update input field
        if (afspraakTimeInput != null)
        {
            afspraakTimeInput.text = volledigeDatum.ToString("dd-MM-yyyy HH:mm");
        }

        // Update de patient controller als die is ingesteld
        if (patientController != null)
        {
            patientController.SetAfspraakDatum(volledigeDatum.ToString("dd-MM-yyyy HH:mm"));
        }

        // Sluit panel
        SluitDateTimePicker();
    }

    void VergrootDropdownTekst(Dropdown dropdown)
    {
        if (dropdown.template == null) return;

        RectTransform templateRectTransform = dropdown.template;
        
        // Vergroot de template
        templateRectTransform.sizeDelta = new Vector2(templateRectTransform.sizeDelta.x, 1000);

        // Zoek de content
        Transform content = templateRectTransform.Find("Viewport/Content");
        if (content == null) return;

        foreach (Transform item in content)
        {
            // Vergroot tekst en items
            Text textComponent = item.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.fontSize = 40; // Grotere tekst
            }

            RectTransform itemRectTransform = item.GetComponent<RectTransform>();
            if (itemRectTransform != null)
            {
                itemRectTransform.sizeDelta = new Vector2(itemRectTransform.sizeDelta.x, 50); // Hogere items
            }
        }
    }
    
    void SetMaand(DateTime maand)
    {
        if (datumGrid == null || huidigeMaandLabel == null)
        {
            Debug.LogWarning("Kan maand niet instellen - datumGrid of huidigeMaandLabel is null!");
            return;
        }

        // Controleer of prefab is ingesteld
        if (datumButtonPrefab == null)
        {
            Debug.LogWarning("Geen datum button prefab ingesteld!");
            return;
        }

        huidigeMaand = new DateTime(maand.Year, maand.Month, 1);
        
        // Update maand label
        huidigeMaandLabel.text = huidigeMaand.ToString("MMMM yyyy");

        // Clear bestaande datum knoppen
        foreach (Transform child in datumGrid.transform)
        {
            Destroy(child.gameObject);
        }

        // Bepaal eerste dag van de week voor de eerste van de maand
        int startDag = (int)huidigeMaand.DayOfWeek;
        int dagenInMaand = DateTime.DaysInMonth(huidigeMaand.Year, huidigeMaand.Month);

        // Vul lege plekken voor de eerste dag
        for (int i = 0; i < startDag; i++)
        {
            GameObject leegDagObject = new GameObject("LeegDag");
            leegDagObject.transform.SetParent(datumGrid.transform, false);
        }

        // Maak knoppen voor elke dag van de maand met prefab
        for (int dag = 1; dag <= dagenInMaand; dag++)
        {
            DateTime dagDatum = new DateTime(huidigeMaand.Year, huidigeMaand.Month, dag);
            
            // Controleer of de datum in het verleden is
            if (dagDatum.Date < DateTime.Today)
            {
                // Maak een lege plek voor verleden dagen
                GameObject leegDagObject = new GameObject("VerledenDag");
                leegDagObject.transform.SetParent(datumGrid.transform, false);
                continue;
            }
            
            // Instantieer prefab
            GameObject dagKnopObject = Instantiate(datumButtonPrefab, datumGrid.transform);
            
            // Configureer tekst op de knop
            Text dagTekst = dagKnopObject.GetComponentInChildren<Text>();
            if (dagTekst != null)
            {
                dagTekst.text = dag.ToString();
            }
            
            // Zoek de Button component
            Button dagKnop = dagKnopObject.GetComponent<Button>();
            if (dagKnop != null)
            {
                int huidigeDag = dag;
                dagKnop.onClick.AddListener(() => SelecteerDatum(huidigeDag));
            }

            // Geef een naam voor herkenbaarheid
            dagKnopObject.name = $"Dag{dag}";
        }
    }

    void SelecteerDatum(int dag)
    {
        DateTime mogelijkeDatum = new DateTime(huidigeMaand.Year, huidigeMaand.Month, dag);
        
        // Controleer of datum in de toekomst of vandaag is
        if (mogelijkeDatum.Date >= DateTime.Today)
        {
            geselecteerdeDatum = mogelijkeDatum;
        }
    }

    void GaNaarVorigeMaand()
    {
        // Controleer of vorige maand niet volledig in het verleden ligt
        DateTime vorigeMaand = huidigeMaand.AddMonths(-1);
        DateTime laatsteDagVorigeMaand = new DateTime(vorigeMaand.Year, vorigeMaand.Month, 
                                        DateTime.DaysInMonth(vorigeMaand.Year, vorigeMaand.Month));
        if (laatsteDagVorigeMaand.Date < DateTime.Today)
        {
            Debug.LogWarning("Je kunt niet verder terug in de tijd!");
            return;
        }
        SetMaand(vorigeMaand);
    }

    void GaNaarVolgendeMaand()
    {
        // Controleer of volgende maand niet volledig in de toekomst ligt
        DateTime volgendeMaand = huidigeMaand.AddMonths(1);
        DateTime eersteDagVolgendeMaand = new DateTime(volgendeMaand.Year, volgendeMaand.Month, 1);
        if (eersteDagVolgendeMaand.Date > DateTime.Today.AddMonths(1))
        {
            Debug.LogWarning("Je kunt niet verder in de toekomst kijken!");
            return;
        }
        SetMaand(volgendeMaand);
    }

    void SluitDateTimePicker()
    {
        if (datumTijdPanel != null)
        {
            datumTijdPanel.SetActive(false);
        }
    }
}