using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] PhotonView _photonView;
    [SerializeField] Camera M_Camera;
    [SerializeField] MP_CameraMovement Camera_Movement;

    [SerializeField] Transform _defenderSpawnPoint;
    [SerializeField] Transform[] _thiefSpawnPoints;

    [SerializeField] GameObject _defenderPrefab;
    [SerializeField] GameObject _thiefPrefab;

    [SerializeField] TextMeshProUGUI _totalBalanceTxt;
    [SerializeField] Button[] _leaveButtons;

    [SerializeField] Transform[] _itemSpawnpoints;
    [SerializeField] GameObject[] _itemPrefabs;

    [Header("Game")]
    [SerializeField] int _minItems;
    [SerializeField] float _gameDuration;
    [SerializeField] float _quota;
    float _totalBalance;

    public PhotonView Photon_View => _photonView;
    public float GameDuration => _gameDuration;
    [HideInInspector] public UnityEvent<string> OnGameEnd;

    List<MP_PlayerData> _connectedPlayers = new();
    HashSet<MP_PlayerData> _thievesInTruck = new();
    int _totalThieves = 0;

    public void NewThief() => _totalThieves++;

    int GetAliveThievesCount()
    {
        int aliveThieves = 0;
        foreach (var player in _connectedPlayers)
        {
            if (player.PlayerTeam != MP_PlayerData.Team.Thief) continue;
            if (player.IsDead) continue;
            aliveThieves++;
        }
        return aliveThieves;
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;
        OnGameEnd = new();
        _totalBalanceTxt.SetText($"$0/0");
    }

    private void Start()
    {
        SpawnPlayer();
        if (PhotonNetwork.IsMasterClient)
            SpawnRandomItems();
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint;
        GameObject player;
        MP_PlayerData pData;

        if (PhotonNetwork.IsMasterClient)
        {
            spawnPoint = _defenderSpawnPoint;
            player = PhotonNetwork.Instantiate(_defenderPrefab.name, spawnPoint.position, Quaternion.identity);
            pData = player.GetComponent<MP_PlayerData>();
            pData.PlayerTeam = MP_PlayerData.Team.Defensor;
            pData.Photon_View.RPC("SetTeam", RpcTarget.AllBuffered, MP_PlayerData.Team.Defensor);
        }
        else
        {
            int index = PhotonNetwork.LocalPlayer.ActorNumber % _thiefSpawnPoints.Length;
            spawnPoint = _thiefSpawnPoints[index];
            player = PhotonNetwork.Instantiate(_thiefPrefab.name, spawnPoint.position, Quaternion.identity);
            pData = player.GetComponent<MP_PlayerData>();
            pData.Photon_View.RPC("SetTeam", RpcTarget.AllBuffered, MP_PlayerData.Team.Thief);
        }

        _connectedPlayers.Add(pData);
        pData.SetUp();
    }

    private void SpawnRandomItems()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        List<Transform> shuffledSpawnPoints = _itemSpawnpoints.OrderBy(x => Random.value).ToList();

        int itemCount = Random.Range(_minItems, _itemSpawnpoints.Length + 1);

        for (int i = 0; i < itemCount; i++)
        {
            Transform spawnPoint = shuffledSpawnPoints[i];

            GameObject randomItemPrefab = _itemPrefabs[Random.Range(0, _itemPrefabs.Length)];

            PhotonNetwork.Instantiate(randomItemPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }

        Debug.Log($"Spawned {itemCount} items.");
    }

    public void SetPlayerDataToCamera(MP_PlayerData pData)
    {
        Camera_Movement._pData = pData;
    }

    public Camera GetCamera()
    {
        return M_Camera;
    }

    [PunRPC]
    public void RPC_IncreaseBalance(float value)
    {
        _totalBalance += value;
        _totalBalanceTxt.SetText($"${_totalBalance}/{_quota}");
    }

    [PunRPC]
    public void RPC_DecreaseBalance(float value)
    {
        _totalBalance -= value;
        _totalBalanceTxt.SetText($"${_totalBalance}/{_quota}");
    }

    [PunRPC]
    public void RPC_EndGame()
    {
        string result;

        if (_totalBalance >= _quota)
            result = "Thieves Win!";
        else
            result = "Defender Wins!";

        Debug.Log($"Game Over: {result}");
        OnGameEnd?.Invoke(result);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var player in _connectedPlayers)
        {
            player.Player_Movement.enabled = false;
            player.Player_SpectatorMode.enabled = false;
        }

        Camera_Movement.enabled = false;
        //ShowEndGameScreen(result);
    }

    [PunRPC]
    public void RPC_EndGame(bool defensorWon)
    {
        string result = defensorWon ? "Defender Wins!" : "Thieves Win!";

        Debug.Log($"Game Over: {result}");
        OnGameEnd?.Invoke(result);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var player in _connectedPlayers)
        {
            player.Player_Movement.enabled = false;
            player.Player_SpectatorMode.enabled = false;
        }

        Camera_Movement.enabled = false;
    }

    public void EndGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Photon_View.RPC("RPC_EndGame", RpcTarget.AllBuffered);
        }
    }

    public void EndGame(bool defensorWon)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Photon_View.RPC("RPC_EndGame", RpcTarget.AllBuffered, defensorWon);
        }
    }

    public void PlayerEntersTruck(MP_PlayerData player)
    {
        if (player.PlayerTeam != MP_PlayerData.Team.Thief) return;

        _thievesInTruck.Add(player);

        if (_thievesInTruck.Count < GetAliveThievesCount()) return;
        Debug.Log("All thieves are in the truck.");

        if (_totalBalance < _quota) return;
        Debug.Log("Quota reached. Ending game.");
        EndGame();
    }

    public void PlayerExitsTruck(MP_PlayerData player)
    {
        if (player.PlayerTeam != MP_PlayerData.Team.Thief) return;

        if (_thievesInTruck.Contains(player))
        {
            _thievesInTruck.Remove(player);
            Debug.Log($"{player.name} exited the truck. Thieves in truck: {_thievesInTruck.Count}");
        }
    }

    public void LeaveGameFromButton()
    {
        MP_PlayerData localPlayer = _connectedPlayers.FirstOrDefault(p => p.Photon_View.IsMine);
        if (localPlayer != null)
        {
            LeaveGame(localPlayer);
        }
    }

    public void LeaveGame(MP_PlayerData player)
    {
        if (_connectedPlayers.Contains(player))
        {
            _connectedPlayers.Remove(player);
            PhotonNetwork.Destroy(player.gameObject);
            Debug.Log($"{player.name} removed from connected players.");
        }

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayerDied()
    {
        float deadPlayers = 0;
        foreach (var player in _connectedPlayers)
        {
            if (player.IsDead)
                deadPlayers++;
        }

        if (deadPlayers == _totalThieves)
        {
            EndGame(true);
        }

        if (_thievesInTruck.Count < GetAliveThievesCount()) return;
        Debug.Log("All thieves are in the truck.");

        if (_totalBalance < _quota) return;
        Debug.Log("Quota reached. Ending game.");
        EndGame();
    }
}
