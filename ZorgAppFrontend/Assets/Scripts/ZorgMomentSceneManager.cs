using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ZorgMomentSceneManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public UserApiClient userApiClient;
    public string url;

    private void Start()
    {
        PerformLoadzorgMomentData();
    }

    public async void PerformLoadzorgMomentData()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.LoadZorgMomentData(TrajectManager.Instance.zorgMomentID);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                ZorgMoment parsedzorgMoment = JsonUtility.FromJson<ZorgMoment>(dataResponse.Data);
                text.text = parsedzorgMoment.tekst;
                url = parsedzorgMoment.url;
                break;
            case WebRequestError errorResponse:
                Debug.LogError("Error: " + errorResponse.ErrorMessage);
                break;
            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    public async Task<bool> PerformFinishZorgMoment()
    {
        int currentZorgMomentId = TrajectManager.Instance.zorgMomentID;
        List<int> zorgMomentIds = TrajectManager.Instance.zorgMomentIds;
        int currentIndex = zorgMomentIds.IndexOf(currentZorgMomentId);

        if (currentIndex == -1)
        {
            Debug.LogError("Current zorgmoment ID not found in the list.");
            return false;
        }

        for (int i = 0; i < currentIndex; i++)
        {
            int prevId = zorgMomentIds[i];
            if (!TrajectManager.Instance.behaaldeZorgMomentIds.Contains(prevId))
            {
                Debug.Log("Cannot complete this zorgmoment: Previous zorgmoment " + prevId + " not completed.");
                return false;
            }
        }

        IWebRequestResponse webRequestResponse = await userApiClient.FinishZorgMoment(currentZorgMomentId);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                return true;
            case WebRequestError errorResponse:
                Debug.LogError("Error: " + errorResponse.ErrorMessage);
                return false;
            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    public async void ReturnToRouteScene()
    {
        await PerformFinishZorgMoment();
        SceneManager.LoadScene("traject" + TrajectManager.Instance.trajectNumber);
    }

    public void OpenUrlInBrowser()
    {
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                Application.OpenURL(url);
            }
            catch (Exception e)
            {
                Debug.LogError("Error opening URL: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("URL is empty");
        }
    }

    private string ParseUrlFromHyperlink(string hyperlinkText)
    {
        // This method extracts the URL from the formatted hyperlink text
        int startIndex = hyperlinkText.IndexOf("href=\"") + 6;
        int endIndex = hyperlinkText.IndexOf("\"", startIndex);

        if (startIndex > 5 && endIndex > startIndex)
        {
            return hyperlinkText.Substring(startIndex, endIndex - startIndex);
        }

        return string.Empty;
    }
}