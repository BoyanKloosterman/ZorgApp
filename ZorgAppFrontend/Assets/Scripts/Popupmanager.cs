using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Assets.Scripts.Model;
using Assets.Scripts.ApiClient;
using System.Threading.Tasks;



public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance;
    
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Text messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private float autoCloseTime = 15f;

    private ZorgMoment currentZorgMoment;
    public UserApiClient userApiClient;
    
    private string trajectSceneName;
    private List<string> encouragingMessages = new List<string>()
    {
        "Goed gedaan! Je bent een echte ster!",
        "Wat knap van jou! Je hebt het super gedaan!",
        "Geweldig! Je maakt goede vooruitgang!",
        "Fantastisch werk! Je kunt trots zijn!",
        "Wauw, dat heb je heel goed gedaan!",
        "Super! Je bent echt aan het groeien!",
        "Uitstekend! Je zet grote stappen vooruit!",
        "Dat is knap gedaan! Je bent op de goede weg!",
        "Prachtig! Je bent een doorzetter!",
        "Top werk! Ga zo door!",
        "Je bent een kanjer! Mooi gedaan!",
        "Heel goed! Je kunt dit!",
        "Wat ben jij goed bezig! Indrukwekkend!",
        "Prima gedaan! Je verdient een compliment!",
        "Je bent fantastisch! Dat heb je super opgelost!",
        "Bravo! Je hebt het helemaal zelf gedaan!",
        "Wat een prestatie! Je bent geweldig!",
        "Goed bezig! Zo kom je steeds verder!",
        "Dat was perfect! Je bent een topper!",
        "Fenomenaal! Je bent een echte held!"
    };
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAndNavigate);
        
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
    
    public void ShowSuccessPopup(string completedZorgMomentName)
    {
        if (popupPanel == null || messageText == null)
            return;
            
        trajectSceneName = "traject" + TrajectManager.Instance.trajectNumber;
        
        // Get a random encouraging message
        string randomMessage = encouragingMessages[Random.Range(0, encouragingMessages.Count)];
        
        // Set initial message
        // messageText.text = $"{randomMessage}\nJe hebt '{completedZorgMomentName}' succesvol voltooid!";
        
        // Find and display next zorgmoment name
        DisplayNextZorgMomentInfo(completedZorgMomentName, randomMessage);
        
        // Show the popup
        popupPanel.SetActive(true);
        
        // Auto close after the set time
        StopAllCoroutines();
        StartCoroutine(AutoClosePopup());
    }
    
    private async Task DisplayNextZorgMomentInfo(string completedZorgMomentName, string randomMessage)
    {
        if (messageText == null)
            return;
            
        int nextIndex = TrajectManager.Instance.GetNextEnabledIndex();
        Debug.Log($"Next enabled index: {nextIndex}");
        
        if (nextIndex >= 0 && nextIndex < TrajectManager.Instance.zorgMomentIds.Count)
        {
            int nextZorgMomentId = TrajectManager.Instance.zorgMomentIds[nextIndex];
            Debug.Log($"Next zorgmoment ID: {nextZorgMomentId}");
            IWebRequestResponse webRequestResponse = await userApiClient.LoadZorgMomentData(nextZorgMomentId);

            switch (webRequestResponse)
            {
                case WebRequestData<string> dataResponse:
                    currentZorgMoment = JsonUtility.FromJson<ZorgMoment>(dataResponse.Data);
                    messageText.text = $"{randomMessage}\nJe hebt '{completedZorgMomentName}' succesvol voltooid!\nVolgende: {currentZorgMoment.naam}";
                    break;
                case WebRequestError errorResponse:
                    Debug.LogError($"API error: {errorResponse.ErrorMessage}");
                    messageText.text = $"{randomMessage}\nJe hebt '{completedZorgMomentName}' succesvol voltooid!\nVolgende: Onbekend zorgmoment";
                    break;
                default:
                    Debug.LogError("Unexpected response type: " + webRequestResponse.GetType());
                    messageText.text = $"{randomMessage}\nJe hebt '{completedZorgMomentName}' succesvol voltooid!\nVolgende: Onbekend zorgmoment";
                    break;
            }
        }
        else
        {
            // Update the message text with "all completed" info
            messageText.text = $"{randomMessage}\nJe hebt '{completedZorgMomentName}' succesvol voltooid!\nJe hebt alle zorgmomenten voltooid!";
        }
    }    
    
    private IEnumerator AutoClosePopup()
    {
        yield return new WaitForSeconds(autoCloseTime);
        CloseAndNavigate();
    }
    
    public void CloseAndNavigate()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
            
        // Navigate to trajectory scene
        if (!string.IsNullOrEmpty(trajectSceneName))
            SceneManager.LoadScene(trajectSceneName);
    }
    
    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseAndNavigate);
    }
}