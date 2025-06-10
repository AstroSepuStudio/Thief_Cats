using Photon.Pun;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public MP_PlayerData _pData;
    [SerializeField] float maxHP = 100f;
    [SerializeField] float currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    [PunRPC]
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        _pData.IsDead = true;
        _pData.Player_SpectatorMode.OnEnterSpectator();
        _pData.PullVFX.SetActive(false);

        if (!_pData.Photon_View.IsMine)
            _pData.Flashlight.SetActive(false);

        GameManager.Instance.PlayerDied();
    }
}
