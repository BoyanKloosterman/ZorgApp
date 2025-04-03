using Assets.Scripts.ApiClient.ModelApiClient;
using Assets.Scripts.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AvatarController : MonoBehaviour
{
    public GameObject buttonPrefab; // Assign a UI Button prefab in the Inspector
    public Transform buttonContainer; // Assign a parent UI panel/container
    public List<Sprite> avatarSprites; // Assign avatars in the Inspector

    public PatientApiClient patientApiClient;
    private void Start()
    {
        GenerateAvatarButtons();
    }

    void GenerateAvatarButtons()
    {
        for (int i = 0; i < avatarSprites.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.GetComponent<Image>().sprite = avatarSprites[i];

            int avatarIndex = i + 1;
            newButton.GetComponent<Button>().onClick.AddListener(() => SelectAvatar(avatarIndex));
        }
    }

    async Task SelectAvatar(int index)
    {
        IWebRequestResponse response = await patientApiClient.UpdatePatientAvatar(new PatientAvatarDto {userid = "", avatarId = index});

        if (response is WebRequestData<Patient> data)
        {
            Debug.Log("Avatar updated: " + data.Data.avatarId);
        }

        //navigate to next scene
        SceneManager.LoadScene("DashboardScene");
    }
}

