using TMPro;
using UnityEngine;

public class IG_CanvasManager : MonoBehaviour
{
    [SerializeField] GameObject _endGamePanel;
    [SerializeField] TextMeshProUGUI _endGameText;
    [SerializeField] TextMeshProUGUI _timeText;
    float _timer;

    private void Start()
    {
        GameManager.Instance.OnGameEnd.AddListener(ShowEndGameScreen);
        _timer = GameManager.Instance.GameDuration;
    }

    private void Update()
    {
        if (_timer <= 0f) return;

        _timer -= Time.deltaTime;
        int minutes = Mathf.FloorToInt(_timer / 60f);
        int seconds = Mathf.FloorToInt(_timer % 60f);
        _timeText.text = $"{minutes:00}:{seconds:00}";

        if (_timer <= 0f)
        {
            _timeText.text = "00:00";
            GameManager.Instance.EndGame(true);
        }
    }

    private void ShowEndGameScreen(string result)
    {
        if (_endGamePanel != null && _endGameText != null)
        {
            _endGamePanel.SetActive(true);
            _endGameText.text = result;
        }
    }
}
