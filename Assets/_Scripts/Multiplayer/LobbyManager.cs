using Photon.Pun;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] MM_CanvasManager _canvasManager;
    [SerializeField] GameObject _roomWindow;

    [Header("UI Elements")]
    [SerializeField] TMP_InputField _createRoomNameInput;
    [SerializeField] TMP_InputField _joinRoomNameInput;
    [SerializeField] TMP_InputField _playerNameInput;
    [SerializeField] TMP_Text _roomInfoText;
    [SerializeField] TMP_Text _playerListText;
    [SerializeField] Button _startGameButton;

    private void Start()
    {
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
            roomName = "Room_" + Random.Range(1000, 9999);
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
        if (PhotonNetwork.IsMasterClient)
        {
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
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created");
        _canvasManager.OpenWindow(_roomWindow);
    }

    public override void OnJoinedRoom()
    {
        UpdateLobbyUI();

        _startGameButton.interactable = PhotonNetwork.IsMasterClient;
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

        _startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        _startGameButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }
}
