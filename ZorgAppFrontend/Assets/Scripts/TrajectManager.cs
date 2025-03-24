using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TrajectManager : MonoBehaviour
{
    public static TrajectManager Instance;
    public UserApiClient userApiClient;
    public Button noteButton;

    public int HighestBehaaldId { get; private set; }

    public int zorgMomentID;
    public string trajectNumber;
    public List<int> BehaaldeZorgMomentIds;
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
        if (scene.name == "Traject13")
        {
            LoadBehaaldeZorgMomenten();
        }
    }

    private void Start()
    {
        LoadBehaaldeZorgMomenten();

        if (noteButton != null)
            noteButton.onClick.AddListener(GoToNoteScene);
    }

    public async void LoadBehaaldeZorgMomenten()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.LoadBehaaldeZorgMomenten();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                try
                {
                    List<BehaaldeZorgMoment> zorgMomenten =
                        JsonHelper.ParseJsonArray<BehaaldeZorgMoment>(dataResponse.Data);

                    BehaaldeZorgMomentIds = zorgMomenten
                        .Select(z => z.zorgMomentId)
                        .ToList();
                    HighestBehaaldId = BehaaldeZorgMomentIds.DefaultIfEmpty(0).Max();

                    OnZorgMomentenUpdated?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Parse error: {ex.Message}");
                }
                break;
            case WebRequestError errorResponse:
                Debug.LogError($"API error: {errorResponse.ErrorMessage}");
                break;
        }
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

[Serializable]
public class BehaaldeZorgMoment
{
    public int zorgMomentId;
}