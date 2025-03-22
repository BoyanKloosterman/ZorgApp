using Assets.Scripts.ApiClient.ModelApiClient;
using Assets.Scripts.Model;
using System.Collections.Generic;
using System;
using System.Linq;
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

    public Patient patient;
    public PatientApiClient patientApiClient;

    private List<Patient> allPatients = new List<Patient>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set up search UI
        if (searchInputField != null)
        {
            searchInputField.onValueChanged.AddListener(FilterPatients);
        }

        GetPatients();
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

    }

    public void ClearList()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}