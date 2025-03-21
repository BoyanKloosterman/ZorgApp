using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectManager : MonoBehaviour
{
    public static TrajectManager Instance;
    public UserApiClient userApiClient;

    public int zorgMomentID;
    public string trajectNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // Nieuwe event listener
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Nieuwe methode die wordt aangeroepen bij elke scene load
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene geladen: {scene.name}");

        if (scene.name == "Traject13") // Exacte scene naam check
        {
            Debug.Log("Traject13 scene gedetecteerd");
            LoadBehaaldeZorgMomenten();
        }
    }

    private void Start()
    {
        LoadBehaaldeZorgMomenten();
    }

    public async void LoadBehaaldeZorgMomenten()
    {
        IWebRequestReponse webRequestResponse = await userApiClient.LoadBehaaldeZorgMomenten();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                Debug.Log("Response data raw: " + dataResponse.Data);
                break;
            case WebRequestError errorResponse:
                Debug.LogError("Error: " + errorResponse.ErrorMessage);
                break;
            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
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
        SceneManager.sceneLoaded -= OnSceneLoaded; // Belangrijk voor cleanup
    }
}