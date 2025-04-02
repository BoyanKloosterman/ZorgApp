using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class TrajectAvatarManager : MonoBehaviour
{
    public GameObject avatar;
    private CanvasGroup canvasGroup;  
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        avatar = GameObject.FindWithTag("Avatar");
        canvasGroup = GameObject.Find("Canvas").GetComponent<CanvasGroup>();  // Vervang "Canvas" door de naam van je Canvas
    }
    public async Task MoveAvatarTo(int index, float duration)
    {
        SetGameObjectInteractable(false);  // Zet het hele GameObject uit zodat er niet meer op geklikt kan worden

        if (avatar != null)
        {
            
            Debug.Log("Avatar beweegt naar " + TrajectManager.Instance.positions[index].transform.position);
            Tweener tweener = avatar.transform.DOMove(TrajectManager.Instance.positions[index].transform.position, duration);
            while (tweener.IsActive() && !tweener.IsComplete())
            {
                await Task.Yield(); // Wachten tot de animatie klaar is
            }
            //avatar.transform.DOMove(TrajectManager.Instance.positions[index].transform.position, duration);
        }

        SetGameObjectInteractable(true);  // Zet het GameObject weer aan nadat de animatie klaar is
    }

    // Zet de interactie van het hele GameObject uit
    private void SetGameObjectInteractable(bool interactable)
    {
        canvasGroup.interactable = interactable;  // Zet de interactie van de UI-elementen aan of uit
        canvasGroup.blocksRaycasts = interactable;  // Dit zorgt ervoor dat de UI-elementen niet meer klikbaar zijn
    }
}
