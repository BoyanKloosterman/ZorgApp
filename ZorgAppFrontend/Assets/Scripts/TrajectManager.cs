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

    public List<int> zorgMomentIds = new List<int>();
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
        if (scene.name == "Traject13")
        {
            LoadZorgMomenten();
        }
    }

    private void Start()
    {
        LoadZorgMomenten();

        if (noteButton != null)
            noteButton.onClick.AddListener(GoToNoteScene);
    }

    public async void LoadZorgMomenten()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.LoadZorgMomenten();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                try
                {
                    zorgMomentIds = JsonHelper.ParseJsonArray<int>(dataResponse.Data);
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