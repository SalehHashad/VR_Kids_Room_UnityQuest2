using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonToggleManager : MonoBehaviour
{
    [Header("Buttons to Manage")]
    [SerializeField] List<Button> buttons;

    [Header("Create Room Script")]
    [SerializeField] CreateRoom createRoom;

    private void Start()
    {
        foreach (var button in buttons)
        {
            Button currentButton = button;
            currentButton.onClick.AddListener(() => ToggleButtons(currentButton));
        }
    }

    private void ToggleButtons(Button clickedButton)
    {
        foreach (var button in buttons)
        {
            button.interactable = button != clickedButton;
        }
        if (clickedButton.name.Contains("2VS2"))
        {
            createRoom.PlayerCount = 4;
        }else if (clickedButton.name.Contains("4VS4"))
        {
            createRoom.PlayerCount = 8;
        }
    }

    private void OnDestroy()
    {
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}
