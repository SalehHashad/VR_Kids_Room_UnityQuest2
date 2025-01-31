using UnityEngine;
using UnityEngine.UI;

public class MaxPlayerCountButton : MonoBehaviour
{
    [SerializeField] private int maxPlayers;
    private Button button;
    private CreateRoom createRoom;

    private void Start()
    {
        button = GetComponent<Button>();
        createRoom = FindObjectOfType<CreateRoom>();

        if (createRoom == null)
        {
            Debug.LogError("CreateRoom script not found in the scene!");
            return;
        }

        button.onClick.AddListener(OnClicked);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        if (createRoom == null) return;

        createRoom.PlayerCount = maxPlayers;

        foreach (Button btn in transform.parent.GetComponentsInChildren<Button>())
        {
            btn.interactable = true;
        }
        button.interactable = false;
    }
}
