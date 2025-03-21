using UnityEngine;
using UnityEngine.UI;

public class zorgMomentButton : MonoBehaviour
{
	public int zorgMomentID;
	public string trajectNumber;

	private Button button;

	private void Start()
	{
		button = GetComponent<Button>();
		button.onClick.AddListener(() => TrajectManager.Instance.LoadZorgMomentScene(zorgMomentID, trajectNumber));
	}
}
