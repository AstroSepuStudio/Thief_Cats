using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class MP_PlayerData : MonoBehaviour
{
    public PlayerInput Player_Input;
    public CharacterController Character_Controller;
    public Transform _cameraTarget;
    public PhotonView Photon_View;
    [HideInInspector] public GameManager Game_Manager;
    [HideInInspector] public Camera Player_Camera;

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
            Game_Manager.SetPlayerDataToCamera(this);
            Player_Camera = Game_Manager.GetCamera();
        }
    }
}
