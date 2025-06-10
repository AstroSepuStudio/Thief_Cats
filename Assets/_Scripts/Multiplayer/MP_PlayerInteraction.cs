using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_PlayerInteraction : MonoBehaviour
{
    [SerializeField] float _interactRange = 5f;
    [SerializeField] float _pullStrength = 10f;
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] Transform _pullPoint;
    NetworkInteractable _currentTarget;

    void Start()
    {
        if (!_pData.Photon_View.IsMine)
        {
            enabled = false;
            return;
        }

        _pData.Player_Input.actions["Interact"].started += BeginInteract;
        _pData.Player_Input.actions["Interact"].canceled += EndInteract;
    }

    private void OnDestroy()
    {
        if (!_pData.Photon_View.IsMine) return;

        _pData.Player_Input.actions["Interact"].started -= BeginInteract;
        _pData.Player_Input.actions["Interact"].canceled -= EndInteract;
    }

    void BeginInteract(InputAction.CallbackContext ctx)
    {
        Ray ray = new (_pData.Player_Camera.transform.position, _pData.Player_Camera.transform.forward);

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
                        targetPV.RequestOwnership();

                    _currentTarget = hit.collider.GetComponent<NetworkInteractable>();
                    if (_currentTarget == null) return;

                    Debug.Log("Found Network Interactable component");
                    _currentTarget.StartPull(_pData.Player_Camera.transform, _interactRange, _pullStrength);
                    _pullPoint.position = _pData.Player_Camera.transform.position + _pData.Player_Camera.transform.forward * _interactRange;
                }
            }
        }
    }

    void EndInteract(InputAction.CallbackContext ctx)
    {
        if (_currentTarget != null)
        {
            _currentTarget.StopPull();
            _currentTarget = null;
        }
    }
}
