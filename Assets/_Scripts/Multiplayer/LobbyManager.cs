using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] MM_CanvasManager _canvasManager;
    [SerializeField] GameObject _roomWindow;
    [SerializeField] GameObject _connectingToLobby;
    [SerializeField] Image _firstLoadingCircle;
    [SerializeField] Image _secondLoadingCircle;

    [Header("UI Elements")]
    [SerializeField] TMP_InputField _createRoomNameInput;
    [SerializeField] TMP_InputField _joinRoomNameInput;
    [SerializeField] TMP_InputField _playerNameInput;
    [SerializeField] TMP_Text _roomInfoText;
    [SerializeField] TMP_Text _playerListText;
    [SerializeField] Button _startGameButton;

    public UnityEvent OnGameStart = new();
    bool _loading;

    private void Start()
    {
        _loading = true;
        _connectingToLobby.SetActive(true);
        StartCoroutine(LoadingAnimation());
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void SetPlayerName()
    {
        if (!string.IsNullOrEmpty(_playerNameInput.text))
        {
            PhotonNetwork.NickName = _playerNameInput.text;
        }
        else
        {
            PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
        }
    }

    public void TryCreateRoom()
    {
        string roomName = _createRoomNameInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room" + Random.Range(1000, 9999);
        }

        SetPlayerName();
        RoomOptions options = new();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void TryJoinRoom()
    {
        SetPlayerName();
        if (!string.IsNullOrEmpty(_joinRoomNameInput.text))
        {
            PhotonNetwork.JoinRoom(_joinRoomNameInput.text);
        }
    }

    public void TryJoinRandomRoom()
    {
        SetPlayerName();
        PhotonNetwork.JoinRandomRoom();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void StartRoomGame()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            OnGameStart?.Invoke();
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
    
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        _loading = false;
        _connectingToLobby.SetActive(false);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created");
        //_canvasManager.OpenWindow(_roomWindow);
    }

    public override void OnJoinedRoom()
    {
        UpdateLobbyUI();

        _startGameButton.interactable = false;
        _startGameButton.gameObject.SetActive(false);
        _canvasManager.OpenWindow(_roomWindow);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateLobbyUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No room found");
    }

    public override void OnLeftRoom()
    {
        _canvasManager.CloseTopWindow();
    }

    private void UpdateLobbyUI()
    {
        _roomInfoText.text = $"Room: {PhotonNetwork.CurrentRoom.Name}\nPlayers: {PhotonNetwork.CurrentRoom.PlayerCount}/4";
        _playerListText.text = "";

        foreach (var player in PhotonNetwork.PlayerList)
        {
            string role = (player.IsMasterClient) ? "Defender" : "Thief";
            _playerListText.text += $"{player.NickName} ({role})\n";
        }

        bool moraThanOne = PhotonNetwork.CurrentRoom.PlayerCount > 1;
        if (PhotonNetwork.IsMasterClient)
            _startGameButton.gameObject.SetActive(moraThanOne);
        _startGameButton.interactable = moraThanOne;
    }

    IEnumerator LoadingAnimation()
    {
        float fillDuration = 0.5f;
        _firstLoadingCircle.fillAmount = 0f;
        _secondLoadingCircle.fillAmount = 0f;

        while (_loading)
        {
            // Go from _firstLoadingCircle.fillAmount 0 to 1, then set it back to 0
            // Set _secondLoadingCircle.fillAmount to 1 and the go from 1 to 0
            // Repeat

            float timer = 0f;
            while (timer < fillDuration)
            {
                float t = timer / fillDuration;
                _firstLoadingCircle.fillAmount = t;

                timer += Time.deltaTime;
                yield return null;
            }

            _firstLoadingCircle.fillAmount = 0f;
            _secondLoadingCircle.fillAmount = 1f;

            timer = fillDuration;
            while (timer > 0)
            {
                float t = timer / fillDuration;
                _secondLoadingCircle.fillAmount = t;

                timer -= Time.deltaTime;
                yield return null;
            }

            _firstLoadingCircle.fillAmount = 0f;
            _secondLoadingCircle.fillAmount = 0f;

            yield return null;
        }
    }
}
