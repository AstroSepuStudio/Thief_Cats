using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MP_Shotgun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] ParticleSystem _shotEffect;
    [SerializeField] Image _reloadImage;

    [Header("Values")]
    [SerializeField] float _damage = 25f;
    [SerializeField] float _cooldown = 1f;

    [Header("Detection")]
    [SerializeField] float _range = 10f;
    [SerializeField] float _spreadWidth = 4f;
    [SerializeField] float _spreadHeight = 2f;
    [SerializeField] LayerMask _playerMask;

    WaitForSeconds _shootDelay = new(0.3f);
    float _lastShotTime = -Mathf.Infinity;
    Coroutine _currentShoot;

    private void Start()
    {
        if (!_pData.Photon_View.IsMine || _pData == null) return;
        _pData.Player_Input.actions["Fire"].started += Shoot;
        _reloadImage.fillAmount = 1f;
    }

    void Shoot(InputAction.CallbackContext context)
    {
        if (Time.time < _lastShotTime + _cooldown || _currentShoot != null) return;

        _currentShoot = StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        yield return _shootDelay;

        _lastShotTime = Time.time;
        _currentShoot = null;
        
        _pData.Photon_View.RPC("RPC_PlayShootEffect", RpcTarget.All);

        _reloadImage.fillAmount = 0f;
        StartCoroutine(ReloadUIRoutine());

        Vector3 origin = _pData.Player_Camera.transform.position;
        Vector3 direction = _pData.Player_Camera.transform.forward;

        Vector3 boxCenter = origin + direction * (_range / 2f);
        Vector3 boxHalfExtents = new(_spreadWidth / 2f, _spreadHeight / 2f, _range / 2f);
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

    IEnumerator ReloadUIRoutine()
    {
        _pData.Player_Movement._reloading = true;

        float elapsed = 0f;
        while (elapsed < _cooldown)
        {
            elapsed += Time.deltaTime;
            _reloadImage.fillAmount = elapsed / _cooldown;
            yield return null;
        }
        _reloadImage.fillAmount = 1f;

        _pData.Player_Movement._reloading = false;
    }

    [PunRPC]
    void RPC_PlayShootEffect()
    {
        if (_shotEffect != null)
            _shotEffect.Play();
    }
}
