using UnityEngine;

public class NavigationController : MonoBehaviour
{
    [SerializeField] private GameObject patientInformationButton;
    [SerializeField] private GameObject trajectButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string role = PlayerPrefs.GetString("UserRole");
        if (role != "Arts")
        {
            patientInformationButton.SetActive(false);
        }

        if (role != "Patient")
        {
            trajectButton.SetActive(false);
        }
    }

    public void Home()
    {
        LoadScene("Traject13");
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

    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
