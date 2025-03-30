using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    public User user;
    public UserApiClient userApiClient;
    public InputField emailInput;
    public InputField passwordInput;
    public Button loginButton;

    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;

    void Start()
    {
        loginButton.onClick.AddListener(PerformLogin);
        if (ErrorPopup != null)
        {
            ErrorPopup.SetActive(false);
        }
        if (popupCloseButton != null)
        {
            popupCloseButton.onClick.AddListener(OnPopupCloseButtonClick);
        }
    }

    public async void PerformLogin()
    {
        user.email = emailInput.text;
        user.password = passwordInput.text;

        IWebRequestResponse webRequestResponse = await userApiClient.Login(user);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                Debug.Log("login success");
                Debug.Log("Token opgeslagen in sessie: " + SecureUserSession.Instance.GetToken());
                GetCurrentUserRole();
                //tijdelijk forcen naar traject13 scene.
                SceneManager.LoadScene("Traject13");
                break;
            case WebRequestError errorResponse:
                Debug.Log("error");
                ShowErrorPopup("Geen account gevonden met deze gegevens!");
                break;

            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    private async void GetCurrentUserRole()
    {
        IWebRequestResponse webRequestResponse = await userApiClient.GetCurretnUserRole();

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                Debug.Log("Role: " + dataResponse.Data);
                var roleObject = JsonUtility.FromJson<User>(dataResponse.Data);
                string roleName = roleObject.role;

                // Store just the role name
                PlayerPrefs.SetString("UserRole", roleName);
                Debug.Log("Extracted role: " + roleName);
                break;
            case WebRequestError errorResponse:
                Debug.Log("error");
                break;
            default:
                throw new NotImplementedException("No implementation for webRequestResponse of class: " + webRequestResponse.GetType());
        }
    }

    private void ShowErrorPopup(string message)
    {
        if (ErrorPopup != null)
        {
            if (popupMessageText != null)
            {
                popupMessageText.text = message;
            }

            ErrorPopup.SetActive(true);
        }
    }
    private void OnPopupCloseButtonClick()
    {
        ErrorPopup.SetActive(false);
    }

    [Serializable]
    public class Token
    {
        public string accessToken;
    }
}