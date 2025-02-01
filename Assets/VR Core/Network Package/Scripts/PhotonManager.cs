using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("Photon Settings")]
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private string nextGameScene;

    [SerializeField] List<GameObject> CacheRoomList = new List<GameObject>();
    private Dictionary<string, RoomInfo> cachedRoomList;

    [Header("UI Room")]
    [SerializeField] GameObject roomUIPrefab;
    [SerializeField] Transform parentPrefab;
    [SerializeField] Sprite lockedScreen;
    [SerializeField] Sprite unLockedScreen;

    [Header("After Join Room")]
    [SerializeField] GameObject mainPanel;

    void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        Debug.Log("Connecting to Photon Network...");
        ConnectToPhoton();
    }

    private void Update()
    {
        //DebugAllRoomsInLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        HashSet<string> existingRooms = new HashSet<string>(cachedRoomList.Keys);

        foreach (RoomInfo info in roomList)
        {
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
                continue;
            }

            cachedRoomList[info.Name] = info;
            existingRooms.Remove(info.Name);
        }

        // Remove UI for rooms that no longer exist
        CacheRoomList.RemoveAll(x =>
        {
            bool shouldRemove = existingRooms.Contains(x.GetComponent<RoomPrefabInitialized>().RoomName.text);
            if (shouldRemove) Destroy(x);
            return shouldRemove;
        });

        InstanceRoomList();
    }


    void InstanceRoomList()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = CacheRoomList.Find(x => x.GetComponent<RoomPrefabInitialized>().RoomName.text == info.Name);

            if (entry == null) // If room UI doesn't exist, create a new one
            {
                entry = Instantiate(roomUIPrefab, parentPrefab);
                CacheRoomList.Add(entry);
            }

            Sprite lockStatus = info.CustomProperties.ContainsKey("pwd") && !string.IsNullOrEmpty(info.CustomProperties["pwd"].ToString())
                ? lockedScreen
                : unLockedScreen;

            entry.GetComponent<Button>().onClick.RemoveAllListeners();
            entry.GetComponent<Button>().onClick.AddListener(() => AttemptToJoinRoom(info.Name, entry.GetComponent<RoomPrefabInitialized>()));

            entry.GetComponent<RoomPrefabInitialized>().Initialize(info.Name, $"{info.PlayerCount}/{info.MaxPlayers}", lockStatus);
            entry.GetComponent<Button>().interactable = info.PlayerCount < info.MaxPlayers;
        }

        Debug.Log("Updated Room List UI");
    }


    public void DebugAllRoomsInLobby()
    {
        if (cachedRoomList.Count == 0)
        {
            Debug.Log("No rooms available in the lobby.");
            return;
        }

        Debug.Log("Available Rooms in Lobby:");
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            Debug.Log($"Room Name: {room.Name}, Players: {room.PlayerCount}/{room.MaxPlayers}, " +
                      $"Is Open: {room.IsOpen}, Is Visible: {room.IsVisible}");
        }
    }


    public void CreateRoom(string roomName, byte maxPlayers, string password)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogError("Room name cannot be empty.");
            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        if (!string.IsNullOrEmpty(password))
        {
            roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "pwd", password }
        };
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "pwd" };
        }

        PhotonNetwork.CreateRoom(roomName, roomOptions);
        Debug.Log($"Attempting to create room: {roomName}");
        //PhotonNetwork.LoadLevel(nextGameScene);
        mainPanel.SetActive(false);
    }


    public void AttemptToJoinRoom(string roomName, RoomPrefabInitialized roomPrefab)
    {
        if (cachedRoomList.TryGetValue(roomName, out RoomInfo roomInfo))
        {
            // Check if the room has a password
            if (roomInfo.CustomProperties.ContainsKey("pwd") &&
                !string.IsNullOrEmpty(roomInfo.CustomProperties["pwd"].ToString()))
            {
                Debug.Log($"Room '{roomName}' requires a password. Please provide the password to join.");
                roomPrefab.PasswordPanel.SetActive(true);
                roomPrefab.PasswordPanel.GetComponentInChildren<Button>().onClick.
                    AddListener(()=> AttemptToJoinRoomWithPassword(roomName, roomPrefab.PasswordPanel.GetComponentInChildren<TMP_InputField>().text));
            }
            else
            {
                // Room doesn't have a password, attempt to join
                PhotonNetwork.JoinRoom(roomName);
                Debug.Log($"Joining room: {roomName}");
                mainPanel.SetActive(false);
                //PhotonNetwork.LoadLevel(nextGameScene);
                mainPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError($"Room '{roomName}' not found in the cached room list.");
        }
    }
    public void AttemptToJoinRoomWithPassword(string roomName, string password)
    {
        if (cachedRoomList.TryGetValue(roomName, out RoomInfo roomInfo))
        {
            string roomPassword = roomInfo.CustomProperties["pwd"].ToString();
            if (roomPassword == password)
            {
                PhotonNetwork.JoinRoom(roomName);
                Debug.Log($"Joining room: {roomName}");
            }
            else
            {
                Debug.LogError($"Incorrect password for room: {roomName}");
                // Reset input field and show error UI
                TMP_InputField inputField = roomUIPrefab.GetComponent<RoomPrefabInitialized>().PasswordPanel.GetComponentInChildren<TMP_InputField>();
                inputField.textComponent.text = "";
                inputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Incorrect Password!";
            }
        }
        else
        {
            Debug.LogError($"Room '{roomName}' not found in the cached room list.");
        }
    }



    private void ConnectToPhoton()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Photon Master Server.");
        JoinLobby();
    }

    private void JoinLobby()
    {
        Debug.Log("Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby successfully.");
        ExecutePlatformMethod();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon. Reason: {cause}");
    }

    public void DisconnectFromPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("Disconnecting from Photon...");
        }
    }

    public void DebugClientsInLobby()
    {
        if (PhotonNetwork.InLobby)
        {
            Debug.Log("Clients in the same lobby:");
            foreach (var player in PhotonNetwork.PlayerList)
            {
                Debug.Log($"Player: {player.NickName}, UserId: {player.UserId}");
            }
        }
        else
        {
            Debug.LogWarning("Not currently in a lobby. Cannot list clients.");
        }
    }


    private void ExecutePlatformMethod()
    {
        mainPanel.SetActive(false);
        CreateRoom(SystemInfo.deviceUniqueIdentifier, 2, "");
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("ExecutePlatformMethod: "+ SystemInfo.deviceUniqueIdentifier);

        }
        else
        {
            Debug.Log("ExecutePlatformMethod: "+SystemInfo.deviceUniqueIdentifier);
            Debug.Log("Application.platform: " + Application.platform.ToString());

        }
    }
}