using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MM_CanvasManager : MonoBehaviour
{
    [SerializeField] LobbyManager _lobbyManager;
    [SerializeField] PlayerInput _input;

    Stack<GameObject> _windowsOpened = new();

    private void Start()
    {
        _input.actions["Cancel"].canceled += EscapePressed;
        _lobbyManager.OnGameStart.AddListener(Disable);
    }

    public void Disable()
    {
        _input.actions["Cancel"].canceled -= EscapePressed;
    }

    public void OpenWindow(GameObject window)
    {
        _windowsOpened.Push(window);
        window.SetActive(true);
    }

    public void CloseTopWindow()
    {
        if (_windowsOpened.Count > 0)
            _windowsOpened.Pop().SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void EscapePressed(InputAction.CallbackContext context)
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
            {
                _lobbyManager.LeaveRoom();
            }
            else
            {
                Debug.LogWarning($"Cannot leave room: IsConnectedAndReady={PhotonNetwork.IsConnectedAndReady}, InRoom={PhotonNetwork.InRoom}, State={PhotonNetwork.NetworkClientState}");
            }
        }
        else
        {
            CloseTopWindow();
        }
    }
}
