using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_Shotgun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] ParticleSystem _shotEffect;
    [SerializeField] LineRenderer _lineRenderer;

    [Header("Values")]
    [SerializeField] float _damage = 25f;
    [SerializeField] float _cooldown = 1f;

    [Header("Detection")]
    [SerializeField] float _range = 10f;
    [SerializeField] float _spreadWidth = 4f;
    [SerializeField] float _spreadHeight = 2f;
    [SerializeField] LayerMask _playerMask;

    float _lastShotTime = -Mathf.Infinity;

    private void Start()
    {
        if (!_pData.Photon_View.IsMine || _pData == null) return;

        _pData.Player_Input.actions["Fire"].started += Shoot;
    }

    void Shoot(InputAction.CallbackContext context)
    {
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;

        _pData.Photon_View.RPC("RPC_PlayShootEffect", RpcTarget.All);

        Vector3 origin = _pData.Player_Camera.transform.position;
        Vector3 direction = _pData.Player_Camera.transform.forward;

        Vector3 boxCenter = origin + direction * (_range / 2f);
        Vector3 boxHalfExtents = new (_spreadWidth / 2f, _spreadHeight / 2f, _range / 2f);
        Quaternion orientation = Quaternion.LookRotation(direction);

        Collider[] playersHit = Physics.OverlapBox(boxCenter, boxHalfExtents, orientation, _playerMask);
        foreach (Collider playerHit in playersHit)
        {
            if (playerHit.transform.root == transform.root) continue;

            if (Physics.Raycast(origin, (playerHit.transform.position - origin).normalized, out RaycastHit rayHit, _range))
            {
                Transform playerHitRoot = playerHit.transform.root;

                if (rayHit.transform.root == playerHitRoot && playerHitRoot.TryGetComponent<PlayerStat>(out var targetStat))
                {
                    targetStat._pData.Photon_View.RPC("TakeDamage", RpcTarget.AllBuffered, _damage);
                }
                else
                {
                    Debug.Log($"PlayerStat component not found, hit {rayHit.transform.gameObject.name}");
                }

                _lineRenderer.positionCount = 2;
                _lineRenderer.SetPosition(0, origin);
                _lineRenderer.SetPosition(1, ((playerHit.transform.position - origin).normalized * _range) + origin);
            }
        }
    }

    [PunRPC]
    void RPC_PlayShootEffect()
    {
        if (_shotEffect != null)
            _shotEffect.Play();
    }

    private void OnDrawGizmosSelected()
    {
        if (_pData == null || _pData.Player_Camera == null) return;

        Vector3 origin = _pData.Player_Camera.transform.position;
        Vector3 direction = _pData.Player_Camera.transform.forward;
        Vector3 boxCenter = origin + direction * (_range / 2f);
        Vector3 boxHalfExtents = new Vector3(_spreadWidth / 2f, _spreadHeight / 2f, _range / 2f);
        Quaternion orientation = Quaternion.LookRotation(direction);

        Gizmos.color = Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
    }
}
