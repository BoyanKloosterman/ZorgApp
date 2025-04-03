using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationController : MonoBehaviour
{
    [SerializeField] private GameObject patientInformationButton;
    [SerializeField] private GameObject AddKindInformatieButton;
    [SerializeField] private GameObject patientTrajectButton;

    //[SerializeField] private GameObject patientInformationButtonImage;
    //[SerializeField] private GameObject AddKindInformatieButtonImage;
    //[SerializeField] private GameObject patientTrajectButtonImage;

    public UserApiClient userApiClient;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string role = PlayerPrefs.GetString("UserRole");
        if (role == "Arts")
        {
            patientInformationButton.SetActive(true);
            AddKindInformatieButton.SetActive(false);
        }
        else if (role == "Patient")
        {
            patientInformationButton.SetActive(false);
            AddKindInformatieButton.SetActive(false);
        }
        else
        {
            patientInformationButton.SetActive(false);
            AddKindInformatieButton.SetActive(true);
        }

        if (role != "Patient")
        {
            patientTrajectButton.SetActive(false);
        }
    }

    public void Home()
    {
        LoadScene("DashboardScene");
    }

    public void Account()
    {
        LoadScene("AvatarEditScene");
    }

    public void Notities()
    {
        LoadScene("NoteScene");
    }

    public void Meldingen()
    {
        LoadScene("NotificatieScene");
    }

    public void PatientInformation()
    {
        LoadScene("DossierPatient");
    }

    public void AddKindInformatie() {
        LoadScene("PatientInformatie");    
    }

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public async void GetZorgMomentenNumberToLoadScene()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.LoadZorgMomenten();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                var zorgMomentIds = JsonHelper.ParseJsonArray<int>(dataResponse.Data);
                SceneManager.LoadScene("Traject" + zorgMomentIds.Count);
                break;
            case WebRequestError errorResponse:
                Debug.LogError($"API error: {errorResponse.ErrorMessage}");
                break;
        }
    }
}
