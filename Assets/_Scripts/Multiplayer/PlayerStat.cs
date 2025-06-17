using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    public MP_PlayerData _pData;
    [SerializeField] float maxHP = 100f;
    [SerializeField] float currentHP;

    [SerializeField] CanvasGroup _takeDamageGroup;
    [SerializeField] CanvasGroup _youDiedGroup;

    WaitForSeconds _sleepTime = new WaitForSeconds(1f);

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
        else
        {
            if (_pData.Photon_View.IsMine)
            {
                StartCoroutine(FadeCanvasGroup(_takeDamageGroup, 0.1f, 0.7f));
            }
        }
    }

    void Die()
    {
        _pData.IsDead = true;
        _pData.Player_SpectatorMode.OnEnterSpectator();

        if (!_pData.Photon_View.IsMine)
        {
            _pData.Flashlight.SetActive(false);
        }
        else
        {
            StartCoroutine(FadeCanvasGroup(_youDiedGroup, 0.5f, 0.5f));
        }

        GameManager.Instance.PlayerDied();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float inDuration, float outDuration)
    {
        float timer = 0f;
        while (timer < inDuration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, timer / inDuration);
            yield return null;
        }
        group.alpha = 1f;

        yield return _sleepTime;

        timer = 0f;
        while (timer < outDuration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, timer / outDuration);
            yield return null;
        }
        group.alpha = 0f;
    }

}
