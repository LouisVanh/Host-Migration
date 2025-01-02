using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Card : NetworkBehaviour
{
    private TMPro.TMP_Text _nameText;
    private TMPro.TMP_Text _descriptionText;
    private Image _cardBackground;
    private Button _button;
    private IBooster _booster;

    private void Start()
    {
        // Get references
        _nameText = transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        _descriptionText = transform.GetChild(1).GetComponent<TMPro.TMP_Text>();
        _cardBackground = transform.GetComponent<Image>();
        _button = transform.GetComponent<Button>();
    }

    public void SetupCardVisual(IBooster cardBooster, Color color)
    {
        _booster = cardBooster;
        _nameText.text = cardBooster.Name;
        _descriptionText.text = cardBooster.Description;
        _cardBackground.color = color;
        _button.interactable = true;
    }

    public void DisableCardClick()
    {
        _button.interactable = false;
    }

    public void OnCardClicked()
    {
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
        player.BoosterManager.AddOwnedBooster(name);
    }

    [Command(requiresAuthority = false)]
    private void CmdSetPlayerReadyForNextWaveAndPossiblyStart(uint playerNetId)
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
                ResetPlayerReady();
                TurnManager.Instance.UpdateGameState(GameState.NewWaveEveryonePickedBooster);
            }
        }
    }

    [ClientRpc]
    private void ResetPlayerReady()
    {
        var player = NetworkClient.localPlayer.GetComponent<Player>();
        player.HasAlreadyRolled = false;
        player.ReadyForNextWave = false;
        player.CurrentDiceRollAmount = 0;
    }
}
