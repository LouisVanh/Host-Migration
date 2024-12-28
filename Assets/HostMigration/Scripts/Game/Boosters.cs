public interface IBooster
{
    string Name { get; }
    string Description { get; }
    BoosterRarity Rarity { get; }
}
public enum BoosterRarity
{
    Common, Legendary, Chaos
}

public interface IBoosterPermanent : IBooster
{
    void AddPermanentEffect(Player player);
    void RemovePermanentEffect(Player player);
}

public interface IBoosterConsumable : IBooster
{
    void ConsumeEffect(Player player);
}
