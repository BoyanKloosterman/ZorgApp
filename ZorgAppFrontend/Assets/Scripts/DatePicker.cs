using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

public class DatePicker : MonoBehaviour
{
    [Header("UI Elementen")]
    [SerializeField] private InputField dateText;
    [SerializeField] private Button openDatePickerButton;
    [SerializeField] private GameObject datePickerPanel; 
    [SerializeField] private Button previousMonthButton; 
    [SerializeField] private Button nextMonthButton;
    [SerializeField] private Text monthYearText; 
    [SerializeField] private GameObject dateButtonPrefab; 
    [SerializeField] private Transform dayButtonContainer;

    [SerializeField] private AddPatientController addPatientController;

    private Button[] dayButtons;
    private DateTime selectedDate;

    private void Awake()
    {
        // Zorg dat de luisteraar wordt toegevoegd voordat andere methoden worden aangeroepen
        if (openDatePickerButton != null)
        {
            openDatePickerButton.onClick.AddListener(ToggleDatePicker);
        }
        else
        {
            Debug.LogError("OpenDatePickerButton is niet ingesteld!");
        }
    }

    private void Start()
    {
        CreateDayButtons();
        SetupButtonListeners();
        
        // Controleer of alle essentiële componenten aanwezig zijn
        ValidateComponents();

        // Zorg dat de DatePicker NIET opent bij start
        if (datePickerPanel != null)
        {
            datePickerPanel.SetActive(false);
        }

        // Stel de huidige datum in
        selectedDate = DateTime.Now;
        UpdateDateText();
        
        // Voeg luisteraars toe voor navigatie knoppen
        if (previousMonthButton != null)
            previousMonthButton.onClick.AddListener(PreviousMonth);
        if (nextMonthButton != null)
            nextMonthButton.onClick.AddListener(NextMonth);
        
        // Update de kalender
        UpdateCalendar();
    }

    private void ValidateComponents()
    {
        if (dateText == null)
            Debug.LogError("DateText is niet ingesteld!");
        if (datePickerPanel == null)
            Debug.LogError("DatePickerPanel is niet ingesteld!");
        if (dayButtonContainer == null)
            Debug.LogError("DayButtonContainer is niet ingesteld!");
        if (monthYearText == null)
            Debug.LogError("MonthYearText is niet ingesteld!");
    }

    private void CreateDayButtons()
    {
        // Verwijder bestaande buttons
        foreach (Transform child in dayButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Maak nieuwe buttons
        dayButtons = new Button[31];
        for (int i = 0; i < 31; i++)
        {
            GameObject buttonObj = Instantiate(dateButtonPrefab, dayButtonContainer);
            Button button = buttonObj.GetComponent<Button>();
            
            // Zet tekst voor de dag
            TextMeshProUGUI textComponent = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            Text legacyText = buttonObj.GetComponentInChildren<Text>();
            
            if (textComponent != null)
            {
                textComponent.text = (i + 1).ToString();
            }
            else if (legacyText != null)
            {
                legacyText.text = (i + 1).ToString();
            }

            dayButtons[i] = button;
        }
    }

    private void SetupButtonListeners()
    {
        if (dayButtons != null)
        {
            for (int i = 0; i < dayButtons.Length; i++)
            {
                int dayIndex = i; // Lokale variabele voor closure
                if (dayButtons[i] != null)
                {
                    dayButtons[i].onClick.AddListener(() => OnDaySelected(dayButtons[dayIndex]));
                }
            }
        }
    }

    private void ToggleDatePicker()
    {
        if (datePickerPanel != null)
        {
            bool isCurrentlyActive = datePickerPanel.activeSelf;
            datePickerPanel.SetActive(!isCurrentlyActive);

            if (addPatientController != null)
                {
                    addPatientController.SetGeboorteDatum(selectedDate.ToString("dd-MM-yyyy"));
                }
                else
                {
                    Debug.LogError("AddPatientController is niet gekoppeld in de DatePicker!");
                }
            if (!isCurrentlyActive)
            {
                UpdateCalendar();
            }
        }
    }

    private void PreviousMonth()
    {
        selectedDate = selectedDate.AddMonths(-1);
        UpdateCalendar();
    }

    private void NextMonth()
    {
        selectedDate = selectedDate.AddMonths(1);
        UpdateCalendar();
    }

    private void UpdateCalendar()
    {
        // Gebruik Nederlandse maandnamen
        CultureInfo dutchCulture = new CultureInfo("nl-NL");

        // Update maand en jaar in Nederlandse stijl
        if (monthYearText != null)
        {
            monthYearText.text = selectedDate.ToString("MMMM yyyy", dutchCulture);
            Debug.Log(monthYearText.text);
        }
        
        // Verkrijg aantal dagen in de maand
        int daysInMonth = DateTime.DaysInMonth(selectedDate.Year, selectedDate.Month);
        
        // Update dagknoppen
        for (int i = 0; i < dayButtons.Length; i++)
        {
            if (dayButtons[i] != null)
            {
                if (i < daysInMonth)
                {
                    dayButtons[i].gameObject.SetActive(true);

                    // Update dag tekst
                    TextMeshProUGUI tmpText = dayButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    Text legacyText = dayButtons[i].GetComponentInChildren<Text>();

                    if (tmpText != null)
                    {
                        tmpText.text = (i + 1).ToString();
                    }
                    else if (legacyText != null)
                    {
                        legacyText.text = (i + 1).ToString();
                    }
                }
                else
                {
                    dayButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }


    private void OnDaySelected(Button selectedButton)
    {
        Debug.Log("Day button selected!");

        // Haal de dag op uit de knoptekst
        string dayText = "";
        TextMeshProUGUI tmpText = selectedButton.GetComponentInChildren<TextMeshProUGUI>();
        Text legacyText = selectedButton.GetComponentInChildren<Text>();

        if (tmpText != null)
        {
            dayText = tmpText.text;
        }
        else if (legacyText != null)
        {
            dayText = legacyText.text;
        }

        Debug.Log($"Selected day text: {dayText}");

        // Converteer naar een getal en update de geselecteerde datum
        if (int.TryParse(dayText, out int selectedDay))
        {
            selectedDate = new DateTime(selectedDate.Year, selectedDate.Month, selectedDay);
            Debug.Log($"Updated selectedDate: {selectedDate}");

            UpdateDateText();
            ToggleDatePicker();
        }
        else
        {
            Debug.LogError("Kon de geselecteerde dag niet omzetten naar een getal.");
        }
    }

    private void UpdateDateText()
    {
        if (dateText != null)
        {
            // Use specific format for input field
            dateText.text = selectedDate.ToString("dd-MM-yyyy");
            
            // Optional: inform AddPatientController
            if (addPatientController != null)
            {
                addPatientController.SetGeboorteDatum(dateText.text);
            }
            
            Debug.Log($"Updated date text: {dateText.text}");
        }
    }
}