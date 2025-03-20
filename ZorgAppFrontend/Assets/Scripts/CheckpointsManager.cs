using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSceneManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UserApiClient userApiClient;

    private void Start()
    {
        Debug.Log($"Checkpoint: {RoutesManager.Instance.checkpointID}, Route: {RoutesManager.Instance.routeName}");
        PerformLoadCheckpointData();
    }

    public async void PerformLoadCheckpointData()
    {
        IWebRequestReponse webRequestResponse = await userApiClient.LoadCheckpointData(RoutesManager.Instance.checkpointID);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                // Gebruik JsonUtility om de JSON te deserialiseren naar een Checkpoint object
                Checkpoint parsedCheckpoint = JsonUtility.FromJson<Checkpoint>(dataResponse.Data);

                // Toegang krijgen tot 'naam'
                Debug.Log("Naam: " + parsedCheckpoint.naam);
                text.text = parsedCheckpoint.naam;  // Stel de naam in op je UI-element

                break;
            case WebRequestError errorResponse:
                Debug.LogError("Error: " + errorResponse.ErrorMessage);
                break;

            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    public void ReturnToRouteScene()
    {
        SceneManager.LoadScene(RoutesManager.Instance.routeName);
    }
}
