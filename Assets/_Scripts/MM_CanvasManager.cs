using System.Collections.Generic;
using UnityEngine;

public class MM_CanvasManager : MonoBehaviour
{
    Stack<GameObject> _windowsOpened = new ();

    public void OpenWindow(GameObject window)
    {
        _windowsOpened.Push(window);
        window.SetActive(true);
    }

    public void CloseTopWindow()
    {
        _windowsOpened.Pop().SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
