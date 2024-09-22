using DunGen;
using HarmonyLib;
using SawTapes.Values;
using System.Linq;

namespace SawTapes.Patches
{
    internal class DungeonPatch
    {
        public static bool isGenerateTileGame = true;

        [HarmonyPatch(typeof(Dungeon), nameof(Dungeon.SpawnDoorPrefab))]
        [HarmonyPrefix]
        private static void SpawnDoor(ref Doorway a, ref Doorway b)
        {
            if (isGenerateTileGame)
            {
                RemoveSpawnEmptyDoor(a);
                RemoveSpawnEmptyDoor(b);
            }
        }

        private static void RemoveSpawnEmptyDoor(Doorway doorway)
        {
            Room room = SawTapes.rooms.FirstOrDefault(r => doorway.tile.name.Contains(r.RoomName));
            if (room != null
                && doorway.ConnectorPrefabWeights.HasAnyViableEntries()
                && doorway.ConnectedDoorway.ConnectorPrefabWeights.HasAnyViableEntries())
            {
                doorway.ConnectorPrefabWeights.RemoveAll(d => !d.GameObject.name.Equals(room.DoorName));
                doorway.ConnectedDoorway.ConnectorPrefabWeights.RemoveAll(d => !d.GameObject.name.Equals(room.DoorName));
                if (!SawTapes.eligibleTiles.Contains(doorway.tile))
                {
                    for (int i = 0; i < room.Weight; i++)
                    {
                        SawTapes.eligibleTiles.Add(doorway.tile);
                    }
                }
            }
        }
    }
}
