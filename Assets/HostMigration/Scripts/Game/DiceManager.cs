using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Dice
{
    public uint PlayerNetId;
    public int EyesRolled;
    public uint DiceNetId;

    public Dice(uint playerNetId, int eyesRolled)
    {
        PlayerNetId = playerNetId;
        EyesRolled = eyesRolled;
        //DiceNetId = diceNetId; // assigned when instantiated
    }
    public Dice() { }

    // Anything else could be added in the future, too.


    public override string ToString()
    {
        return $"Player={PlayerNetId} roll={EyesRolled} DiceNetId={DiceNetId}";
    }
}

public class DiceManager : NetworkBehaviour
{
    public readonly SyncList<Dice> DiceList = new SyncList<Dice>();
    public List<uint> CupIdList = new();
    [SerializeField] private GameObject _dicePrefab;

    public int LeftOverEyesFromLastRoll;
    public static DiceManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnStartClient()
    {
        // Add handlers for SyncList Actions
        DiceList.OnAdd += OnDiceAdded;
        DiceList.OnInsert += OnDiceInserted;
        DiceList.OnSet += OnDiceChanged;
        DiceList.OnRemove += OnDiceRemoved;
        DiceList.OnClear += OnDiceListCleared;

        // List is populated before handlers are wired up so we
        // need to manually invoke OnAdd for each element.
        for (int i = 0; i < DiceList.Count; i++)
            DiceList.OnAdd.Invoke(i);
    }

    public override void OnStopClient()
    {
        // Remove handlers when client stops
        DiceList.OnAdd -= OnDiceAdded;
        DiceList.OnInsert -= OnDiceInserted;
        DiceList.OnSet -= OnDiceChanged;
        DiceList.OnRemove -= OnDiceRemoved;
        DiceList.OnClear -= OnDiceListCleared;
    }

    [Server]
    public int CountTotalRollAmount()
    {
        var total = 0;
        foreach (Dice dice in GetAllDice())
        {
            if (NetworkServer.spawned.TryGetValue(dice.PlayerNetId, out NetworkIdentity playerNetId))
            {
                total += Mathf.RoundToInt(dice.EyesRolled * playerNetId.GetComponent<Player>().BoosterManager.DamageMultiplier);
            }
        }
        return total;
    }


    [Server]
    private Vector3 GetDicePosition(int index)
    {
        // Ensure the player identity exists
        if (NetworkClient.spawned.TryGetValue(DiceList[index].PlayerNetId, out NetworkIdentity playerNetIdentity))
        {
            // Get the center position for this player's dice
            Vector3 centerPosition = playerNetIdentity.GetComponent<Player>().GetPlayerCupPosition();

            // Define the radius for spacing between dice
            float diceRadius = 2; // Adjust based on dice size
            float spacing = 1.2f;    // Multiplier for distance between dice to avoid intersections

            // Use a circular pattern for up to 7 dice
            const int maxDice = 7; // Max dice to spawn
            float angleStep = 360f / maxDice; // Angle between dice

            // Calculate position in the circle
            float angle = index * angleStep * Mathf.Deg2Rad; // Convert to radians
            float xOffset = Mathf.Cos(angle) * diceRadius * spacing;
            float yOffset = Mathf.Sin(angle) * diceRadius * spacing;

            // Return the calculated position
            return centerPosition + new Vector3(xOffset, yOffset, 0);
        }
        else
        {
            Debug.LogWarning("NO PLAYER FOUND WHEN PUTTING DOWN DICE!");
            return Vector3.zero;
        }
    }

    [Server]
    private Quaternion GetDiceRotation(int index)
    {
        int eyesRolled = DiceList[index].EyesRolled;

        // Add a random rotation around the Z-axis for realism
        float randomZ = Random.Range(0, 360);

        // Define the target rotations for each face (1 to 6).
        switch (eyesRolled)
        {
            case 1:
                return Quaternion.Euler(randomZ, -90, 90); // 1 up
            case 2:
                return Quaternion.Euler(randomZ, -90, 0); // 2 up
            case 3:
                return Quaternion.Euler(0, 180, randomZ); // 3 up (Different axis then the 1, 2, 5, 6)
            case 4:
                return Quaternion.Euler(0, 0, randomZ); // 4 up (default orientation) (Different axis then the 1, 2, 5, 6)
            case 5:
                return Quaternion.Euler(randomZ, 90, 0); // 5 up 
            case 6:
                return Quaternion.Euler(randomZ, 90, 90); // 6 up 
            default:
                Debug.LogWarning($"Invalid eyes rolled value: {eyesRolled}");
                return Quaternion.identity; // Default to 4 up if invalid
        }
    }



    [ServerCallback]
    void OnDiceAdded(int index)
    {
        //Debug.Log($"Dice added at index {index} {DiceList[index]}");
        // Only spawn dice for this player if he's still connected
        if (NetworkClient.spawned.TryGetValue(DiceList[index].PlayerNetId, out NetworkIdentity _))
        {

            var dice = Instantiate(_dicePrefab);
            NetworkServer.Spawn(dice);

            DiceList[index].DiceNetId = dice.GetComponent<NetworkIdentity>().netId;
            CheckIfEverybodyRolledDice();
            // Set position and rotation after spawning so it gets synced. Make sure dice has a NT for this to get synced!
            dice.transform.SetPositionAndRotation(GetDicePosition(index), GetDiceRotation(index));
        }
    }

    [ServerCallback]
    void OnDiceInserted(int index)
    {
        Debug.Log($"Dice inserted at index {index} {DiceList[index]}");
    }

    [ServerCallback]
    void OnDiceChanged(int index, Dice oldValue)
    {
        Debug.Log($"Dice changed at index {index} from {oldValue} to {DiceList[index]}");
    }

    [ServerCallback]
    void OnDiceRemoved(int index, Dice oldDice)
    {
        //Debug.Log($"Dice removed at index {index} {oldDice}");
        var id = oldDice.DiceNetId;
        if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
        {
            Debug.Log($"Dice removed {objNetIdentity.transform.name}");
            NetworkServer.UnSpawn(objNetIdentity.gameObject);
        }
    }

    [ServerCallback]
    void OnDiceListCleared()
    {
        // OnListCleared is called before the list is actually cleared
        // so we can iterate the list to get the elements if needed.
        foreach (Dice oldDice in DiceList)
        {
            var id = oldDice.DiceNetId;
            if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
            {
                Debug.Log($"Dice cleared {objNetIdentity.transform.name}");
                NetworkServer.UnSpawn(objNetIdentity.gameObject);
            }
        }
    }

    [Server]
    void CheckIfEverybodyRolledDice()
    {
        bool checkSum = true;
        var list = PlayersManager.Instance.GetAlivePlayers();

        foreach (Player player in list)
        {
        // HasAlreadyRolled is only set when all dice of a player are rolled
            if (player.HasAlreadyRolled == false) checkSum = false;
        }
        if (checkSum)
        {
            Debug.Log("DICEMANAGER/ Everyone has rolled! Moving to EveryoneJustRolled state");
            TurnManager.Instance.UpdateGameState(GameState.EveryoneJustRolled);
        }
    }

    [Server]
    public void AddDice(Dice dice)
    {
        //Debug.Log("WE GOT INTO THE ADDDICE METHOD!");
        if (!DiceList.Contains(dice))
        {
            DiceList.Add(dice);
            Debug.Log($"Server: Added dice - {dice}");
        }
    }

    [Server]
    public void RemoveDice(Dice dice)
    {
        if (DiceList.Contains(dice))
        {
            DiceList.Remove(dice);
            Debug.Log($"Server: Removed dice - {dice}");
        }
    }

    [Server]
    public void ResetAfterWaveComplete()
    {
        RemoveAllDice();
        LeftOverEyesFromLastRoll = 0;
    }
    [Server]
    public void RemoveAllDice()
    {
        DiceList.Clear();
        TurnManager.Instance.CurrentDiceRoll = 0;
    }

    public List<Dice> GetAllDice()
    {
        List<Dice> diceList = new();
        foreach (Dice dice in DiceList)
        {
            diceList.Add(dice);
        }
        return diceList;
    }
}