using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class MP_GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<MP_PlayerData> ConnectedPlayers;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Vector3 spawnPos = new (Random.Range(-5, 5), 1, Random.Range(-5, 5));
            GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);
            ConnectedPlayers.Add(player.GetComponent<MP_PlayerData>());
        }
    }
}
