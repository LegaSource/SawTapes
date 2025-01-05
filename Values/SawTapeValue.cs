using System;

namespace SawTapes.Values
{
    public class SawTapeValue(Type type, Item item, int rarity, bool isTileGame, string interiorsExclusion)
    {
        public Type Type { get; internal set; } = type;
        public Item Item { get; internal set; } = item;
        public int DefaultRarity { get; internal set; } = rarity;
        public int Rarity { get; internal set; } = rarity;
        public bool IsTileGame { get; internal set; } = isTileGame;
        public string InteriorsExclusion { get; internal set; } = interiorsExclusion;
    }
}
