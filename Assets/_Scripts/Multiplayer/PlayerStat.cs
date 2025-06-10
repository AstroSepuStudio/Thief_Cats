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
        GameManager.Instance.PlayerDied();
        _pData.Player_SpectatorMode.OnEnterSpectator();
    }
}
