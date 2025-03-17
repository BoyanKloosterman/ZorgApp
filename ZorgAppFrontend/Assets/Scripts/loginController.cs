using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class loginController : MonoBehaviour
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
        Debug.Log("test");
        user.email = emailInput.text;
        user.password = passwordInput.text;

        IWebRequestReponse webRequestResponse = await userApiClient.Login(user);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                Debug.Log("login succes");
                SecureUserSession.Instance.SetCurrentUser(new User
                {
                    email = user.email
                });

                string responseData = dataResponse.Data;
                string token = JsonHelper.ExtractToken(responseData); 
                SecureUserSession.Instance.SetToken(token);
                break;
            case WebRequestError errorResponse:
                Debug.Log("error");
                ShowErrorPopup("Geen account gevonden met deze gegevens!");
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
}
