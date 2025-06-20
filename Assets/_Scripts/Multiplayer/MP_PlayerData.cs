using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using System;
using TMPro;

public class MP_PlayerData : MonoBehaviour
{
    public enum Team { None, Thief, Defensor}
    public int PlayerID;
    public TextMeshProUGUI ID_DIsplayer;
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
    public GameObject Glass;

    [SerializeField] MP_PlayerCanvas _playerCanvas;
    [HideInInspector] public Camera Player_Camera;

    public bool IsDead = false;

    private void Awake()
    {
        PullVFX.SetActive(false);
        if (!Photon_View.IsMine)
        {
            //Character_Controller.enabled = false;
            Player_Input.enabled = false;
            _playerCanvas.gameObject.SetActive(false);
            GameManager.Instance.OnGameEnd.AddListener(OnGameEnd);
        }
        else
        {
            _playerCanvas.gameObject.SetActive(true);
            Player_Input.actions["Flashlight"].started += SwitchFlashlightState;
            Glass.SetActive(false);
            PlayerModel.SetActive(false);
        }
    }

    private void SwitchFlashlightState(InputAction.CallbackContext context)
    {
        Photon_View.RPC("RPC_SwitchFlashlight", RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_SwitchFlashlight()
    {
        Flashlight.SetActive(!Flashlight.activeInHierarchy);
    }

    private void OnGameEnd(string arg0)
    {
        Player_Movement.enabled = false;
        Player_SpectatorMode.enabled = false;
        _playerCanvas.Resume();
        _playerCanvas.Disable();

        if (Photon_View.IsMine)
        {
            Player_Input.actions["Flashlight"].started -= SwitchFlashlightState;
        }
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

        GameManager.Instance.AddPlayer(this);
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

    public void SetCCSize(float height, float center)
    {
        Character_Controller.height = height;
        Character_Controller.center = new (0, center, 0);

        PlayerModel.transform.localScale = new(1, height / 2, 1);
        Glass.transform.localPosition = new(Glass.transform.localPosition.x, center - 1, Glass.transform.localPosition.z);
    }

    [PunRPC]
    public void RPC_SetPlayerID(int id)
    {
        PlayerID = id;

        if (Photon_View.IsMine)
            ID_DIsplayer.text = id.ToString();
    }

    [PunRPC]
    public void RPC_SetCameraTargetRotation(Quaternion rotation)
    {
        _cameraTarget.rotation = rotation;
    }
}
