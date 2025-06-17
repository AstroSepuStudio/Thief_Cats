using Photon.Pun;
using Photon.Realtime;
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

    [SerializeField] List<MP_PlayerData> _players;
    int _totalThieves = 0;
    int _thievesInTruck = 0;
    int _deadThieves = 0;
    int _aliveThieves => _totalThieves - _deadThieves;

    bool _gameEnded;

    [PunRPC]
    public void NewThief() => _totalThieves++;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;
        OnGameEnd = new();
        _totalBalanceTxt.SetText($"$0/{_quota}");
    }

    private void Start()
    {
        SpawnPlayer();
        if (PhotonNetwork.IsMasterClient)
            SpawnRandomItems();

        M_Camera.fieldOfView = SettingsManager.CameraFOV;
        SettingsManager.OnCameraFOVChanged.AddListener(OnCameraFOV);
    }

    void OnCameraFOV(float value) => M_Camera.fieldOfView = value;

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

    public void AddPlayer(MP_PlayerData pData)
    {
        _players.Add(pData);

        if (PhotonNetwork.IsMasterClient)
        {
            pData.Photon_View.RPC("RPC_SetPlayerID", RpcTarget.AllBuffered, _players.Count - 1);
        }
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
        string result = _totalBalance >= _quota ? "Thieves Wins!" : "Defender Win!";

        Debug.Log($"Game Over: {result}");
        OnGameEnd?.Invoke(result);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera_Movement.enabled = false;
        _gameEnded = true;
    }

    [PunRPC]
    public void RPC_EndGame(bool defensorWon)
    {
        string result = defensorWon ? "Defender Wins!" : "Thieves Win!";

        Debug.Log($"Game Over: {result}");
        OnGameEnd?.Invoke(result);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera_Movement.enabled = false;
        _gameEnded = true;
    }

    public void EndGame()
    {
        if (_gameEnded) return;

        if (PhotonNetwork.IsMasterClient)
        {
            Photon_View.RPC("RPC_EndGame", RpcTarget.AllBuffered);
        }
    }

    public void EndGame(bool defensorWon)
    {
        if (_gameEnded) return;

        if (PhotonNetwork.IsMasterClient)
        {
            Photon_View.RPC("RPC_EndGame", RpcTarget.AllBuffered, defensorWon);
        }
    }

    public void PlayerEntersTruck(MP_PlayerData player)
    {
        if (player.PlayerTeam != MP_PlayerData.Team.Thief) return;

        _thievesInTruck++;

        if (_thievesInTruck < _aliveThieves) return;
        Debug.Log("All thieves are in the truck.");

        if (_totalBalance < _quota) return;
        Debug.Log("Quota reached. Ending game.");
        EndGame();
    }

    public void PlayerExitsTruck(MP_PlayerData player)
    {
        if (player.PlayerTeam != MP_PlayerData.Team.Thief) return;
        _thievesInTruck--;
        Debug.Log("Thief exit the truck");
    }

    public void LeaveGameFromButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            EndGame(false);
        }

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _totalThieves--;

        if (_totalThieves <= 0)
        {
            EndGame(true);
        }
    }

    public void PlayerDied()
    {
        _deadThieves++;

        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log($"Thieves: {_totalThieves}, dead thieves; {_deadThieves}");

        if (_deadThieves == _totalThieves)
        {
            EndGame(true);
            return;
        }

        if (_thievesInTruck < _aliveThieves || _aliveThieves <= 0) return;
        Debug.Log("All thieves are in the truck.");

        if (_totalBalance < _quota) return;
        Debug.Log("Quota reached. Ending game.");
        EndGame();
    }

    public void EnablePullVFX(MP_PlayerData pData)
    {
        if (pData == null || pData.Photon_View == null) return;

        pData.Photon_View.RPC("RPC_EnablePullVFX", RpcTarget.AllBuffered);
    }

    public void DisablePullVFX(MP_PlayerData pData)
    {
        if (pData == null || pData.Photon_View == null) return;

        pData.Photon_View.RPC("RPC_DisablePullVFX", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void RPC_SetCCSize(int id, float height, float center)
    {
        foreach (var p in _players)
        {
            if (p.PlayerID.Equals(id))
            {
                p.SetCCSize(height, center);
                break;
            }
        }
    }
}
