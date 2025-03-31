using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class TrajectManager : MonoBehaviour
{
    public static TrajectManager Instance;
    public UserApiClient userApiClient;
    public Button noteButton;

    public List<int> zorgMomentIds = new List<int>();
    public List<int> behaaldeZorgMomentIds = new List<int>();
    public int zorgMomentID;
    public string trajectNumber;
    public event Action OnZorgMomentenUpdated;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Traject13" || scene.name == "Traject8") 
        {
            LoadZorgMomenten();
            LoadBehaaldeZorgMomenten();
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
}