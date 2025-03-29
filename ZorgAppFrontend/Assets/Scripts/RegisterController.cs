using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RegisterController : MonoBehaviour
{
    public UserApiClient userApiClient;

    public InputField firstNameInput;
    public InputField lastNameInput;
    public InputField emailInput;
    public InputField passwordInput;
    public InputField repeatPasswordInput;
    public Dropdown roleDropdown;
    public Button registerButton;

    public GameObject ErrorPopup;
    public Text popupMessageText;
    public Button popupCloseButton;
    public LoginController login;

    void Start()
    {
        registerButton.onClick.AddListener(PerformRegistration);
        if (ErrorPopup != null) ErrorPopup.SetActive(false);
        if (popupCloseButton != null) popupCloseButton.onClick.AddListener(() => ErrorPopup.SetActive(false));
    }

    public async void PerformRegistration()
    {
        string errorMessage = ValidateInputs();
        if (errorMessage != null)
        {
            ShowErrorPopup(errorMessage);
            return;
        }

        User newUser = new User
        {
            voornaam = firstNameInput.text.Trim(),
            achternaam = lastNameInput.text.Trim(),
            email = emailInput.text.Trim(),
            password = passwordInput.text,
            role = roleDropdown.options[roleDropdown.value].text
        };

        IWebRequestResponse webRequestResponse = await userApiClient.Register(newUser);

        if (webRequestResponse is WebRequestData<string> successResponse)
        {
            Debug.Log("Registratie geslaagd!");
            Debug.Log(successResponse.Data);

            login = FindObjectOfType<LoginController>();

            if (login == null)
            {
                Debug.LogError("LoginController niet gevonden! Zorg ervoor dat deze in de scene zit.");
            }
            else
            {
                login.PerformLogin(true);
            }
            return;
        }
        else if (webRequestResponse is WebRequestError errorResponse)
        {
            ShowErrorPopup("Registratie mislukt: " + errorResponse.ErrorMessage);
        }
    }

    private string ValidateInputs()
    {
        List<string> feedbackMessages = new List<string>();

        if (string.IsNullOrWhiteSpace(firstNameInput.text))
            feedbackMessages.Add("Voornaam is verplicht.");

        if (string.IsNullOrWhiteSpace(lastNameInput.text))
            feedbackMessages.Add("Achternaam is verplicht.");

        if (string.IsNullOrWhiteSpace(emailInput.text) || !IsValidEmail(emailInput.text))
            feedbackMessages.Add("Voer een geldig e-mailadres in.");

        string password = passwordInput.text;

        if (password.Length < 8)
            feedbackMessages.Add("Wachtwoord moet minimaal 8 karakters lang zijn.");
        if (!Regex.IsMatch(password, "[a-z]"))
            feedbackMessages.Add("Wachtwoord moet minstens 1 kleine letter bevatten.");
        if (!Regex.IsMatch(password, "[A-Z]"))
            feedbackMessages.Add("Wachtwoord moet minstens 1 hoofdletter bevatten.");
        if (!Regex.IsMatch(password, "[0-9]"))
            feedbackMessages.Add("Wachtwoord moet minstens 1 cijfer bevatten.");
        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            feedbackMessages.Add("Wachtwoord moet minstens 1 speciaal teken bevatten (!@#$%^&* etc.).");

        if (password != repeatPasswordInput.text)
            feedbackMessages.Add("Wachtwoorden komen niet overeen.");

        return feedbackMessages.Count > 0 ? string.Join("\n", feedbackMessages) : null;
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    private void ShowErrorPopup(string message)
    {
        if (ErrorPopup != null)
        {
            if (popupMessageText != null) popupMessageText.text = message;
            ErrorPopup.SetActive(true);
        }
    }
}
