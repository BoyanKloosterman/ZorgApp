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
                Checkpoint parsedCheckpoint = JsonUtility.FromJson<Checkpoint>(dataResponse.Data);
                text.text = parsedCheckpoint.tekst; 

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
