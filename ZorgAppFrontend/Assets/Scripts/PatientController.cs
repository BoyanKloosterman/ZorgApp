using Assets.Scripts.ApiClient.ModelApiClient;
using Assets.Scripts.Model;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PatientController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] public Transform contentParent;
    [SerializeField] public GameObject PatientPrefab;

    [Header("Search Features")]
    [SerializeField] private TMP_InputField searchInputField;

    [Header("Dropdown Menus")]
    [SerializeField] private TMP_Dropdown artsDropdown;
    [SerializeField] private TMP_Text ouderVoogdText;
    [SerializeField] private TMP_Dropdown behandelplanDropdown;

    [Header("Patient Details Panel")]
    [SerializeField] private GameObject patientDetailsPanel;

    [Header("Patient Details UI")]
    [SerializeField] private TMP_Text patientNameText;

    [Header("Avatar")]
    [SerializeField] private Image avatarImage;

    [Header("API Clients")]
    public Patient patient;
    public PatientApiClient patientApiClient;
    public ArtsApiClient artsApiClient;
    public OuderVoogdApiClient ouderVoogdApiClient;

    private List<Patient> allPatients = new List<Patient>();
    private List<Arts> allDoctors = new List<Arts>();   // Arts objects from API
    private List<OuderVoogd> allGuardians = new List<OuderVoogd>(); // OuderVoogd objects from API


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        patientDetailsPanel.SetActive(false); // Hide patient details panel initially
        // Set up search UI
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(FilterPatients);
        }

        // Load data for dropdowns
        await LoadDropdownData();

        // Get patients list
        GetPatients();
    }

    private async Task LoadDropdownData()
    {
        await LoadDoctors();
        await LoadGuardians();
        await LoadBehandelplannen();
    }

    // Set dropdown to specific value
    public void SetArtsDropdown(int? artsId)
    {
        if (artsDropdown == null || !artsId.HasValue)
        {
            // Set default selection
            if (artsDropdown != null)
                artsDropdown.value = 0;
            return;
        }

        // Find the doctor in the list
        Arts selectedArts = allDoctors.FirstOrDefault(a => a.id == artsId.Value);

        if (selectedArts != null)
        {
            // Find the corresponding dropdown index
            string doctorName = $"{selectedArts.voornaam} {selectedArts.achternaam}";
            int index = artsDropdown.options.FindIndex(option => option.text == doctorName);

            if (index >= 0)
            {
                artsDropdown.value = index;
            }
            else
            {
                artsDropdown.value = 0; // Set to default if not found
            }
        }
        else
        {
            artsDropdown.value = 0; // Set to default if not found
        }
    }

    // Update your SetOuderVoogdDropdown method to query the list
    public void SetOuderVoogdText(int? voogdId)
    {
        if (ouderVoogdText == null)
            return;

        if (!voogdId.HasValue || voogdId.Value == 0)
        {
            ouderVoogdText.text = "Geen ouder/voogd gekoppeld";
            return;
        }

        // Zoek de ouder/voogd op basis van ID
        OuderVoogd selectedVoogd = allGuardians.FirstOrDefault(v => v.id == voogdId.Value);

        if (selectedVoogd != null)
        {
            // Toon de naam van de ouder/voogd als tekst
            ouderVoogdText.text = $"{selectedVoogd.voornaam} {selectedVoogd.achternaam}";
        }
        else
        {
            // Als de ouder/voogd niet gevonden is
            ouderVoogdText.text = "Ouder/voogd niet gevonden";
        }
    }

    public async void GetPatients()
    {
        // Get all patients from the API
        IWebRequestResponse webRequestResponse = await patientApiClient.GetPatients();

        // Check the type of the response
        switch (webRequestResponse)
        {
            case WebRequestData<List<Patient>> dataResponse:
                Debug.Log("Amount of patients: " + dataResponse.Data.Count);
                allPatients = dataResponse.Data; // Store all patients
                ClearList();
                FillList(allPatients); // Display all patients initially
                break;
            case WebRequestError errorResponse:
                Debug.Log("Error: " + errorResponse.ErrorMessage);
                break;
            default:
                throw new System.NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    public async void UpdatePatient()
    {
        if (patient == null)
        {
            Debug.LogWarning("No patient selected");
            return;
        }
        // Get the selected doctor from the dropdown
        int selectedDoctorIndex = artsDropdown.value;
        int selectedDoctorId = selectedDoctorIndex > 0 ? allDoctors[selectedDoctorIndex - 1].id : 0;
        // Get the selected treatment plan from the dropdown
        int selectedPlanIndex = behandelplanDropdown.value;
        int selectedPlanId = selectedPlanIndex > 0 ? selectedPlanIndex : 0;
        // Update the patient with the selected doctor and treatment plan
        patient.artsid = selectedDoctorId;
        patient.trajectid = selectedPlanId;
        // Call the API to update the patient
        IWebRequestResponse webRequestResponse = await patientApiClient.UpdatePatient(new PatientDto { id = patient.id, artsid = selectedDoctorId, trajectid = selectedPlanId });
    }

    public void FilterPatients(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            // If search is empty, show all patients
            ClearList();
            FillList(allPatients);
            return;
        }

        // Convert search to lowercase for case-insensitive comparison
        string lowerSearch = searchText.ToLower();

        // Filter patients whose first or last name contains the search text
        var filteredPatients = allPatients.Where(p =>
            p.voornaam.ToLower().Contains(lowerSearch) ||
            p.achternaam.ToLower().Contains(lowerSearch)
        ).ToList();

        // Update the UI with filtered results
        ClearList();
        FillList(filteredPatients);
    }

    public void FillList(List<Patient> patients)
    {
        foreach (var patient in patients)
        {
            GameObject newEnv = Instantiate(PatientPrefab, contentParent);
            SetupPatientUI(newEnv, patient);
        }
    }

    void ConfigureButton(Transform parent, string buttonName, Action action)
    {
        Transform buttonTransform = parent.Find(buttonName);
        if (buttonTransform == null) return;

        Button button = buttonTransform.GetComponent<Button>();
        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => action?.Invoke());
    }

    void SetupPatientUI(GameObject newEnv, Patient patient)
    {
        // 1. Naam instellen
        Transform nameButton = newEnv.transform.Find("NameButton");
        if (nameButton != null)
        {
            TextMeshProUGUI nameText = nameButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (nameText != null)
                nameText.text = $"{patient.voornaam} {patient.achternaam}";
        }

        // 2. Klikfunctionaliteit op knoppen instellen
        ConfigureButton(newEnv.transform, "NameButton", () => SeePatient(patient.id));

        // voeg juiste avatar toe
        Transform avatarTransform = newEnv.transform.Find("Avatar");
        if (avatarTransform != null)
        {
            Image avatarImage = avatarTransform.GetComponent<Image>();
            if (avatarImage != null)
            {
                // Als er een avatarId is, gebruik deze om de avatar te vinden
                if (patient.avatarId.HasValue)
                {
                    // Zoek de avatar in de resources map
                    Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{patient.avatarId.Value}");
                    if (avatarSprite != null)
                    {
                        avatarImage.sprite = avatarSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"Avatar {patient.avatarId.Value} not found");
                    }
                }
                else
                {
                    // Gebruik een standaard avatar als er geen avatarId is
                    avatarImage.sprite = Resources.Load<Sprite>("Avatars/TempHero_0");
                }
            }
        }
    }

    public void SeePatient(int id)
    {
        // Find patient with this id
        Patient selectedPatient = allPatients.FirstOrDefault(p => p.id == id);
        if (selectedPatient != null)
        {
            // Set the selected patient to the patient field
            this.patient = selectedPatient;

            // Set the dropdown values for this patient
            SetArtsDropdown(selectedPatient.artsid);

            // Gebruik de nieuwe methode voor ouder/voogd
            SetOuderVoogdText(selectedPatient.oudervoogdid);

            // Set behandelplan dropdown to default 
            if (behandelplanDropdown != null)
                behandelplanDropdown.value = 0;

            // Set the patient name text
            if (patientNameText != null)
            {
                patientNameText.text = $"{selectedPatient.voornaam} {selectedPatient.achternaam}";
            }

            // Set the avatar image
            if (avatarImage != null)
            {
                // If there's an avatarId, use it to find the avatar
                if (selectedPatient.avatarId.HasValue)
                {
                    // Find the avatar in the resources folder
                    Sprite avatarSprite = Resources.Load<Sprite>($"Avatars/{selectedPatient.avatarId.Value}");
                    if (avatarSprite != null)
                    {
                        avatarImage.sprite = avatarSprite;
                    }
                    else
                    {
                        Debug.LogWarning($"Avatar {selectedPatient.avatarId.Value} not found");
                    }
                }
                else
                {
                    // Use a default avatar if there's no avatarId
                    avatarImage.sprite = Resources.Load<Sprite>("Avatars/TempHero_0");
                }
            }

            // Show the patient details panel
            if (patientDetailsPanel != null)
            {
                patientDetailsPanel.SetActive(true);
            }
        }
    }

    // Add a method to hide the patient details panel
    public void HidePatientDetails()
    {
        if (patientDetailsPanel != null)
        {
            patientDetailsPanel.SetActive(false);
        }
    }

    public void ClearList()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }

    private async Task LoadDoctors()
    {
        if (artsDropdown != null && artsApiClient != null)
        {
            // Clear existing options
            artsDropdown.ClearOptions();

            try
            {
                // Get doctors from API
                IWebRequestResponse response = await artsApiClient.GetArtsen();

                // Process response
                switch (response)
                {
                    case WebRequestData<List<Arts>> dataResponse:
                        allDoctors = dataResponse.Data;

                        // Create dropdown options
                        List<string> options = new List<string>();
                        options.Add("Selecteer een arts"); // Default option

                        foreach (var arts in allDoctors)
                        {
                            string fullName = $"{arts.voornaam} {arts.achternaam}";
                            options.Add(fullName);
                        }

                        // Add options to dropdown
                        artsDropdown.AddOptions(options);
                        Debug.Log($"Loaded {allDoctors.Count} doctors");
                        break;

                    case WebRequestError errorResponse:
                        Debug.LogError($"Error fetching doctors: {errorResponse.ErrorMessage}");
                        // Add default option
                        artsDropdown.AddOptions(new List<string> { "Geen artsen beschikbaar" });
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception when loading doctors: {ex.Message}");
                artsDropdown.AddOptions(new List<string> { "Fout bij laden van artsen" });
            }
        }
    }

    private async Task LoadGuardians()
    {
        if (ouderVoogdApiClient == null)
            return;

        try
        {
            // Get guardians from API
            IWebRequestResponse response = await ouderVoogdApiClient.GetOuderVoogden();

            // Process response
            switch (response)
            {
                case WebRequestData<List<OuderVoogd>> dataResponse:
                    allGuardians = dataResponse.Data;
                    Debug.Log($"Loaded {allGuardians.Count} guardians");
                    break;

                default:
                    Debug.LogError($"Error fetching guardians: Unable to process response of type {response.GetType().Name}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception when loading guardians: {ex.Message}");
        }
    }

    private async Task LoadBehandelplannen()
    {
        if (behandelplanDropdown != null)
        {
            // Clear existing options
            behandelplanDropdown.ClearOptions();

            // Since there's no specific API client for treatment plans in the provided files,
            // we'll add placeholder data or you can implement the actual API call

            List<string> options = new List<string>();
            options.Add("Selecteer behandelplan"); // Default option
            options.Add("Behandelplan A");
            options.Add("Behandelplan B");
            options.Add("Behandelplan C");

            behandelplanDropdown.AddOptions(options);
            Debug.Log("Added placeholder treatment plans");

            // Note: Replace this with actual API call when available
        }
    }
}