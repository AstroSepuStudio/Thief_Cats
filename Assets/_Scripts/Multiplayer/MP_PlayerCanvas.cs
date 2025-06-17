using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_PlayerCanvas : MonoBehaviour
{
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] GameObject _menuWindow;

    Stack<GameObject> _windowsOpened = new();

    private void Start()
    {
        if (!_pData.Photon_View.IsMine) return;
        _pData.Player_Input.actions["Cancel"].canceled += EscapePressed;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        if (!_pData.Photon_View.IsMine) return;
        _pData.Player_Input.actions["Cancel"].canceled -= EscapePressed;
    }

    public void Disable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _menuWindow.SetActive(false);
        _pData.Player_Input.actions["Cancel"].canceled -= EscapePressed;
    }

    void EscapePressed(InputAction.CallbackContext context)
    {
        if (_windowsOpened.Count > 0)
        {
            CloseTopWindow();
        }
        else
        {
            OpenWindow(_menuWindow);
        }
    }

    public void OpenWindow(GameObject window)
    {
        if (_windowsOpened.Contains(window)) return;

        _windowsOpened.Push(window);
        window.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseTopWindow()
    {
        if (_windowsOpened.Count > 0)
            _windowsOpened.Pop().SetActive(false);

        if (_windowsOpened.Count > 0) return;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LeaveGame()
    {
        GameManager.Instance.LeaveGameFromButton();
    }

    public void Resume()
    {
        while (_windowsOpened.Count > 0)
        {
            CloseTopWindow();
        }
    }
}
