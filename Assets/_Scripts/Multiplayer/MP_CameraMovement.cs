using Photon.Pun;
using UnityEngine;

public class MP_CameraMovement : MonoBehaviour
{
    [Header("References")]
    public MP_PlayerData _pData;

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
        if (_pData == null) return;
        _mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        _mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (_pData == null) return;
        _xRotation -= _mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        _yRotation += _mouseX;

        transform.SetPositionAndRotation(_pData._cameraTarget.position, Quaternion.Euler(_xRotation, _yRotation, 0f));
        _pData.transform.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }
}
