using Mirror;
using UnityEngine;

public enum ScreenState
{
    WaitingLobby,
    PreDiceReceived,
    EveryoneRollingTime,
    EveryoneJustRolled,
    AfterRollDamageEnemy,
    AfterRollEnemyAttack,
    EveryonePickBooster
}
public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    // For simple on / offs
    [SerializeField] private Canvas _startScreen, _preDiceScreen, _diceRollingScreen, _allDiceRolledScreen;
    public Canvas HealthBarsCanvas;
    // For animations, things that move or change in size (leave canvas on here, change transforms)
    RectTransform _layoutOwnedBoosterRect, _layoutPotentialBoosterRect;

    private float _largeScaleMultiplier = 1;
    private float _smallScaleMultiplier = 0.5f;

    private ScreenState _screenState;
    public ScreenState ScreenState
    {
        get { return _screenState; }
        private set
        {
            _screenState = value;
            switch (value)
            {
                case ScreenState.WaitingLobby:
                    _startScreen.gameObject.SetActive(true);
                    _preDiceScreen.gameObject.SetActive(false);
                    _diceRollingScreen.gameObject.SetActive(false);
                    _allDiceRolledScreen.gameObject.SetActive(false);
                    HealthBarsCanvas.gameObject.SetActive(false);
                    // ... animations under here
                    HideOwnedBoosterLayout();
                    HidePotentialBoosterLayout();
                    break;
                case ScreenState.PreDiceReceived:
                    _preDiceScreen.gameObject.SetActive(true);
                    HealthBarsCanvas.gameObject.SetActive(true);
                    _startScreen.gameObject.SetActive(false);
                    _diceRollingScreen.gameObject.SetActive(false);
                    _allDiceRolledScreen.gameObject.SetActive(false);
                    // ... animations under here
                    if (TurnManager.Instance.TurnCount > 1) // is this not the first time playing?
                    {
                        ShowOwnedBoosterLayout();
                    }
                    break;
                case ScreenState.EveryoneRollingTime:
                    ShrinkOwnedBoosterLayout();
                    break;
                case ScreenState.EveryoneJustRolled:
                    break;
                case ScreenState.AfterRollDamageEnemy:
                    break;
                case ScreenState.AfterRollEnemyAttack:
                    break;
                case ScreenState.EveryonePickBooster:
                    HealthBarsCanvas.gameObject.SetActive(true);
                    _startScreen.gameObject.SetActive(false);
                    _preDiceScreen.gameObject.SetActive(false);
                    _diceRollingScreen.gameObject.SetActive(false);
                    _allDiceRolledScreen.gameObject.SetActive(false);
                    // ... animations under here
                    HideOwnedBoosterLayout();
                    ShowPotentialBoosterLayout();
                    break;
                default:
                    break;
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void StartTheUI()
    {
        _layoutOwnedBoosterRect = GameObject.FindWithTag("CardLayout").GetComponent<RectTransform>();
        _layoutPotentialBoosterRect = GameObject.FindWithTag("NewCardLayout").GetComponent<RectTransform>();
        _layoutOwnedBoosterRect.gameObject.SetActive(false);
        _layoutPotentialBoosterRect.gameObject.SetActive(false);
        ScreenState = ScreenState.WaitingLobby;
    }

    public void ChangeScreenState(ScreenState newScreenState)
    {
        ScreenState = newScreenState;
    }


    #region UI animations

    [ClientRpc]
    public void EnlargeOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        var smallScale = new Vector3(_smallScaleMultiplier, _smallScaleMultiplier, _smallScaleMultiplier);
        _layoutOwnedBoosterRect.localScale = smallScale;
        LeanTweenUtility.ScaleTo(_layoutOwnedBoosterRect, bigScale, 1);
    }

    [ClientRpc]
    public void ShrinkOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        var smallScale = new Vector3(_smallScaleMultiplier, _smallScaleMultiplier, _smallScaleMultiplier);
        _layoutOwnedBoosterRect.localScale = bigScale;
        LeanTweenUtility.ScaleTo(_layoutOwnedBoosterRect, smallScale, 1);
    }

    [ClientRpc]
    public void HideOwnedBoosterLayout()
    {
        if (_layoutOwnedBoosterRect.gameObject.activeSelf)
            LeanTweenUtility.ScaleOut(_layoutOwnedBoosterRect, 0.25f, true);
    }

    [ClientRpc]
    public void ShowOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        LeanTweenUtility.ScaleIn(_layoutOwnedBoosterRect, bigScale, 0.75f);
    }

    // Could also not be rpcs down here these 2
    [ClientRpc]
    public void ShowPotentialBoosterLayout()
    {
        LeanTweenUtility.ScaleIn(_layoutPotentialBoosterRect, Vector3.one, 0.75f);
    }

    [ClientRpc]
    public void HidePotentialBoosterLayout()
    {
        if (_layoutOwnedBoosterRect.gameObject.activeSelf)
            LeanTweenUtility.ScaleOut(_layoutPotentialBoosterRect, 0.25f, true);
    }
    #endregion



    public void DebugStartGame()
    {
        ChangeScreenState(ScreenState.PreDiceReceived);
    }
    public void DebugAnimationPlay()
    {
        ShowOwnedBoosterLayout();
    }

    public void StartGameWithOnePlayer()
    {
        if(PlayersManager.Instance.Players.Count == 1) { DebugStartGame(); }
    }
    public void StartGameWithTwoPlayers()
    {
        if (PlayersManager.Instance.Players.Count == 2) { DebugStartGame(); }

    }
    public void StartGameWithThreePlayers()
    {
        if (PlayersManager.Instance.Players.Count == 3) { DebugStartGame(); }

    }
    public void StartGameWithFourPlayers()
    {
        if (PlayersManager.Instance.Players.Count == 4) { DebugStartGame(); }

    }
}
