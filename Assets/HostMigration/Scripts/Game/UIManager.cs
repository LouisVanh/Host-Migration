using Mirror;
using System.Collections.Generic;
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
    EveryonePickBooster,
    EndOfGame
}
public class UIManager : NetworkBehaviour
// IMPORTANT: DO NOT SYNC UI, NO MATTER HOW TEMPTING. BAD IDEA.
//  seriously. sync the values, call the changes on the client. 
{
    public static UIManager Instance { get; private set; }

    public ScreenState ScreenState;
    // For simple on / offs
    [Header("Canvases")]

    public Canvas HealthBarsCanvas;

    [SerializeField]
    private Canvas _startScreen, _preDiceScreen, _rollingTimePopupScreen, _diceRollingScreen, _allDiceRolledScreen, _ownedBoosterCardsPopUpIconCanvas,
        _coolAnimationCountingDiceCanvas, _newBoostersShopCanvas, _boosterCardsCanvas, _waveCountCanvas, _endGameCanvas;


    [Header("Animated")]

    // For animations, things that move or change in size (leave canvas on here, change transforms)
    [SerializeField] private FadePanel _fadePanel;
    RectTransform _layoutOwnedBoosterRect, _layoutPotentialBoosterRect;
    private float _largeScaleMultiplier = 1;
    private float _smallScaleMultiplier = 0.5f;

    [Header("Color")]
    [SerializeField] private Color _commonCardColor;
    [SerializeField] private Color _legendaryCardColor;
    [SerializeField] private Color _chaosCardColor;
    public Dictionary<BoosterRarity, Color> RarityColors;

    [Header("Potential")]
    [SerializeField] private Card _potentialUnlockCard1;
    [SerializeField] private Card _potentialUnlockCard2;
    [SerializeField] private Card _potentialUnlockCard3;

    [Header("Owned")]
    [SerializeField] private Card _inventoryCard1;
    [SerializeField] private Card _inventoryCard2;
    [SerializeField] private Card _inventoryCard3;
    [SerializeField] private Card _inventoryCard4;
    [SerializeField] private Card _inventoryCard5;
    [SerializeField] private Card _inventoryCard6;
    [SerializeField] private Card _inventoryCard7;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);

        //setup colors
        RarityColors = new Dictionary<BoosterRarity, Color>
        {
            { BoosterRarity.Common, _commonCardColor },
            { BoosterRarity.Legendary, _legendaryCardColor },
            { BoosterRarity.Chaos, _chaosCardColor }
        };
    }

    public void StartOwnPlayerUI()
    {
        _layoutOwnedBoosterRect = GameObject.FindWithTag("CardLayout").GetComponent<RectTransform>();
        _layoutPotentialBoosterRect = GameObject.FindWithTag("NewCardLayout").GetComponent<RectTransform>();
        _layoutOwnedBoosterRect.gameObject.SetActive(false);
        _layoutPotentialBoosterRect.gameObject.SetActive(false);
        UpdateUIState(ScreenState.WaitingLobby);
    }
    private void SetEverythingFalse()
    {
        HealthBarsCanvas.gameObject.SetActive(false);
        _startScreen.gameObject.SetActive(false);
        _preDiceScreen.gameObject.SetActive(false);
        _rollingTimePopupScreen.gameObject.SetActive(false);
        _diceRollingScreen.gameObject.SetActive(false);
        _allDiceRolledScreen.gameObject.SetActive(false);
        _ownedBoosterCardsPopUpIconCanvas.gameObject.SetActive(false);
        _allDiceRolledScreen.gameObject.SetActive(false);
        _coolAnimationCountingDiceCanvas.gameObject.SetActive(false);
        _newBoostersShopCanvas.gameObject.SetActive(false);
        _boosterCardsCanvas.gameObject.SetActive(false);
        _endGameCanvas.gameObject.SetActive(false);
        _waveCountCanvas.gameObject.SetActive(false);
        _fadePanel.FadeOut(0);
    }

    [ClientRpc]
    internal void RpcUpdateWaveCounter(int currentWaveCount)
    {
        _waveCountCanvas.transform.GetChild(0).GetComponent<TMPro.TMP_Text>().text = $"Wave: {currentWaveCount}";
    }

    // Client side, ran from syncvar hook
    internal void UpdatePotentialCardsVisualAndShow()
    {
        BoosterCardVisualData[] slotsVisuals = NetworkClient.localPlayer.GetComponent<BoostersManager>().PotentialBoosterSlotsVisuals;

        UpdatePotentialCardVisualAndShow(_potentialUnlockCard1, slotsVisuals[0]);
        UpdatePotentialCardVisualAndShow(_potentialUnlockCard2, slotsVisuals[1]);
        UpdatePotentialCardVisualAndShow(_potentialUnlockCard3, slotsVisuals[2]);
    }
    private void UpdatePotentialCardVisualAndShow(Card card, BoosterCardVisualData visualData) => card.SetupCardVisual(visualData);

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
                var fadeInDuration = .5f;
                var fadeOutDuration = 1f;

                _fadePanel.FadeIn(fadeInDuration);

                await System.Threading.Tasks.Task.Delay((int)(fadeInDuration * 1000));
                _rollingTimePopupScreen.gameObject.SetActive(true);
                await System.Threading.Tasks.Task.Delay((int)(fadeOutDuration * 1000));

                _fadePanel.FadeOut(fadeOutDuration);
                _rollingTimePopupScreen.gameObject.SetActive(false);

                // short delay let people catch their breath
                await System.Threading.Tasks.Task.Delay(250);

                _preDiceScreen.gameObject.SetActive(true);
                HealthBarsCanvas.gameObject.SetActive(true);
                _waveCountCanvas.gameObject.SetActive(true);

                if (!TurnManager.Instance.FirstWavePlaying) // is this not the first time playing?
                {
                    _ownedBoosterCardsPopUpIconCanvas.gameObject.SetActive(true); // Clicking this will bring up the thing under here (TODO)
                    //ShowOwnedBoosterLayout();
                }

                if (isServer) // just preventing warnings, this will only work on server, but it syncs for everything.
                    TurnManager.Instance.UpdateGameState(GameState.EveryoneRollingTime);
                // ... animations under here
                break;

            case ScreenState.EveryoneRollingTime:
                _waveCountCanvas.gameObject.SetActive(true);
                _diceRollingScreen.gameObject.SetActive(true);
                //if (!TurnManager.Instance.FirstRoundPlaying) // is this not the first time playing?
                //{
                //    ShrinkOwnedBoosterLayout();
                //}
                break;

            case ScreenState.EveryoneJustRolled:
                // GET READY TO START ANIMATION
                SetEverythingFalse();
                _waveCountCanvas.gameObject.SetActive(true);
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
                if (isServer)
                    TurnManager.Instance.UpdateGameState(GameState.AfterRollDamageEnemy);
                break;

            case ScreenState.AfterRollEnemyAttack:
                break;

            case ScreenState.EveryonePickBooster:
                SetEverythingFalse();
                HealthBarsCanvas.gameObject.SetActive(true);
                _newBoostersShopCanvas.gameObject.SetActive(true);
                _waveCountCanvas.gameObject.SetActive(true);

                // ... animations under here
                HideOwnedBoosterLayout();
                ShowPotentialBoosterLayout();
                break;

            case ScreenState.EndOfGame:
                SetEverythingFalse();
                _endGameCanvas.gameObject.SetActive(true);
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

    public void DebugAnimationPlay()
    {
        ShowOwnedBoosterLayout();
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
        {
            NetworkClient.localPlayer.GetComponent<Player>().BoosterManager.PopulateOwnedBoosters();

            TurnManager.Instance.UpdateGameState(GameState.PreDiceReceived);
        }
    }

    private void RollDiceInPlayer()
        => NetworkClient.localPlayer.GetComponent<Player>()
        .CmdRollDice(NetworkClient.localPlayer.GetComponent<Player>().BoosterManager.LifestealPercentage);

    private async void CupAnimation()
    {
        NetworkClient.localPlayer.GetComponent<Player>().SpawnAndShakeDiceJar();
        SoundManager.Instance.PlayDiceInCupSound();
        await System.Threading.Tasks.Task.Delay(1000);
    }
    #region Buttons

    // TODO MAKE BUTTON FOR UI POPUP

    public void RollDiceBtn()
    {
        if (NetworkClient.localPlayer.GetComponent<Player>().HasAlreadyRolled) return;
        // do animation with cup
        //CupAnimation();
        RollDiceInPlayer();
    }

    public void RestartGameBtn()
    {
        Debug.Log("Trying to restart game");
        CmdRestartGame();
    }
    [Command(requiresAuthority = false)]
    private void CmdRestartGame()
    {
        Debug.Log("Changing scene to game again");
        NetworkManager.singleton.ServerChangeScene("GameScene");
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
