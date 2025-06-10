using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_SpectatorMode : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PhotonView _photonView;
    [SerializeField] MP_PlayerData _pData;

    [Header("Values")]
    [SerializeField] int _waterLayer;
    [SerializeField] float _jumpForce;
    [SerializeField] float _walkSpeed;
    [SerializeField] float _sprintSpeed;

    [Header("Debug")]
    [SerializeField] float _currentSpeed;
    [SerializeField] Vector2 _inputDir;
    [SerializeField] Vector3 _movVector = new();
    [SerializeField] float _yVelocity;

    public void OnEnterSpectator()
    {
        _pData.PlayerModel.SetActive(false);
        gameObject.layer = _waterLayer;
        if (!_photonView.IsMine)
        {
            return;
        }

        this.enabled = true;
        _pData.Player_Movement.enabled = false;
        _pData.Player_Interaction.enabled = false;

        _currentSpeed = _walkSpeed;

        _pData.Player_Input.actions["Sprint"].started += StartSprint;
        _pData.Player_Input.actions["Sprint"].canceled += StopSprint;
        _pData.Player_Input.actions["Crouch"].started += StartCrouch;
        _pData.Player_Input.actions["Crouch"].canceled += StopCrouch;
        _pData.Player_Input.actions["Jump"].started += StartJump;
        _pData.Player_Input.actions["Jump"].canceled += StopJump;
    }

    private void OnDestroy()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        _pData.Player_Input.actions["Sprint"].started -= StartSprint;
        _pData.Player_Input.actions["Sprint"].canceled -= StopSprint;
        _pData.Player_Input.actions["Crouch"].started -= StartCrouch;
        _pData.Player_Input.actions["Crouch"].canceled -= StopCrouch;
        _pData.Player_Input.actions["Jump"].started -= StartJump;
        _pData.Player_Input.actions["Jump"].canceled -= StopJump;
    }

    #region Input
    private void StartCrouch(InputAction.CallbackContext context)
    { StartCrouch(); }

    private void StopCrouch(InputAction.CallbackContext context)
    { StopCrouch(); }

    private void StartCrouch()
    {
        _yVelocity = -_jumpForce;
    }

    private void StopCrouch()
    {
        _yVelocity = 0;
    }

    private void StartSprint(InputAction.CallbackContext context)
    {
        _currentSpeed = _sprintSpeed;
    }

    private void StopSprint(InputAction.CallbackContext context)
    {
        _currentSpeed = _walkSpeed;
    }

    private void StartJump(InputAction.CallbackContext context)
    {
        _yVelocity = _jumpForce;
    }

    private void StopJump(InputAction.CallbackContext context)
    {
        _yVelocity = 0;
    }

    bool CheckMovInput()
    {
        _inputDir = _pData.Player_Input.actions["Move"].ReadValue<Vector2>();

        if (Mathf.Approximately(_inputDir.magnitude, 0))
            return false;
        return true;
    }
    #endregion

    void Update()
    {
        if (!_photonView.IsMine)
        {
            return;
        }

        CheckMovInput();

        _movVector = _inputDir.x * transform.right + _inputDir.y * transform.forward;
        _movVector *= _currentSpeed;

        Vector3 velocity = _movVector + _yVelocity * Vector3.up;
        _pData.Character_Controller.Move(velocity * Time.deltaTime);
    }
}
