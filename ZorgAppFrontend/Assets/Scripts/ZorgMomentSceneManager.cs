using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZorgMomentSceneManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UserApiClient userApiClient;

    private void Start()
    {
        Debug.Log($"zorgMoment: {TrajectManager.Instance.zorgMomentID}, Route: {TrajectManager.Instance.trajectNumber}");
        PerformLoadzorgMomentData();
    }

    public async void PerformLoadzorgMomentData()
    {
        IWebRequestReponse webRequestResponse = await userApiClient.LoadZorgMomentData(TrajectManager.Instance.zorgMomentID);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                ZorgMoment parsedzorgMoment = JsonUtility.FromJson<ZorgMoment>(dataResponse.Data);
                text.text = parsedzorgMoment.tekst; 

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
        SceneManager.LoadScene("traject" + TrajectManager.Instance.trajectNumber);
    }
}
