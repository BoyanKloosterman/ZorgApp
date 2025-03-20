using UnityEngine;
using UnityEngine.UI;

public class CheckpointButton : MonoBehaviour
{
	public int checkpointID;
	public string routeName;

	private Button button;

	private void Start()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(() => RoutesManager.Instance.LoadCheckpointScene(checkpointID, routeName));
	}
}
