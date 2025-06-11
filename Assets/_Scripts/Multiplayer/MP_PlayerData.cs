using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using System;

public class MP_PlayerData : MonoBehaviour
{
    public enum Team { None, Thief, Defensor}
    public Team PlayerTeam;
    public PlayerInput Player_Input;
    public CharacterController Character_Controller;
    public Transform _cameraTarget;
    public PhotonView Photon_View;
    public GameObject PlayerModel;
    public MP_PlayerMovement Player_Movement;
    public MP_SpectatorMode Player_SpectatorMode;
    public MP_PlayerInteraction Player_Interaction;
    public GameObject PullVFX;
    public GameObject Flashlight;
    [SerializeField] GameObject[] DeactivateForClientOnPlay;
    [HideInInspector] public Camera Player_Camera;

    public bool IsDead = false;

    private void Awake()
    {
        PullVFX.SetActive(false);
        if (!Photon_View.IsMine)
        {
            //Character_Controller.enabled = false;
            Player_Input.enabled = false;

            GameManager.Instance.OnGameEnd.AddListener(OnGameEnd);
        }
        else
        {
            foreach (var obj in DeactivateForClientOnPlay)
            {
                obj.SetActive(false);
            }
        }
    }

    private void OnGameEnd(string arg0)
    {
        Player_Movement.enabled = false;
        Player_SpectatorMode.enabled = false;
    }

    public void SetUp()
    {
        if (Photon_View.IsMine)
        {
            GameManager.Instance.SetPlayerDataToCamera(this);
            Player_Camera = GameManager.Instance.GetCamera();
        }
    }

    [PunRPC]
    public void SetTeam(Team team)
    {
        PlayerTeam = team;

        if (team == Team.Thief)
            GameManager.Instance.NewThief();
    }

    [PunRPC] 
    public void RPC_EnablePullVFX()
    {
        PullVFX.SetActive(true);
    }

    [PunRPC]
    public void RPC_DisablePullVFX()
    {
        PullVFX.SetActive(false);
    }

    [PunRPC]
    public void RPC_SetPullPoint(Vector3 position)
    {
        if (Player_Interaction != null)
            Player_Interaction.SetPullPointPosition(position);
    }
}
