using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MP_PlayerMovement : MonoBehaviour
{
    enum MovementState { Idle, Walk, Sprint, Crouch, OnAir }

    [Header("References")]
    [SerializeField] PhotonView _photonView;
    [SerializeField] MP_PlayerData _pData;
    [SerializeField] Transform _feet;
    [SerializeField] LayerMask _ignorePlayer;

    [SerializeField] Transform _cameraTarget;
    [SerializeField] Transform _head;
    [SerializeField] Transform _crouchHeadPos;

    [Header("Values")]
    [SerializeField] float _gravityForce;
    [SerializeField] float _fallGravity;
    [SerializeField] float _groundYforce;

    [SerializeField] float _jumpForce;
    [SerializeField] float _walkSpeed;
    [SerializeField] float _sprintSpeed;
    [SerializeField] float _crouchSpeed;

    [SerializeField] float _groundRadiusDet;

    [SerializeField] float _defaultHeight;
    [SerializeField] Vector3 _defaultCenter;
    [SerializeField] float _crouchHeight;
    [SerializeField] Vector3 _crouchCenter;

    [SerializeField] private float _cameraTransitionDuration = 0.2f;
    Coroutine _cameraTransitionRoutine;

    [Header("Debug")]
    [SerializeField] MovementState _currentState;
    [SerializeField] float _currentSpeed;
    [SerializeField] Vector2 _inputDir;
    [SerializeField] Vector3 _movVector = new();
    [SerializeField] float _yVelocity;
    [SerializeField] bool _input;
    [SerializeField] bool _isGrounded;
    [SerializeField] bool _tryCrouch;
    [SerializeField] bool _trySprint;

    void Start()
    {
        if (!_photonView.IsMine)
        {
            //enabled = false;
            return;
        }

        _currentState = MovementState.Idle;
        _currentSpeed = _walkSpeed;

        _pData.Player_Input.actions["Sprint"].started += StartSprint;
        _pData.Player_Input.actions["Sprint"].canceled += StopSprint;
        _pData.Player_Input.actions["Crouch"].started += StartCrouch;
        _pData.Player_Input.actions["Crouch"].canceled += StopCrouch;
        _pData.Player_Input.actions["Jump"].started += TryJump;
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
        _pData.Player_Input.actions["Jump"].started -= TryJump;
    }

    #region Input
    private void StartCrouch(InputAction.CallbackContext context)
    { StartCrouch(); }

    private void StartCrouch()
    {
        if (_currentState == MovementState.Sprint)
        {
            return;
        }

        if (_cameraTransitionRoutine != null)
            StopCoroutine(_cameraTransitionRoutine);
        _cameraTransitionRoutine = StartCoroutine(SmoothCameraTargetMove(_crouchHeadPos.position));

        _pData.Character_Controller.height = _crouchHeight;
        _pData.Character_Controller.center = _crouchCenter;

        _currentState = MovementState.Crouch;
        _currentSpeed = _crouchSpeed;
    }

    private void StopCrouch(InputAction.CallbackContext context)
    { StopCrouch(); }

    private void StopCrouch()
    {
        _tryCrouch = false;

        if (_cameraTransitionRoutine != null)
            StopCoroutine(_cameraTransitionRoutine);
        _cameraTransitionRoutine = StartCoroutine(SmoothCameraTargetMove(_head.position));

        _pData.Character_Controller.height = _defaultHeight;
        _pData.Character_Controller.center = _defaultCenter;

        if (_currentState != MovementState.Crouch)
        {
            return;
        }

        EnterWalkState();
    }

    private void StartSprint(InputAction.CallbackContext context)
    {
        if (_currentState == MovementState.Crouch)
        {
            StopCrouch(context);
        }

        _currentState = MovementState.Sprint;
        _currentSpeed = _sprintSpeed;
    }

    private void StopSprint(InputAction.CallbackContext context)
    {
        _trySprint = false;
        if (_currentState != MovementState.Sprint)
        {
            return;
        }

        EnterWalkState();
    }

    private void TryJump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
            _yVelocity = _jumpForce / 100;
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

        _input = CheckMovInput();
        _isGrounded = Physics.CheckSphere(_feet.transform.position, _groundRadiusDet, _ignorePlayer);

        HandleGravity();
        HandleStates();

        _movVector = _inputDir.x * transform.right + _inputDir.y * transform.forward;
        _movVector *= _currentSpeed * Time.deltaTime;

        _pData.Character_Controller.Move(_yVelocity * Vector3.up);
        _pData.Character_Controller.Move(_movVector);
    }

    void HandleGravity()
    {
        if (_yVelocity < 0)
        {
            _yVelocity += _fallGravity * Time.deltaTime * Time.deltaTime;
            if (_isGrounded)
                _yVelocity = _groundYforce * Time.deltaTime * Time.deltaTime;
        }
        else
            _yVelocity += _gravityForce * Time.deltaTime * Time.deltaTime;
    }

    #region State
    void EnterWalkState()
    {
        _currentState = MovementState.Walk;
        _currentSpeed = _walkSpeed;
    }

    void EnterIdleState()
    {
        _currentState = MovementState.Idle;
        _currentSpeed = _walkSpeed;
    }
    
    void HandleStates()
    {
        if (!_isGrounded && _currentState != MovementState.OnAir)
        {
            if (_currentState == MovementState.Crouch)
            {
                StopCrouch();
                _tryCrouch = true;
            }
            else if (_currentState == MovementState.Sprint)
                _trySprint = true;

            _currentState = MovementState.OnAir;
        }
        else if (_isGrounded && _currentState == MovementState.OnAir)
        {
            if (_tryCrouch)
                StartCrouch();
            else if (_trySprint)
                _currentState = MovementState.Sprint;
            else
            {
                if (_input)
                    EnterWalkState();
                else
                    EnterIdleState();
            }
        }
        else if (_currentState == MovementState.Idle && _input)
        {
            EnterWalkState();
        }
        else if (_currentState == MovementState.Walk && !_input)
        {
            EnterIdleState();
            return;
        }
    }
    #endregion

    private IEnumerator SmoothCameraTargetMove(Vector3 targetPos)
    {
        Vector3 startPos = _cameraTarget.position;
        float elapsed = 0f;

        while (elapsed < _cameraTransitionDuration)
        {
            _cameraTarget.position = Vector3.Lerp(startPos, targetPos, elapsed / _cameraTransitionDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _cameraTarget.position = targetPos;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(_feet.transform.position, _groundRadiusDet);
    //}
}
