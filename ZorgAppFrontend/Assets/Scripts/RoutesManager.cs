using UnityEngine;
using UnityEngine.SceneManagement;

public class RoutesManager : MonoBehaviour
{
    public static RoutesManager Instance;

    public int checkpointID;
    public string routeName;

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

    public void LoadCheckpointScene(int id, string route)
    {
        checkpointID = id;
        routeName = route;
        SceneManager.LoadScene("Checkpoint");
    }
    public void GoToNoteScene()
    {
        SceneManager.LoadScene("NoteScene");
    }

}