using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Assets.Scripts.ApiClient.ModelApiClient;
using Assets.Scripts.Model;

public class TrajectManager : MonoBehaviour
{
    public static TrajectManager Instance;
    public UserApiClient userApiClient;
    public PatientApiClient patientApiClient;
    public Button noteButton;

    public List<int> zorgMomentIds = new List<int>();
    public List<int> behaaldeZorgMomentIds = new List<int>();
    public int zorgMomentID;
    public string trajectNumber;
    public event Action OnZorgMomentenUpdated;

    public GameObject[] positions;
    public GameObject avatar;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Traject13" || scene.name == "Traject8")
        {
            LoadCurrentAvatar();
            LoadZorgMomenten();
            LoadBehaaldeZorgMomenten();


            if (avatar == null) // Controleer of avatar al bestaat
            {
                avatar = GameObject.FindWithTag("Avatar");

            }
            avatar.SetActive(false);

            await EnsureDataLoaded();

            PlaceAvatarAtLastCompletedMoment();
            Debug.Log("Avatar geplaatst op laatst behaalde zorgmoment");
        }
    }

    private async Task EnsureDataLoaded()
    {
        // Wait until zorgMomentIds and behaaldeZorgMomentIds are filled
        while (zorgMomentIds.Count == 0)
        {
            await Task.Delay(100); // Wait 100ms and check again
        }

        // Ensure buttons are found and sorted correctly
        while (positions == null || positions.Length == 0 || positions[0] == null)
        {
            positions = GameObject.FindGameObjectsWithTag("ButtonZorgMoment")
                          .OrderBy(p => int.Parse(System.Text.RegularExpressions.Regex.Match(p.name, @"\d+").Value))
                          .ToArray();
            await Task.Delay(100); // Wait and try again
        }
    }


    //private void Start()
    //{
    //    LoadZorgMomenten();
    //    if (zorgMomentIds.Count != 13)
    //    {
    //        SceneManager.LoadScene("Traject" + zorgMomentIds.Count);
    //    }
    //    LoadBehaaldeZorgMomenten();
    //}



    public async void LoadZorgMomenten()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.LoadZorgMomenten();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                zorgMomentIds = JsonHelper.ParseJsonArray<int>(dataResponse.Data);
                OnZorgMomentenUpdated?.Invoke();
                break;
            case WebRequestError errorResponse:
                Debug.LogError($"API error: {errorResponse.ErrorMessage}");
                break;
        }
    }

    public async void LoadBehaaldeZorgMomenten()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.LoadBehaaldeZorgMomenten();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                behaaldeZorgMomentIds = JsonHelper.ParseJsonArray<int>(dataResponse.Data);
                OnZorgMomentenUpdated?.Invoke();
                break;
            case WebRequestError errorResponse:
                Debug.LogError($"API error: {errorResponse.ErrorMessage}");
                break;
        }
    }

    public async void LoadCurrentAvatar()
    {
        IWebRequestResponse webRequestResponse = await patientApiClient.GetPatientAvatar();
        switch (webRequestResponse)
        {
            case WebRequestData<Patient> dataResponse:
                int avatarId = dataResponse.Data.avatarId ?? 1;
                Debug.Log("Avatar ID: " + avatarId);
                avatar.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Avatars/{avatarId}");
                break;
            case WebRequestError errorResponse:
                Debug.LogError($"API error: {errorResponse.ErrorMessage}");
                break;
        }
    }

    public int GetNextEnabledIndex()
    {
        for (int i = 0; i < zorgMomentIds.Count; i++)
        {
            if (!behaaldeZorgMomentIds.Contains(zorgMomentIds[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public int GetCurrentAvatarIndex()
    {
        if (avatar == null || positions == null || positions.Length == 0)
        {
            return -1; // Ongeldige index
        }

        for (int i = 0; i < positions.Length; i++)
        {
            if (avatar.transform.position == positions[i].transform.position)
            {
                return i;
            }
        }

        return -1; // Geen match gevonden
    }

    public void LoadZorgMomentScene(int id, string number)
    {
        zorgMomentID = id;
        trajectNumber = number;
        SceneManager.LoadScene("ZorgMoment");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GoToNoteScene()
    {
        SceneManager.LoadScene("NoteScene");
    }

    public async Task PlaceAvatarAtLastCompletedMoment()
    {

        if (avatar == null)
        {
            avatar = GameObject.FindWithTag("Avatar");
        }
        

        if (avatar == null || positions == null || positions.Length == 0)
        {
            Debug.LogWarning("Avatar of positions niet gevonden!");
            return;
        }

        // Zoek het laatst behaalde zorgmoment
        int lastCompletedIndex = -1;
        foreach (int zorgMomentID in behaaldeZorgMomentIds)
        {
            int index = zorgMomentIds.IndexOf(zorgMomentID);
            if (index > lastCompletedIndex)
            {
                lastCompletedIndex = index;
                Debug.Log("Laatst behaalde zorgmoment index: " + lastCompletedIndex);
            }
        }

        // Als er behaalde momenten zijn, plaats de avatar daar
        if (lastCompletedIndex != -1)
        {
            avatar.transform.position = positions[lastCompletedIndex].transform.position;
        }
        else
        {
            // Als er geen behaalde momenten zijn, plaats avatar bij het eerste zorgmoment
            avatar.transform.position = positions[0].transform.position;
        }
        avatar.SetActive(true);
    }
}