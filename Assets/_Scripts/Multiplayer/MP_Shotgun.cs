using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_Shotgun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] ParticleSystem _shotEffect;

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

            if (Physics.Raycast(origin, (playerHit.transform.position - origin + Vector3.up).normalized, out RaycastHit rayHit, _range))
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
            }
        }
    }

    [PunRPC]
    void RPC_PlayShootEffect()
    {
        if (_shotEffect != null)
            _shotEffect.Play();
    }
}
