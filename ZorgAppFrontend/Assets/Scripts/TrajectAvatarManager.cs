using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class TrajectAvatarManager : MonoBehaviour
{
    public GameObject avatar;
    private CanvasGroup canvasGroup;  
    
    public void Start()
    {
        avatar = GameObject.FindWithTag("Avatar");
        canvasGroup = GameObject.Find("CanvasTraject").GetComponent<CanvasGroup>();
    }
    public async Task MoveAvatarTo(int index, float duration)
    {
        SetGameObjectInteractable(false);

        if (avatar != null)
        {
            
            Debug.Log("Avatar beweegt naar " + TrajectManager.Instance.positions[index].transform.position);
            Tweener tweener = avatar.transform.DOMove(TrajectManager.Instance.positions[index].transform.position, duration);
            while (tweener.IsActive() && !tweener.IsComplete())
            {
                await Task.Yield();
            }
        }

        SetGameObjectInteractable(true);
    }

    
    private void SetGameObjectInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }
}
