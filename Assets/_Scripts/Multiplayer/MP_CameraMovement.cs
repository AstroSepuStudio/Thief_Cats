using UnityEngine;

public class MP_CameraMovement : MonoBehaviour
{
    [Header("References")]
    public MP_PlayerData _pData;

    [Header("Values")]
    [SerializeField] float _controllerSensitivity = 200;
    [SerializeField] float _mouseSensitivity = 50;

    float _xRotation = 0f;
    float _yRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (_pData == null) return;

        Vector2 lookInput = _pData.Player_Input.actions["ControllerLook"].ReadValue<Vector2>();
        if (Mathf.Approximately(lookInput.magnitude, 0))
        {
            lookInput = _pData.Player_Input.actions["MouseLook"].ReadValue<Vector2>();
            _yRotation += lookInput.x * _mouseSensitivity * Time.deltaTime;
            _xRotation -= lookInput.y * _mouseSensitivity * Time.deltaTime;
        }
        else
        {
            _yRotation += lookInput.x * _controllerSensitivity * Time.deltaTime;
            _xRotation -= lookInput.y * _controllerSensitivity * Time.deltaTime;
        }

        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
    }

    private void LateUpdate()
    {
        if (_pData == null) return;

        transform.SetPositionAndRotation(_pData._cameraTarget.position, Quaternion.Euler(_xRotation, _yRotation, 0f));
        _pData.transform.rotation = Quaternion.Euler(0f, _yRotation, 0f);
        _pData._cameraTarget.rotation = transform.rotation;
    }
}
