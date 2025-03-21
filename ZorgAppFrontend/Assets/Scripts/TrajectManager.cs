using UnityEngine;
using UnityEngine.SceneManagement;

public class TrajectManager : MonoBehaviour
{
    public static TrajectManager Instance;

    public int zorgMomentID;
    public string trajectNumber;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadZorgMomentScene(int id, string number)
    {
        zorgMomentID = id;
        trajectNumber = number;
        SceneManager.LoadScene("ZorgMoment");
    }
}