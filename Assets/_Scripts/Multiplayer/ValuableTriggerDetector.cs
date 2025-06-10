using Photon.Pun;
using UnityEngine;

public class ValuableTriggerDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!other.CompareTag("Interactable")) return;

        if (other.TryGetComponent<ValuableItem>(out var item))
        {
            float value = item.GetValue();
            GameManager.Instance.Photon_View.RPC("RPC_IncreaseBalance", Photon.Pun.RpcTarget.AllBuffered, value);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!other.CompareTag("Interactable")) return;

        if (other.TryGetComponent<ValuableItem>(out var item))
        {
            float value = item.GetValue();
            GameManager.Instance.Photon_View.RPC("RPC_DecreaseBalance", Photon.Pun.RpcTarget.AllBuffered, value);
        }
    }
}
