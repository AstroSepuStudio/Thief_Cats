using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] TMP_InputField _sensitivityIF;
    [SerializeField] TMP_InputField _fovIF;

    public static float CameraSensitivityMultiplier { get; private set; } = 10f;
    public static float CameraFOV { get; private set; } = 60f;

    public static UnityEvent<float> OnCamSensMultiplierChanged = new();
    public static UnityEvent<float> OnCameraFOVChanged = new();

    void OnSensitivityChanged(string value) => SetCameraSensMultiplier(float.Parse(value));
    void OnFOVChanged(string value) => SetCameraFOV(float.Parse(value));

    string _settingsFilePath;

    private void Start()
    {
        _settingsFilePath = Path.Combine(Application.persistentDataPath, "PlayerSettings.json");
        LoadSettings();

        _sensitivityIF.text = CameraSensitivityMultiplier.ToString();
        _fovIF.text = CameraFOV.ToString();

        _sensitivityIF.onValueChanged.AddListener(OnSensitivityChanged);
        _fovIF.onValueChanged.AddListener(OnFOVChanged);

        _sensitivityIF.onValueChanged.AddListener(SaveSettings);
        _fovIF.onValueChanged.AddListener(SaveSettings);
    }

    public static void SetCameraSensMultiplier(float value)
    {
        CameraSensitivityMultiplier = value;
        OnCamSensMultiplierChanged?.Invoke(value);
    }

    public static void SetCameraFOV(float value)
    {
        Mathf.Clamp(value, 30, 120);
        CameraFOV = value;

        OnCameraFOVChanged?.Invoke(value);
    }

    private void LoadSettings()
    {
        if (File.Exists(_settingsFilePath))
        {
            string json = File.ReadAllText(_settingsFilePath);
            PlayerSettings settings = JsonUtility.FromJson<PlayerSettings>(json);
            SetCameraSensMultiplier(settings.CamSensMult);
            SetCameraFOV(settings.CamFOV);
        }
        else
        {
            SaveSettings();
        }
    }

    private void SaveSettings(string value = "")
    {
        PlayerSettings settings = new(CameraSensitivityMultiplier, CameraFOV);
        string json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(_settingsFilePath, json);
    }
}

public class PlayerSettings
{
    public float CamSensMult;
    public float CamFOV;

    public PlayerSettings(float camSensMult, float camFOV)
    {
        CamSensMult = camSensMult;
        CamFOV = camFOV;
    }
}
