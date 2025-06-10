using Photon.Pun;
using UnityEngine;

public class ThiefTriggerDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!other.CompareTag("Thief")) return;

        if (other.TryGetComponent<MP_PlayerData>(out var player))
        {
            GameManager.Instance.PlayerEntersTruck(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!other.CompareTag("Thief")) return;

        if (other.TryGetComponent<MP_PlayerData>(out var player))
        {
            GameManager.Instance.PlayerExitsTruck(player);
        }
    }
}
