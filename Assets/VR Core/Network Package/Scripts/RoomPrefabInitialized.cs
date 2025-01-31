using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPrefabInitialized : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI roomPlayersCount;
    [SerializeField] private Image lockIcon;
    [SerializeField] private GameObject passwordPanel;

    public GameObject PasswordPanel { get => passwordPanel; set => passwordPanel = value; }
    public TextMeshProUGUI RoomName { get => roomName;}

    public void Initialize(string _roomName, string _roomPlayersCount, Sprite _lockIcon)
    {
        roomName.text = _roomName;
        roomPlayersCount.text = _roomPlayersCount;
        lockIcon.sprite = _lockIcon;
    }

}
