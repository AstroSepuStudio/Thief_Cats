using UnityEngine;

public class SP_CameraMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SP_PlayerData _pData;
    [SerializeField] Transform _cameraTarget;

    [Header("Values")]
    [SerializeField] float _sensitivity;

    float _mouseX;
    float _mouseY;

    float _xRotation = 0f;
    float _yRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        _mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        _xRotation -= _mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        _yRotation += _mouseX;

        transform.SetPositionAndRotation(_cameraTarget.position, Quaternion.Euler(_xRotation, _yRotation, 0f));
        _pData.Player_Character.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }
}
