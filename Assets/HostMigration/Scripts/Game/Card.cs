using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Card : NetworkBehaviour
{
    [ReadOnly, SerializeField] private TMPro.TMP_Text _nameText;
    [ReadOnly, SerializeField] private TMPro.TMP_Text _descriptionText;
    [ReadOnly, SerializeField] private Image _cardBackground;
    [ReadOnly, SerializeField] private Button _button;
    [ReadOnly, SerializeField]
    BoosterCardVisualData _visualDataDebug;

    [Space(50)]
    public string IHopeThisMakesTheDebugVisible = "IHopeThisMakesTheDebugVisible";
    protected override void OnValidate()
    {
        if (Application.isPlaying) return;
        base.OnValidate();
        // Get references
        _nameText = transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        _descriptionText = transform.GetChild(1).GetComponent<TMPro.TMP_Text>();
        _cardBackground = transform.GetComponent<Image>();
        _button = transform.GetComponent<Button>();
    }

    public void SetupCardVisual(BoosterCardVisualData visualData)
    {
        _visualDataDebug = visualData;
        if (NetworkClient.localPlayer.name == null) Debug.LogError("player null");
        if (visualData== null) Debug.LogError("visualdata null");
        Debug.Log($"{NetworkClient.localPlayer.name}'s Visualdata: {visualData}");
        _nameText.text = visualData.Name;
        _descriptionText.text = visualData.Description;
        _cardBackground.color = visualData.Color;
        _button.interactable = true;
    }

    public void DisableCardClick()
    {
        _button.interactable = false;
    }

    public void OnCardClicked()
    {
        if (NetworkClient.localPlayer.GetComponent<Player>().HasAlreadyPickedCard) return;
        // Debug log to show the card was clicked
        Debug.Log($"Card clicked: {_nameText.text}");
        DisableCardClick();
        CmdAddBooster(NetworkClient.localPlayer.GetComponent<Player>(), _nameText.text);

        //Booster picked, wait for everyone to pick and let's go next wave
        CmdSetPlayerReadyForNextWaveAndPossiblyStart(NetworkClient.localPlayer.netId);
    }

    [Command(requiresAuthority = false)]
    private void CmdAddBooster(Player player, string name)
    {
        Debug.LogWarning($"CARD / Adding booster to {player}, by name {name}");
        _ = player.BoosterManager.AddOwnedBooster(player, name);
        player.HasAlreadyPickedCard = true;
    }

    [Command(requiresAuthority = false)]
    private async void CmdSetPlayerReadyForNextWaveAndPossiblyStart(uint playerNetId)
    {
        if (NetworkServer.spawned.TryGetValue(playerNetId, out NetworkIdentity playerIdObj))
        {
            playerIdObj.GetComponent<Player>().ReadyForNextWave = true;
            bool checkSum = true;
            foreach (uint id in PlayersManager.Instance.Players)
            {
                if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
                {
                    if (objNetIdentity.GetComponent<Player>().ReadyForNextWave == false) checkSum = false;
                }
            }
            if (checkSum)
            {
                // short delay let people catch their breath
                await System.Threading.Tasks.Task.Delay(250);

                ResetPlayerReady();
                TurnManager.Instance.UpdateGameState(GameState.NewWaveEveryonePickedBooster);
            }
        }
    }

    [Server]
    private void ResetPlayerReady()
    {
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            // Set the values of each player on the server, as this is Server--)Client syncvar
            player.HasAlreadyRolled = false;
            player.ReadyForNextWave = false;
            player.HasAlreadyPickedCard = false;
        }
    }
}
