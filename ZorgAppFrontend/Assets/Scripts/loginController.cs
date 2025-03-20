using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        IWebRequestReponse webRequestResponse = await userApiClient.Login(user);

        switch (webRequestResponse)
        {
            case WebRequestData<string> dataResponse:
                Debug.Log("login success");

                try
                {
                    // Parse the response to get the user id
                    var responseData = JsonUtility.FromJson<Token>(dataResponse.Data);
                    if (!string.IsNullOrEmpty(responseData.userId))
                    {
                        // Store the user ID in PlayerPrefs
                        PlayerPrefs.SetString("userId", responseData.userId);
                        PlayerPrefs.Save();
                        Debug.Log("User ID saved to PlayerPrefs: " + responseData.userId);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error parsing user ID: " + ex.Message);
                }

                // naar andere scene
                SceneManager.LoadScene("Route13");

                Debug.Log("Token opgeslagen in sessie: " + SecureUserSession.Instance.GetToken());
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

    [Serializable]
    public class Token
    {
        public string accessToken;
        public string userId;
    }



}