using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSceneManager : MonoBehaviour
{
    public TextMeshProUGUI checkpointText;
    private void Start()
    {
        Debug.Log($"Aangekomen bij checkpoint! ID: {RoutesManager.Instance.checkpointID}, Route: {RoutesManager.Instance.routeName}");
        checkpointText.text = $"Route: {RoutesManager.Instance.routeName}, Checkpoint {RoutesManager.Instance.checkpointID}.";
        //hier komt ook de api call groetjes pluk!
    }

    public void ReturnToRouteScene()
    {
        SceneManager.LoadScene(RoutesManager.Instance.routeName);
    }
}
