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
        Debug.Log($"{_pData.Photon_View.Owner.NickName} died.");
    }
}
