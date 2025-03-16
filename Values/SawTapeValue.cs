using System;

namespace SawTapes.Values
{
    public class SawTapeValue(Type type, Item item, int rarity, int minPlayers, int maxPlayers, string interiorsExclusion)
    {
        public Type Type { get; internal set; } = type;
        public Item Item { get; internal set; } = item;
        public int DefaultRarity { get; internal set; } = rarity;
        public int Rarity { get; internal set; } = rarity;
        public int MinPlayers { get; internal set; } = minPlayers;
        public int MaxPlayers { get; internal set; } = maxPlayers;
        public string InteriorsExclusion { get; internal set; } = interiorsExclusion;
    }
}
