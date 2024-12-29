using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum OrientationWhatSideUp : byte { head, body, feet, hands }

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

    // Anything else could be added in the future, too.


    public override string ToString()
    {
        return $"Player={PlayerNetId} roll={EyesRolled} DiceNetId={DiceNetId}";
    }
}

public class DiceManager : NetworkBehaviour
{
    public readonly SyncList<Dice> DiceList = new SyncList<Dice>();
    [SerializeField] private GameObject _dicePrefab;

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

    private Vector3 GetDicePosition(int index)
    {
        Debug.LogWarning("Dice position not implemented yet");
        if (NetworkClient.spawned.TryGetValue(DiceList[index].PlayerNetId, out NetworkIdentity playerNetIdentity))
        {
            Vector3 posOnScreen = playerNetIdentity.GetComponent<Player>().GetPlayerCupPosition();
            // calculate exact dice position too: put down lot of dice in convincing manner, then remove the components and drag in as a transform into this
            return posOnScreen;
        }
        else
        {
            Debug.LogWarning("NO PLAYER FOUND WHEN PUTTING DOWN DICE!");
            return Vector3.zero;
        }
    }

    private Quaternion GetDiceRotation(int index)
    {
        int eyesRolled = DiceList[index].EyesRolled;
        // assuming the prefab has six up, rotate appropriately
        Debug.LogWarning("Dice rotation not implemented yet");
        return Quaternion.identity;
    }

    void OnDiceAdded(int index)
    {
        Debug.Log($"Dice added at index {index} {DiceList[index]}");
        // Only spawn dice for this player if he's still connected
        if (NetworkClient.spawned.TryGetValue(DiceList[index].PlayerNetId, out NetworkIdentity _))
        {

            var dice = Instantiate(_dicePrefab);
            NetworkServer.Spawn(dice);

            // Set position and rotation after spawning so it gets synced. Make sure dice has a NT for this to get synced!
            dice.transform.SetPositionAndRotation(GetDicePosition(index), GetDiceRotation(index));
        }
    }

    void OnDiceInserted(int index)
    {
        Debug.Log($"Dice inserted at index {index} {DiceList[index]}");
    }

    void OnDiceChanged(int index, Dice oldValue)
    {
        Debug.Log($"Dice changed at index {index} from {oldValue} to {DiceList[index]}");
    }

    void OnDiceRemoved(int index, Dice oldDice)
    {
        Debug.Log($"Dice removed at index {index} {oldDice}");
        var id = oldDice.DiceNetId;
        if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
        {
            Debug.Log($"Dice removed {objNetIdentity.transform.name}");
            NetworkServer.UnSpawn(objNetIdentity.gameObject);
        }
    }

    void OnDiceListCleared()
    {
        // OnListCleared is called before the list is actually cleared
        // so we can iterate the list to get the elements if needed.
        foreach (Dice dice in DiceList)
        {
            var id = dice.DiceNetId;
            if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
            {
                Debug.Log($"Dice cleared {objNetIdentity.transform.name}");
                NetworkServer.UnSpawn(objNetIdentity.gameObject);
            }
        }
    }
    [Server]
    public void AddDice(Dice dice)
    {
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