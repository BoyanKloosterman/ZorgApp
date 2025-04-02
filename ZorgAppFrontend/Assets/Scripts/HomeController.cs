using UnityEngine;
using UnityEngine.SceneManagement; // Add this using directive


public class HomeController : MonoBehaviour
{

    public void GoToLogin()
    {
        SceneManager.LoadScene("Login"); // Use SceneManager instead of SceneLoader
    }

    public void GoToRegister()
    {
        SceneManager.LoadScene("Register"); // Use SceneManager instead of SceneLoader
    }
}
