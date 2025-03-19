using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointSceneManager : MonoBehaviour
{
    public TextMeshProUGUI tempText;
    private void Start()
    {
        Debug.Log($"Checkpoint: {RoutesManager.Instance.checkpointID}, Route: {RoutesManager.Instance.routeName}");
        tempText.text = $"Checkpoint: {RoutesManager.Instance.checkpointID}, Route: {RoutesManager.Instance.routeName}";
        //hier komt ook de api call groetjes pluk!
    }

    public void ReturnToRouteScene()
    {
        SceneManager.LoadScene(RoutesManager.Instance.routeName);
    }
}
