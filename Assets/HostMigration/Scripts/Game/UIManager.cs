using Mirror;
using UnityEngine;

public enum ScreenState
{
    WaitingLobby,
    PreDiceReceived,
    EveryoneRollingTime,
    EveryoneJustRolled,
    InDiceCountingAnimation,
    AfterRollDamageEnemy,
    AfterRollEnemyAttack,
    EveryonePickBooster
}
public class UIManager : NetworkBehaviour
// IMPORTANT: DO NOT SYNC UI, NO MATTER HOW TEMPTING. BAD IDEA.
//  seriously. sync the values, call the changes on the client. 
{
    public static UIManager Instance { get; private set; }

    // For simple on / offs
    [Header("Canvases")]

    public Canvas HealthBarsCanvas;

    [SerializeField] private Canvas _startScreen, _preDiceScreen, _rollingTimePopupScreen, _diceRollingScreen, _allDiceRolledScreen, _ownedBoosterCardsCanvas, 
        _coolAnimationCountingDiceCanvas, _newBoostersShopCanvas;


    [Header("Animated")]

    // For animations, things that move or change in size (leave canvas on here, change transforms)
    [SerializeField] private FadePanel _fadePanel;
    RectTransform _layoutOwnedBoosterRect, _layoutPotentialBoosterRect;
    private float _largeScaleMultiplier = 1;
    private float _smallScaleMultiplier = 0.5f;

    public ScreenState ScreenState;

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
        UpdateUIState(ScreenState.WaitingLobby);
    }
    private void SetEverythingFalse()
    {
        _startScreen.gameObject.SetActive(false);
        _preDiceScreen.gameObject.SetActive(false);
        _rollingTimePopupScreen.gameObject.SetActive(false);
        _diceRollingScreen.gameObject.SetActive(false);
        _allDiceRolledScreen.gameObject.SetActive(false);
        HealthBarsCanvas.gameObject.SetActive(false);
        _ownedBoosterCardsCanvas.gameObject.SetActive(false);
        _allDiceRolledScreen.gameObject.SetActive(false);
        _coolAnimationCountingDiceCanvas.gameObject.SetActive(false);
        _newBoostersShopCanvas.gameObject.SetActive(false);
        _fadePanel.FadeOut(0);
    }
    public async void UpdateUIState(ScreenState newScreenState)
    {
        ScreenState = newScreenState; // Currently not in use yet, but a nice API element to have
        switch (newScreenState)
        {
            case ScreenState.WaitingLobby:
                SetEverythingFalse();
                _startScreen.gameObject.SetActive(true);
                // ... animations under here
                break;

            case ScreenState.PreDiceReceived:
                SetEverythingFalse();
                _fadePanel.FadeIn(1.5f);

                await System.Threading.Tasks.Task.Delay(1500);
                _rollingTimePopupScreen.gameObject.SetActive(true);
                await System.Threading.Tasks.Task.Delay(1000);

                _fadePanel.FadeOut(1.5f);
                _rollingTimePopupScreen.gameObject.SetActive(false);

                await System.Threading.Tasks.Task.Delay(1500);

                _preDiceScreen.gameObject.SetActive(true);
                HealthBarsCanvas.gameObject.SetActive(true);
                _ownedBoosterCardsCanvas.gameObject.SetActive(true);

                if(isServer) // just preventing warnings, this will only work on server, but it syncs for everything.
                TurnManager.Instance.UpdateGameState(GameState.EveryoneRollingTime);
                // ... animations under here
                if (! TurnManager.Instance.FirstRoundPlaying) // is this not the first time playing?
                {
                    ShowOwnedBoosterLayout();
                }
                break;
            case ScreenState.EveryoneRollingTime:
                if (! TurnManager.Instance.FirstRoundPlaying) // is this not the first time playing?
                {
                ShrinkOwnedBoosterLayout();
                }
                break;

            case ScreenState.EveryoneJustRolled:
                // GET READY TO START ANIMATION
                SetEverythingFalse();
                HealthBarsCanvas.gameObject.SetActive(true);
                // ...

                UpdateUIState(ScreenState.InDiceCountingAnimation);
                break;

            case ScreenState.InDiceCountingAnimation:
                // Do animation
                _fadePanel.FadeIn(0.5f);
                await System.Threading.Tasks.Task.Delay(500);
                _coolAnimationCountingDiceCanvas.gameObject.SetActive(true);
                _fadePanel.FadeOut(0.5f);
                await System.Threading.Tasks.Task.Delay(500);

                UpdateUIState(ScreenState.AfterRollDamageEnemy);
                break;

            case ScreenState.AfterRollDamageEnemy:
                // space for any animation
                if(isServer)
                TurnManager.Instance.UpdateGameState(GameState.AfterRollDamageEnemy);
                break;

            case ScreenState.AfterRollEnemyAttack:
                break;

            case ScreenState.EveryonePickBooster:
                SetEverythingFalse();
                HealthBarsCanvas.gameObject.SetActive(true);
                _newBoostersShopCanvas.gameObject.SetActive(true);
                // ... animations under here
                HideOwnedBoosterLayout();
                ShowPotentialBoosterLayout();
                break;

            default:
                break;
        }
    }


    #region UI animations

    public void EnlargeOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        var smallScale = new Vector3(_smallScaleMultiplier, _smallScaleMultiplier, _smallScaleMultiplier);
        _layoutOwnedBoosterRect.localScale = smallScale;
        LeanTweenUtility.ScaleTo(_layoutOwnedBoosterRect, bigScale, 1);
    }

    public void ShrinkOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        var smallScale = new Vector3(_smallScaleMultiplier, _smallScaleMultiplier, _smallScaleMultiplier);
        _layoutOwnedBoosterRect.localScale = bigScale;
        LeanTweenUtility.ScaleTo(_layoutOwnedBoosterRect, smallScale, 1);
    }

    public void HideOwnedBoosterLayout()
    {
        if (_layoutOwnedBoosterRect.gameObject.activeSelf)
            LeanTweenUtility.ScaleOut(_layoutOwnedBoosterRect, 0.25f, true);
    }

    public void ShowOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        LeanTweenUtility.ScaleIn(_layoutOwnedBoosterRect, bigScale, 0.75f);
    }


    public void ShowPotentialBoosterLayout()
    {
        LeanTweenUtility.ScaleIn(_layoutPotentialBoosterRect, Vector3.one, 0.75f);
    }

    public void HidePotentialBoosterLayout()
    {
        if (_layoutOwnedBoosterRect.gameObject.activeSelf)
            LeanTweenUtility.ScaleOut(_layoutPotentialBoosterRect, 0.25f, true);
    }
    #endregion


    public void DebugStartGame()
    {
        if(isServer) // to avoid warnings, wont run on client anyway
        TurnManager.Instance.UpdateGameState(GameState.PreDiceReceived);
    }
    public void DebugAnimationPlay()
    {
        ShowOwnedBoosterLayout();
    }

    [ClientRpc]
    private void RpcPlayGameWhenEverybodyReady()
    {
        DebugStartGame();
    }

    [Command(requiresAuthority = false)]
    private void CmdSetReadyAndPossiblyStartGame(Player playerCallingIt)
    {
        playerCallingIt.ReadyToPlay = true;
        bool checkSum = true;
        foreach (uint id in PlayersManager.Instance.Players)
        {
            if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
            {
                if (objNetIdentity.GetComponent<Player>().ReadyToPlay == false) checkSum = false;
            }
        }
        if (checkSum)
            RpcPlayGameWhenEverybodyReady();
    }

    private void RollDiceInPlayer() => NetworkClient.localPlayer.GetComponent<Player>().CmdRollDice();
    private async void CupAnimation()
    {
        NetworkClient.localPlayer.GetComponent<Player>().SpawnAndShakeDiceJar();
        SoundManager.Instance.PlayDiceInCupSound();
        await System.Threading.Tasks.Task.Delay(1000);
    }
    #region Buttons
    public void RollDiceBtn()
    {
        // do animation with cup
        CupAnimation();
        RollDiceInPlayer();
    }
    public void StartGameWithOnePlayer()
    {
        if (PlayersManager.Instance.Players.Count == 1)
        {
            CmdSetReadyAndPossiblyStartGame(NetworkClient.localPlayer.GetComponent<Player>());
        }
    }
    public void StartGameWithTwoPlayers()
    {
        if (PlayersManager.Instance.Players.Count == 2)
        {
            CmdSetReadyAndPossiblyStartGame(NetworkClient.localPlayer.GetComponent<Player>());
        }
    }
    public void StartGameWithThreePlayers()
    {
        if (PlayersManager.Instance.Players.Count == 3)
        {
            CmdSetReadyAndPossiblyStartGame(NetworkClient.localPlayer.GetComponent<Player>());
        }
    }
    public void StartGameWithFourPlayers()
    {
        if (PlayersManager.Instance.Players.Count == 4)
        {
            CmdSetReadyAndPossiblyStartGame(NetworkClient.localPlayer.GetComponent<Player>());
        }
    }
    #endregion Buttons
}
