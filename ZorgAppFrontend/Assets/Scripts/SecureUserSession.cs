using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

public class SecureUserSession : MonoBehaviour
{
    private static SecureUserSession _instance;
    
    public static SecureUserSession Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SecureUserSession>();
                
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SecureUserSession");
                    _instance = singletonObject.AddComponent<SecureUserSession>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }
    
    // Beveiligingssleutel - Ideaal zou je deze ergens anders genereren/opslaan
    private string _encryptionKey;
    
    // Gebruik geheugen voor tijdelijk gebruik, niet direct toegankelijk van buiten
    private string _token;
    private User _currentUser;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Genereer een unieke sleutel gebaseerd op apparaat-identifiers
        _encryptionKey = SystemInfo.deviceUniqueIdentifier + SystemInfo.deviceName;
    }
    
    public void SetToken(string token)
    {
        _token = token;
        
        // Voor extreem kortdurend gebruik kun je deze in het geheugen houden
        // Voor langdurig gebruik moeten we deze versleuteld opslaan met PlayerPrefs
        if (!string.IsNullOrEmpty(token))
        {
            string encryptedToken = EncryptString(token);
            PlayerPrefs.SetString("EncryptedToken", encryptedToken);
            PlayerPrefs.Save();
        }
    }
    
    public string GetToken()
    {
        // Als we de token in geheugen hebben, gebruik die
        if (!string.IsNullOrEmpty(_token))
        {
            return _token;
        }
        
        // Anders probeer uit opslag te halen
        if (PlayerPrefs.HasKey("EncryptedToken"))
        {
            string encryptedToken = PlayerPrefs.GetString("EncryptedToken");
            return DecryptString(encryptedToken);
        }
        
        return null;
    }
    
    public bool IsLoggedIn
    {
        get { return !string.IsNullOrEmpty(GetToken()); }
    }
    
    public void SetCurrentUser(User user)
    {
        _currentUser = user;
    }
    
    public User GetCurrentUser()
    {
        return _currentUser;
    }
    
    public void Logout()
    {
        _token = null;
        _currentUser = null;
        
        // Verwijder opgeslagen tokens
        PlayerPrefs.DeleteKey("EncryptedToken");
        PlayerPrefs.Save();
    }
    
    // Eenvoudige versleuteling - voor productie zou je een sterkere methode willen gebruiken
    private string EncryptString(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
        
        // Zorg ervoor dat de sleutel de juiste lengte heeft voor AES
        using (SHA256 sha = SHA256.Create())
        {
            keyBytes = sha.ComputeHash(keyBytes);
        }
        
        // Voer een eenvoudige XOR-operatie uit voor versleuteling
        for (int i = 0; i < inputBytes.Length; i++)
        {
            inputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        // Converteer naar Base64 voor opslag
        return Convert.ToBase64String(inputBytes);
    }
    
    private string DecryptString(string input)
    {
        try
        {
            byte[] inputBytes = Convert.FromBase64String(input);
            byte[] keyBytes = Encoding.UTF8.GetBytes(_encryptionKey);
            
            // Zorg ervoor dat de sleutel de juiste lengte heeft voor AES
            using (SHA256 sha = SHA256.Create())
            {
                keyBytes = sha.ComputeHash(keyBytes);
            }
            
            // XOR-ontsleuteling
            for (int i = 0; i < inputBytes.Length; i++)
            {
                inputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            
            return Encoding.UTF8.GetString(inputBytes);
        }
        catch
        {
            return null;
        }
    }
}