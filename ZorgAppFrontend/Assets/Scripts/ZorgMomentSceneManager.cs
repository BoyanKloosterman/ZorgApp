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
    public string videoUrl;
    public string infoUrl;
    
    private ZorgMoment currentZorgMoment;
    private bool isCompleting = false;


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
                currentZorgMoment = JsonUtility.FromJson<ZorgMoment>(dataResponse.Data);
                text.text = currentZorgMoment.tekst;
                videoUrl = currentZorgMoment.videoUrl;
                infoUrl = currentZorgMoment.infoUrl;
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
        // Prevent multiple completions
        if (isCompleting)
            return false;
                
        isCompleting = true;
        
        int currentZorgMomentId = TrajectManager.Instance.zorgMomentID;
        List<int> zorgMomentIds = TrajectManager.Instance.zorgMomentIds;
        int currentIndex = zorgMomentIds.IndexOf(currentZorgMomentId);

        if (currentIndex == -1)
        {
            Debug.LogError("Current zorgmoment ID not found in the list.");
            isCompleting = false;
            return false;
        }

        // Check if prerequisites are completed
        for (int i = 0; i < currentIndex; i++)
        {
            int prevId = zorgMomentIds[i];
            if (!TrajectManager.Instance.behaaldeZorgMomentIds.Contains(prevId))
            {
                isCompleting = false;
                return false;
            }
        }

        // Check if zorgmoment is already completed
        bool isAlreadyCompleted = TrajectManager.Instance.behaaldeZorgMomentIds.Contains(currentZorgMomentId);

        // Only make API call if not already completed
        if (!isAlreadyCompleted)
        {
            IWebRequestResponse webRequestResponse = await userApiClient.FinishZorgMoment(currentZorgMomentId);

            switch (webRequestResponse)
            {
                case WebRequestData<string> dataResponse:
                    TrajectManager.Instance.behaaldeZorgMomentIds.Add(currentZorgMomentId);
                    
                    // Only show popup for newly completed zorgmoments
                    if (PopupManager.Instance != null && currentZorgMoment != null)
                    {
                        PopupManager.Instance.ShowSuccessPopup(currentZorgMoment.naam);
                        return true;
                    }
                    break;
                case WebRequestError errorResponse:
                    Debug.LogError("Error: " + errorResponse.ErrorMessage);
                    isCompleting = false;
                    return false;
                default:
                    throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
            }
        }
        
        // If already completed or after API call, just go back to trajectory screen
        SceneManager.LoadScene("traject" + TrajectManager.Instance.trajectNumber);
        return true;
    }

    public async void ReturnToRouteScene()
    {
        // Only do the completion if not already in progress
        if (!isCompleting)
        {
            bool success = await PerformFinishZorgMoment();
            
            // If something went wrong or popup isn't available, go back to traject scene
            if (!success && !isCompleting)
            {
                SceneManager.LoadScene("traject" + TrajectManager.Instance.trajectNumber);
            }
            // If successful with popup, the popup will handle navigation
        }
    }

    public void OpenVideoUrlInBrowser()
    {
        if (!string.IsNullOrEmpty(videoUrl))
        {
            Application.OpenURL(videoUrl);

        }
        else
        {
            Debug.LogError("URL is empty");
        }
    }

    public void OpenInfoUrlInBrowser()
    {
        if (!string.IsNullOrEmpty(infoUrl))
        {
            Application.OpenURL(infoUrl);
        }
        else
        {
            Debug.LogError("URL is empty");
        }
    }


}