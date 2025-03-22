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
    [SerializeField] public Transform contentParent;
    [SerializeField] public GameObject PatientPrefab;

    [Header("Search Features")]
    [SerializeField] private TMP_InputField searchInputField;

    [Header("Dropdown Menus")]
    [SerializeField] private TMP_Dropdown artsDropdown;
    [SerializeField] private TMP_Dropdown ouderVoogdDropdown;
    [SerializeField] private TMP_Dropdown behandelplanDropdown;

    [Header("Patient Details Panel")]
    [SerializeField] private GameObject patientDetailsPanel;

    [Header("Patient Details UI")]
    [SerializeField] private TMP_Text patientNameText; 


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
    public void SetOuderVoogdDropdown(int? voogdId)
    {
        if (ouderVoogdDropdown == null || !voogdId.HasValue)
        {
            // Set default selection
            if (ouderVoogdDropdown != null)
                ouderVoogdDropdown.value = 0;
            return;
        }

        // Find the guardian in the list
        OuderVoogd selectedVoogd = allGuardians.FirstOrDefault(v => v.id == voogdId.Value);

        if (selectedVoogd != null)
        {
            // Find the corresponding dropdown index
            string guardianName = $"{selectedVoogd.voornaam} {selectedVoogd.achternaam}";
            int index = ouderVoogdDropdown.options.FindIndex(option => option.text == guardianName);

            if (index >= 0)
            {
                ouderVoogdDropdown.value = index;
            }
            else
            {
                ouderVoogdDropdown.value = 0; // Set to default if not found
            }
        }
        else
        {
            ouderVoogdDropdown.value = 0; // Set to default if not found
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
    }

    public async void SeePatient(int id)
    {
        // Find patient with this id
        Patient selectedPatient = allPatients.FirstOrDefault(p => p.id == id);
        if (selectedPatient != null)
        {
            // Set the selected patient to the patient field
            this.patient = selectedPatient;

            // Set the dropdown values for this patient
            if (selectedPatient.artsid.HasValue)
            {
                SetArtsDropdown(selectedPatient.artsid.Value);
            }
            else
            {
                // Set to default option (first in list)
                if (artsDropdown != null)
                    artsDropdown.value = 0;
            }

            SetOuderVoogdDropdown(selectedPatient.oudervoogd_id ?? 0);

            // Set behandelplan dropdown to default 
            if (behandelplanDropdown != null)
                behandelplanDropdown.value = 0;

            // Set the patient name text
            if (patientNameText != null)
            {
                patientNameText.text = $"{selectedPatient.voornaam} {selectedPatient.achternaam}";
            }

            Debug.Log($"Selected patient: {selectedPatient.voornaam} {selectedPatient.achternaam}");

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
        if (ouderVoogdDropdown != null && ouderVoogdApiClient != null)
        {
            // Clear existing options
            ouderVoogdDropdown.ClearOptions();

            try
            {
                // Get guardians from API
                IWebRequestResponse response = await ouderVoogdApiClient.GetOuderVoogden();

                // Process response
                switch (response)
                {
                    case WebRequestData<List<OuderVoogd>> dataResponse:
                        allGuardians = dataResponse.Data;

                        // Create dropdown options
                        List<string> options = new List<string>();
                        options.Add("Selecteer ouder/voogd"); // Default option

                        foreach (var guardian in allGuardians)
                        {
                            string fullName = $"{guardian.voornaam} {guardian.achternaam}";
                            options.Add(fullName);
                        }

                        // Add options to dropdown
                        ouderVoogdDropdown.AddOptions(options);
                        Debug.Log($"Loaded {allGuardians.Count} guardians");
                        break;

                    case WebRequestError errorResponse:
                        Debug.LogError($"Error fetching guardians: {errorResponse.ErrorMessage}");
                        // Add default option
                        ouderVoogdDropdown.AddOptions(new List<string> { "Geen ouders/voogden beschikbaar" });
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception when loading guardians: {ex.Message}");
                ouderVoogdDropdown.AddOptions(new List<string> { "Fout bij laden van ouders/voogden" });
            }
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