using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] MP_CameraMovement Camera_Movement;

    [SerializeField] Transform _defenderSpawnPoint;
    [SerializeField] Transform[] _thiefSpawnPoints;

    [SerializeField] GameObject _defenderPrefab;
    [SerializeField] GameObject _thiefPrefab;

    List<MP_PlayerData> _connectedPlayers = new();

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint;
        GameObject player;

        if (PhotonNetwork.IsMasterClient)
        {
            spawnPoint = _defenderSpawnPoint;
            player = PhotonNetwork.Instantiate(_defenderPrefab.name, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            int index = PhotonNetwork.LocalPlayer.ActorNumber % _thiefSpawnPoints.Length;
            spawnPoint = _thiefSpawnPoints[index];
            player = PhotonNetwork.Instantiate(_thiefPrefab.name, spawnPoint.position, Quaternion.identity);
        }

        MP_PlayerData pData = player.GetComponent<MP_PlayerData>();
        _connectedPlayers.Add(pData);
        pData.Game_Manager = this;
        pData.SetUp();
    }

    public void SetPlayerDataToCamera(MP_PlayerData pData)
    {
        Camera_Movement._pData = pData;
    }
}
