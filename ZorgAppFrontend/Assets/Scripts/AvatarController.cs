using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarController : MonoBehaviour
{
    public GameObject buttonPrefab; // Assign a UI Button prefab in the Inspector
    public Transform buttonContainer; // Assign a parent UI panel/container
    public List<Sprite> avatarSprites; // Assign avatars in the Inspector

    private void Start()
    {
        GenerateAvatarButtons();
    }

    void GenerateAvatarButtons()
    {
        for (int i = 0; i < avatarSprites.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.GetComponent<Image>().sprite = avatarSprites[i];

            int avatarIndex = i;
            newButton.GetComponent<Button>().onClick.AddListener(() => SelectAvatar(avatarIndex));
        }
    }

    void SelectAvatar(int index)
    {
        Debug.Log("Selected avatar: " + index);
        PlayerPrefs.SetInt("SelectedAvatar", index); // Store selection
    }


}
