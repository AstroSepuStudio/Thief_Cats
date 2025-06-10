using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_PlayerInteraction : MonoBehaviour, IPunOwnershipCallbacks
{
    [SerializeField] float _interactRange = 5f;
    [SerializeField] float _pullStrength = 10f;
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] Transform _pullPoint;

    NetworkInteractable _currentTarget;
    PhotonView _pendingTargetPV;
    RaycastHit _pendingHit;

    void Start()
    {
        if (!_pData.Photon_View.IsMine)
        {
            enabled = false;
            return;
        }

        PhotonNetwork.AddCallbackTarget(this);

        _pData.Player_Input.actions["Interact"].started += BeginInteract;
        _pData.Player_Input.actions["Interact"].canceled += EndInteract;
    }

    private void OnDestroy()
    {
        if (!_pData.Photon_View.IsMine) return;

        PhotonNetwork.RemoveCallbackTarget(this);

        _pData.Player_Input.actions["Interact"].started -= BeginInteract;
        _pData.Player_Input.actions["Interact"].canceled -= EndInteract;
    }

    void BeginInteract(InputAction.CallbackContext ctx)
    {
        Ray ray = new(_pData.Player_Camera.transform.position, _pData.Player_Camera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactRange))
        {
            Debug.Log("Hit");
            if (hit.collider.CompareTag("Interactable"))
            {
                Debug.Log("Is interatable");
                if (hit.collider.TryGetComponent<PhotonView>(out var targetPV))
                {
                    Debug.Log("Found photon view");
                    if (!targetPV.IsMine)
                    {
                        _pendingTargetPV = targetPV;
                        _pendingHit = hit;
                        targetPV.RequestOwnership();
                        return;
                    }

                    StartPullInteraction(targetPV, hit);
                }
            }
        }
    }

    void StartPullInteraction(PhotonView targetPV, RaycastHit hit)
    {
        _currentTarget = hit.collider.GetComponent<NetworkInteractable>();
        if (_currentTarget == null) return;

        _currentTarget.StartPull(_pData, _pData.Player_Camera.transform, _interactRange, _pullStrength);
        _pData.Photon_View.RPC("RPC_SetPullPoint", RpcTarget.AllBuffered, hit.point);
        GameManager.Instance.EnablePullVFX(_pData);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (_pendingTargetPV != null && targetView == _pendingTargetPV)
        {
            StartPullInteraction(targetView, _pendingHit);
            _pendingTargetPV = null;
        }
    }

    void EndInteract(InputAction.CallbackContext ctx)
    {
        if (_currentTarget != null)
        {
            _currentTarget.StopPull();
            _currentTarget = null;
            GameManager.Instance.DisablePullVFX(_pData);
        }
    }

    public void SetPullPointPosition(Vector3 position)
    {
        _pullPoint.position = position;
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        _pendingTargetPV = null;
    }
}
