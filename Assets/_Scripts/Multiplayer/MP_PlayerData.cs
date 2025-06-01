using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using System.Collections;

public class MP_PlayerData : MonoBehaviour
{
    public PlayerInput Player_Input;
    public CharacterController Character_Controller;
    public Transform _cameraTarget;
    public PhotonView _photonView;
    public GameManager Game_Manager;

    private void Awake()
    {
        if (!_photonView.IsMine)
        {
            Character_Controller.enabled = false;
            Player_Input.enabled = false;
        }
    }

    public void SetUp()
    {
        if (_photonView.IsMine)
        {
            Game_Manager.SetPlayerDataToCamera(this);
        }
    }
}
