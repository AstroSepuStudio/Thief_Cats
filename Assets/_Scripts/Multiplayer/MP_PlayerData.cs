using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

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
    [HideInInspector] public Camera Player_Camera;

    public bool IsDead = false;

    private void Awake()
    {
        if (!Photon_View.IsMine)
        {
            //Character_Controller.enabled = false;
            Player_Input.enabled = false;
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
    }
}
