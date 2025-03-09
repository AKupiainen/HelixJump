using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public struct GameEndUI
{
    [SerializeField]
    [FormerlySerializedAs("GameEndUIParent")]
    private Transform _gameEndUIParent;

    [SerializeField]
    [FormerlySerializedAs("backgroundPanel")]
    private Image _backgroundPanel;

    [SerializeField]
    [FormerlySerializedAs("totalScoreValue")]
    private Text _totalScoreValue;

    [SerializeField]
    [FormerlySerializedAs("totalScoreText")]
    private Text _totalScoreText;

    [SerializeField]
    [FormerlySerializedAs("currentScoreText")]
    private Text _currentScoreText;

    [SerializeField]
    [FormerlySerializedAs("currentScoreValue")]
    private Text _currentScoreValue;

    [SerializeField]
    [FormerlySerializedAs("touchToRestartText")]
    private Text _touchToRestartText;

    [SerializeField]
    [FormerlySerializedAs("passAllLevelsInfoText")]
    private Text _passAllLevelsInfoText;

    [SerializeField]
    [FormerlySerializedAs("touchToContinue")]
    private Text _touchToContinue;

    [SerializeField]
    [FormerlySerializedAs("gameOverInfoText")]
    private Text _gameOverInfoText;

    [SerializeField]
    [FormerlySerializedAs("passedInfoText")]
    private Text _passedInfoText;

    [SerializeField]
    [FormerlySerializedAs("secondChanceCounterText")]
    private Text _secondChanceCounterText;

    [SerializeField]
    [FormerlySerializedAs("secondChanceInfoText")]
    private Text _secondChanceInfoText;

    [SerializeField]
    [FormerlySerializedAs("secondChanceWatchAdText")]
    private Text _secondChanceWatchAdText;

    [SerializeField] 
    private Animation _secondChanceAnimator;

    public Transform GameEndUIParent => _gameEndUIParent;
    public Image BackgroundPanel => _backgroundPanel;
    public Text TotalScoreValue => _totalScoreValue;
    public Text TotalScoreText => _totalScoreText;
    public Text CurrentScoreText => _currentScoreText;
    public Text CurrentScoreValue => _currentScoreValue;
    public Text TouchToRestartText => _touchToRestartText;
    public Text PassAllLevelsInfoText => _passAllLevelsInfoText;
    public Text TouchToContinue => _touchToContinue;
    public Text GameOverInfoText => _gameOverInfoText;
    public Text PassedInfoText => _passedInfoText;
    public Text SecondChanceCounterText => _secondChanceCounterText;
    public Text SecondChanceInfoText => _secondChanceInfoText;
    public Text SecondChanceWatchAdText => _secondChanceWatchAdText;
    public Animation SecondChanceAnimator => _secondChanceAnimator;
}