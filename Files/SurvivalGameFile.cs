using BepInEx;
using Newtonsoft.Json.Linq;
using SawTapes.Files.Values;
using SawTapes.Values;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SawTapes.Files
{
    internal class SurvivalGameFile
    {
        public static string FilePath = Path.Combine(Paths.ConfigPath, "ST.survival_game.json");

        public static string Get()
        {
            return "{\n" +
                "  \"hordes\": [\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 45 }\n" +
                "      ]\n" +
                "    }\n" +
                "  ],\n" +
                "  \"rooms\": [\n" +
                "    {\n" +
                "      \"room_name\": \"4x4BigStairTile\",\n" +
                "      \"door_name\": \"SteelDoorMapSpawn\",\n" +
                "      \"hordes\": [\"Horde1\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"4x4ServerRoomTile\",\n" +
                "      \"door_name\": \"SteelDoorMapSpawn\",\n" +
                "      \"hordes\": [\"Horde1\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"MediumRoomHallway1B\",\n" +
                "      \"door_name\": \"SteelDoorMapSpawn\",\n" +
                "      \"hordes\": [\"Horde1\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"LibraryTile\",\n" +
                "      \"door_name\": \"FancyDoorMapSpawn\",\n" +
                "      \"hordes\": [\"Horde2\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"HallwayTileTypeB\",\n" +
                "      \"door_name\": \"FancyDoorMapSpawn\",\n" +
                "      \"hordes\": [\"Horde2\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"HallwayTileTypeC\",\n" +
                "      \"door_name\": \"FancyDoorMapSpawn\",\n" +
                "      \"hordes\": [\"Horde2\"]\n" +
                "    }\n" +
                "  ]\n" +
                "}";
        }

        public static void LoadJSON()
        {
            if (!File.Exists(Path.Combine(Paths.ConfigPath, FilePath)))
            {
                File.WriteAllText(Path.Combine(Paths.ConfigPath, FilePath), Get());
            }

            using (var reader = new StreamReader(Path.Combine(Paths.ConfigPath, FilePath)))
            {
                List<HordeMapping> hordesMapping = LoadHordes();
                foreach (HordeMapping horde in hordesMapping)
                {
                    Dictionary<int, EnemyType> enemiesSpawn = new Dictionary<int, EnemyType>();
                    foreach (EnemySpawnMapping enemySpawnMapping in horde.EnemiesSpawn)
                    {
                        EnemyType enemyType = SawTapes.allEnemies.FirstOrDefault(e => e.enemyName.Equals(enemySpawnMapping.Enemy));
                        if (enemyType != null)
                        {
                            enemiesSpawn.Add(enemySpawnMapping.Time, enemyType);
                        }
                        else
                        {
                            SawTapes.mls.LogWarning($"The enemy {enemySpawnMapping.Enemy} could not be found from the ST.survival_game.json file.");
                        }
                    }
                    SawTapes.hordes.Add(new Horde(horde.HordeName, horde.GameDuration, enemiesSpawn));
                }

                List<RoomMapping> roomsMapping = LoadRooms();
                foreach (RoomMapping room in roomsMapping)
                {
                    List<Horde> hordes = new List<Horde>();
                    foreach (string hordeName in room.Hordes)
                    {
                        Horde horde = SawTapes.hordes.FirstOrDefault(h => h.HordeName.Equals(hordeName));
                        if (horde != null)
                        {
                            hordes.Add(horde);
                        }
                        else
                        {
                            SawTapes.mls.LogWarning($"The horde {hordeName} could not be found from the file configuration.");
                        }
                    }
                    SawTapes.rooms.Add(new Room(room.RoomName, room.DoorName, hordes));
                }
            }
        }

        public static List<HordeMapping> LoadHordes()
        {
            string json = File.ReadAllText(FilePath);
            return JObject.Parse(json)["hordes"].ToObject<List<HordeMapping>>();
        }

        public static List<RoomMapping> LoadRooms()
        {
            string json = File.ReadAllText(FilePath);
            return JObject.Parse(json)["rooms"].ToObject<List<RoomMapping>>();
        }
    }
}
