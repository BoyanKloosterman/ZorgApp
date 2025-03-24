using UnityEngine;

public class NavigationController : MonoBehaviour
{
    [SerializeField] private GameObject patientInformationButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //check what button are visible
        //if the user is a patient, the button patient information is not visible
        //string role = PlayerPrefs.GetString("UserRole");
        //if (role == "Arts")
        //{
        //    patientInformationButton.SetActive(false);
        //}
    }

    public void Home()
    {
        LoadScene("Traject13");
    }

    public void Account()
    {
        //LoadScene("Account");
    }

    public void Notities()
    {
        LoadScene("NoteScene");
    }

    public void Meldingen()
    {
        //LoadScene("Meldingen");
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
