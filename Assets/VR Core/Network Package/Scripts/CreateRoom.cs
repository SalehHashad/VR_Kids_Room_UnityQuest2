using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button createRoomButton;

    private int playerCount = 0;
    private PhotonManager photonManager;

    public int PlayerCount
    {
        get => playerCount;
        set
        {
            playerCount = value;
            ValidateButtonInteractability();
        }
    }

    private void Start()
    {
        photonManager = FindObjectOfType<PhotonManager>();
        if (photonManager == null)
        {
            Debug.LogError("PhotonManager not found in scene!");
        }

        createRoomButton.onClick.AddListener(OnCreateRoomClicked);
        roomNameInput.onValueChanged.AddListener(_ => ValidateButtonInteractability());
    }

    private void OnDestroy()
    {
        createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);
        roomNameInput.onValueChanged.RemoveListener(_ => ValidateButtonInteractability());
    }

    private void OnCreateRoomClicked()
    {
        if (photonManager != null)
        {
            photonManager.CreateRoom(roomNameInput.text, (byte)playerCount, passwordInput.text);
        }
    }

    private void ValidateButtonInteractability()
    {
        createRoomButton.interactable = !string.IsNullOrEmpty(roomNameInput.text) && playerCount > 0;
    }
}
