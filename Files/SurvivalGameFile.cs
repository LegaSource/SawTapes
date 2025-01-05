using BepInEx;
using Newtonsoft.Json.Linq;
using SawTapes.Files.Values;
using SawTapes.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SawTapes.Files
{
    public class SurvivalGameFile
    {
        public static string FilePath = Path.Combine(Paths.ConfigPath, "ST.survival_game.json");

        public static string Get()
            => "{\n" +
                "  \"hordes\": [\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_FacilitySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_FacilitySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_FacilitySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_FacilitySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_HauntedMansionLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_HauntedMansionLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_HauntedMansionLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_HauntedMansionLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_HauntedMansionTileTypeSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_HauntedMansionTileTypeSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_HauntedMansionTileTypeSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_HauntedMansionTileTypeSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde5_HauntedMansionTileTypeSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_MediumLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_MediumLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_MediumLibrarySize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_WarehouseSizeWithCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 35 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_WarehouseSizeWithCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_WarehouseSizeWithCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 0 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Baboon hawk\", \"time\": 10 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 15 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Baboon hawk\", \"time\": 35 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_WarehouseSizeWithCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 0 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 15 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 20 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 35 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_WarehouseSizeWithoutCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_WarehouseSizeWithoutCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_WarehouseSizeWithoutCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Baboon hawk\", \"time\": 10 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 15 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Baboon hawk\", \"time\": 35 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_WarehouseSizeWithoutCover\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 15 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 20 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 35 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde_WarehouseSizeOutside\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"ForestGiant\", \"time\": 0 },\n" +
                "        { \"enemy\": \"MouthDog\", \"time\": 20 },\n" +
                "        { \"enemy\": \"ForestGiant\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Baboon hawk\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde_WarehouseSizeBlackMesa\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Masked\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_DoubleDoorTile\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_DoubleDoorTile\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 30 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_DoubleDoorTile\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 35 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_DoubleDoorTile\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_Chizra\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 50 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_Chizra\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Blob\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Nutcracker\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 35 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde1_LittleRoomWithoutCoverSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Clay Surgeon\", \"time\": 40 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde2_LittleRoomWithoutCoverSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Spring\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 35 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde3_LittleRoomWithoutCoverSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Crawler\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Bunker Spider\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Blob\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde4_LittleRoomWithoutCoverSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 5 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 25 },\n" +
                "        { \"enemy\": \"Spring\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 40 },\n" +
                "        { \"enemy\": \"Hoarding bug\", \"time\": 45 }\n" +
                "      ]\n" +
                "    },\n" +
                "    {\n" +
                "      \"horde_name\": \"Horde5_LittleRoomWithoutCoverSize\",\n" +
                "      \"game_duration\": 60,\n" +
                "      \"billy_value\": 120,\n" +
                "      \"min_hour\": 1,\n" +
                "      \"max_hour\": 18,\n" +
                "      \"enemies_spawn\": [\n" +
                "        { \"enemy\": \"Masked\", \"time\": 0 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 10 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 15 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 20 },\n" +
                "        { \"enemy\": \"Flowerman\", \"time\": 35 },\n" +
                "        { \"enemy\": \"Puffer\", \"time\": 45 }\n" +
                "      ]\n" +
                "    }\n" +
                "  ],\n" +
                "  \"rooms\": [\n" +
                "    // Facility\n" +
                "    {\n" +
                "      \"room_name\": \"4x4BigStairTile\",\n" +
                "      \"doors_names\": [\"SteelDoorMapSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"4x4ServerRoomTile\",\n" +
                "      \"doors_names\": [\"SteelDoorMapSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"MediumRoomHallway1B\",\n" +
                "      \"doors_names\": [\"SteelDoorMapSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // Haunted Mansion\n" +
                "    {\n" +
                "      \"room_name\": \"LibraryTile\",\n" +
                "      \"doors_names\": [\"FancyDoorMapSpawn\"],\n" +
                "      \"weight\": 4,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionLibrarySize\", \"Horde2_HauntedMansionLibrarySize\", \"Horde3_HauntedMansionLibrarySize\", \"Horde4_HauntedMansionLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"HallwayTileTypeB\",\n" +
                "      \"doors_names\": [\"FancyDoorMapSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionTileTypeSize\", \"Horde2_HauntedMansionTileTypeSize\", \"Horde3_HauntedMansionTileTypeSize\", \"Horde4_HauntedMansionTileTypeSize\", \"Horde5_HauntedMansionTileTypeSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"HallwayTileTypeC\",\n" +
                "      \"doors_names\": [\"FancyDoorMapSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionTileTypeSize\", \"Horde2_HauntedMansionTileTypeSize\", \"Horde3_HauntedMansionTileTypeSize\", \"Horde4_HauntedMansionTileTypeSize\", \"Horde5_HauntedMansionTileTypeSize\"]\n" +
                "    },\n" +
                "    // Guardia Fortress\n" +
                "    {\n" +
                "      \"room_name\": \"GFC_FlagRoom\",\n" +
                "      \"doors_names\": [\"GuardiaDoorSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\", \"Horde_WarehouseSizeOutside\"]\n" +
                "    },\n" +
                "    // Nali Haven\n" +
                "    {\n" +
                "      \"room_name\": \"NHBigLibrary\",\n" +
                "      \"doors_names\": [\"HavenDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"NHEntryRoom2\",\n" +
                "      \"doors_names\": [\"HavenDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\", \"Horde_WarehouseSizeOutside\"]\n" +
                "    },\n" +
                "    // Skaarj Outpost\n" +
                "    {\n" +
                "      \"room_name\": \"USKObservationalHall\",\n" +
                "      \"doors_names\": [\"SkaarjDoubleDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionTileTypeSize\", \"Horde2_HauntedMansionTileTypeSize\", \"Horde3_HauntedMansionTileTypeSize\", \"Horde4_HauntedMansionTileTypeSize\", \"Horde5_HauntedMansionTileTypeSize\"]\n" +
                "    },\n" +
                "    // Chizra Temple\n" +
                "    {\n" +
                "      \"room_name\": \"UCHIZDeathPit\",\n" +
                "      \"doors_names\": [\"ChizraDoorSpawn\"],\n" +
                "      \"weight\": 3,\n" +
                "      \"hordes\": [\"Horde1_Chizra\", \"Horde2_Chizra\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"UCHIZ6FiresPool\",\n" +
                "      \"doors_names\": [\"ChizraDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    // Gothic Monastery\n" +
                "    {\n" +
                "      \"room_name\": \"GMFountainHall\",\n" +
                "      \"doors_names\": [\"GothicDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"GMStep8Way\",\n" +
                "      \"doors_names\": [\"GothicDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionTileTypeSize\", \"Horde2_HauntedMansionTileTypeSize\", \"Horde3_HauntedMansionTileTypeSize\", \"Horde4_HauntedMansionTileTypeSize\", \"Horde5_HauntedMansionTileTypeSize\"]\n" +
                "    },\n" +
                "    // StarlancerWarehouse\n" +
                "    {\n" +
                "      \"room_name\": \"MegaPowerCore\",\n" +
                "      \"doors_names\": [\"SciFiDoorSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"RoomOfVerticality\",\n" +
                "      \"doors_names\": [\"SciFiDoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\", \"Horde_WarehouseSizeOutside\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"MegaEnergyStorage\",\n" +
                "      \"doors_names\": [\"SciFiDoorSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithCover\", \"Horde2_WarehouseSizeWithCover\", \"Horde3_WarehouseSizeWithCover\", \"Horde4_WarehouseSizeWithCover\", \"Horde_WarehouseSizeOutside\"]\n" +
                "    },\n" +
                "    // Devil Mansion\n" +
                "    {\n" +
                "      \"room_name\": \"SM_ServQuarters_V2_FINAL_24x16 Tile\",\n" +
                "      \"doors_names\": [\"CN_Door180_Spawner_Prefab\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_DoubleDoorTile\", \"Horde2_DoubleDoorTile\", \"Horde3_DoubleDoorTile\", \"Horde4_DoubleDoorTile\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"SM_Kitchen_V4_FINAL_24x16 Tile\",\n" +
                "      \"doors_names\": [\"CN_Door180_Spawner_Prefab\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"SM_Library_V1_FINAL_24x16 Tile\",\n" +
                "      \"doors_names\": [\"CN_Door180_Spawner_Prefab\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    // Storehouse\n" +
                "    {\n" +
                "      \"room_name\": \"appyroom\",\n" +
                "      \"doors_names\": [\"ConnectorDoor\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_DoubleDoorTile\", \"Horde2_DoubleDoorTile\", \"Horde3_DoubleDoorTile\", \"Horde4_DoubleDoorTile\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"TypicalRoom\",\n" +
                "      \"doors_names\": [\"ConnectorDoor\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"UnremarkableRoom\",\n" +
                "      \"doors_names\": [\"ConnectorDoor\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // Niven Reactor\n" +
                "    {\n" +
                "      \"room_name\": \"NIVC_DoubleReactorCore\",\n" +
                "      \"doors_names\": [\"NIV_DoorSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionTileTypeSize\", \"Horde2_HauntedMansionTileTypeSize\", \"Horde3_HauntedMansionTileTypeSize\", \"Horde4_HauntedMansionTileTypeSize\", \"Horde5_HauntedMansionTileTypeSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"NIVC_MegaConnectorRoom\",\n" +
                "      \"doors_names\": [\"NIV_DoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // The Rubber Rooms\n" +
                "    {\n" +
                "      \"room_name\": \"RubberRRRRRRRRRRoom\",\n" +
                "      \"doors_names\": [\"DoorFrameDoor\"],\n" +
                "      \"weight\": 4,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // Toy Store\n" +
                "    {\n" +
                "      \"room_name\": \"StRoom3\",\n" +
                "      \"doors_names\": [\"AtDoorFrameDoor\", \"StDoorFrameDoorInterior\"],\n" +
                "      \"weight\": 4,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithCover\", \"Horde2_WarehouseSizeWithCover\", \"Horde3_WarehouseSizeWithCover\", \"Horde4_WarehouseSizeWithCover\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"StRoom\",\n" +
                "      \"doors_names\": [\"AtDoorFrameDoor\", \"StDoorFrameDoorInterior\"],\n" +
                "      \"weight\": 3,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithCover\", \"Horde2_WarehouseSizeWithCover\", \"Horde3_WarehouseSizeWithCover\", \"Horde4_WarehouseSizeWithCover\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"StRoom1\",\n" +
                "      \"doors_names\": [\"AtDoorFrameDoor\", \"StDoorFrameDoorInterior\"],\n" +
                "      \"weight\": 3,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"StEndRoom\",\n" +
                "      \"doors_names\": [\"AtDoorFrameDoor\", \"StDoorFrameDoorInterior\"],\n" +
                "      \"weight\": 3,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"StRoom2\",\n" +
                "      \"doors_names\": [\"AtDoorFrameDoor\", \"StDoorFrameDoorInterior\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionTileTypeSize\", \"Horde2_HauntedMansionTileTypeSize\", \"Horde3_HauntedMansionTileTypeSize\", \"Horde4_HauntedMansionTileTypeSize\", \"Horde5_HauntedMansionTileTypeSize\"]\n" +
                "    },\n" +
                "    // Yavin IV\n" +
                "    {\n" +
                "      \"room_name\": \"LargeRoom\",\n" +
                "      \"doors_names\": [\"DoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionLibrarySize\", \"Horde2_HauntedMansionLibrarySize\", \"Horde3_HauntedMansionLibrarySize\", \"Horde4_HauntedMansionLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"LargeRoom2\",\n" +
                "      \"doors_names\": [\"DoorSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionLibrarySize\", \"Horde2_HauntedMansionLibrarySize\", \"Horde3_HauntedMansionLibrarySize\", \"Horde4_HauntedMansionLibrarySize\"]\n" +
                "    },\n" +
                "    // CastleGrounds\n" +
                "    {\n" +
                "      \"room_name\": \"HMCRoom\",\n" +
                "      \"doors_names\": [\"DoorWoodSpawn\", \"DoorSlidingSpawn\", \"DoorMetalSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"BasementPaintingRoom1\",\n" +
                "      \"doors_names\": [\"DoorWoodSpawn\", \"DoorSlidingSpawn\", \"DoorMetalSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionLibrarySize\", \"Horde2_HauntedMansionLibrarySize\", \"Horde3_HauntedMansionLibrarySize\", \"Horde4_HauntedMansionLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"MirrorRoom 1\",\n" +
                "      \"doors_names\": [\"DoorWoodSpawn\", \"DoorSlidingSpawn\", \"DoorMetalSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionLibrarySize\", \"Horde2_HauntedMansionLibrarySize\", \"Horde3_HauntedMansionLibrarySize\", \"Horde4_HauntedMansionLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"UpperFloorHub\",\n" +
                "      \"doors_names\": [\"DoorWoodSpawn\", \"DoorSlidingSpawn\", \"DoorMetalSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_HauntedMansionLibrarySize\", \"Horde2_HauntedMansionLibrarySize\", \"Horde3_HauntedMansionLibrarySize\", \"Horde4_HauntedMansionLibrarySize\"]\n" +
                "    },\n" +
                "    // CircusFacility\n" +
                "    {\n" +
                "      \"room_name\": \"Circus_4x4BigStairTile\",\n" +
                "      \"doors_names\": [\"CircusDoorMapSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Circus_MediumRoomHallway1B\",\n" +
                "      \"doors_names\": [\"CircusDoorMapSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // Liminal Pool Rooms\n" +
                "    {\n" +
                "      \"room_name\": \"DoublePoolsRoom\",\n" +
                "      \"doors_names\": [\"SpawnDoorObjectThingy\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_MediumLibrarySize\", \"Horde2_MediumLibrarySize\", \"Horde3_MediumLibrarySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"TheBiggestOne\",\n" +
                "      \"doors_names\": [\"SpawnDoorObjectThingy\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"ThatOneStaircaseRoom\",\n" +
                "      \"doors_names\": [\"SpawnDoorObjectThingy\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"TheRoomRoom\",\n" +
                "      \"doors_names\": [\"SpawnDoorObjectThingy\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\"]\n" +
                "    },\n" +
                //"    // Museum\n" +
                //"    {\n" +
                //"      \"room_name\": \"MuseumRoom2\",\n" +
                //"      \"doors_names\": [\"Door0Spawn\", \"Door1Spawn\"],\n" +
                //"      \"weight\": 1,\n" +
                //"      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                //"    },\n" +
                //"    // Mausoleum\n" +
                //"    {\n" +
                //"      \"room_name\": \"MausoleumStart\",\n" +
                //"      \"doors_names\": [\"Door2Spawn\", \"Door3Spawn\"],\n" +
                //"      \"weight\": 1,\n" +
                //"      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                //"    },\n" +
                //"    {\n" +
                //"      \"room_name\": \"MausoleumRoom1\",\n" +
                //"      \"doors_names\": [\"Door2Spawn\", \"Door3Spawn\"],\n" +
                //"      \"weight\": 2,\n" +
                //"      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                //"    },\n" +
                "    // Slaughterhouse\n" +
                "    {\n" +
                "      \"room_name\": \"PigStyRoom\",\n" +
                "      \"doors_names\": [\"SlaughterhouseDoorMapSpawn\", \"SlaughterhouseDoorMapSpawn2\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"GrinderRoom\",\n" +
                "      \"doors_names\": [\"SlaughterhouseDoorMapSpawn\", \"SlaughterhouseDoorMapSpawn2\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // Blue Mines\n" +
                "    {\n" +
                "      \"room_name\": \"Blue_FactoryFloor1\",\n" +
                "      \"doors_names\": [\"BlueDoor_Spawner\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Blue_FactoryFloor3\",\n" +
                "      \"doors_names\": [\"BlueDoor_Spawner\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Blue_FactoryFloor4\",\n" +
                "      \"doors_names\": [\"BlueDoor_Spawner\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_LittleRoomWithoutCoverSize\", \"Horde2_LittleRoomWithoutCoverSize\", \"Horde3_LittleRoomWithoutCoverSize\", \"Horde4_LittleRoomWithoutCoverSize\", \"Horde5_LittleRoomWithoutCoverSize\"]\n" +
                "    },\n" +
                "    // Gray Apartments\n" +
                "    {\n" +
                "      \"room_name\": \"GrayApparatusRoom\",\n" +
                "      \"doors_names\": [\"GrayDoor_Spawner\", \"GrayDoubleDoor_Spawner\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"GrayOfficePowerRoom\",\n" +
                "      \"doors_names\": [\"GrayDoor_Spawner\", \"GrayDoubleDoor_Spawner\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    // Black Mesa\n" +
                "    {\n" +
                "      \"room_name\": \"Quarantine 4 Door Complex Room\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Military Storage Room 2\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\", \"Horde_WarehouseSizeBlackMesa\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Military Cafeteria\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 3,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\", \"Horde_WarehouseSizeBlackMesa\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Military Storage Room Large\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithoutCover\", \"Horde2_WarehouseSizeWithoutCover\", \"Horde3_WarehouseSizeWithoutCover\", \"Horde4_WarehouseSizeWithoutCover\", \"Horde_WarehouseSizeBlackMesa\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Office Super Complex\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 3,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithCover\", \"Horde2_WarehouseSizeWithCover\", \"Horde3_WarehouseSizeWithCover\", \"Horde4_WarehouseSizeWithCover\", \"Horde_WarehouseSizeBlackMesa\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Brick Mega Warehouse\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 2,\n" +
                "      \"hordes\": [\"Horde1_WarehouseSizeWithCover\", \"Horde2_WarehouseSizeWithCover\", \"Horde3_WarehouseSizeWithCover\", \"Horde4_WarehouseSizeWithCover\", \"Horde_WarehouseSizeBlackMesa\"]\n" +
                "    },\n" +
                "    {\n" +
                "      \"room_name\": \"Resonance Chamber\",\n" +
                "      \"doors_names\": [\"QuarantineDoorMapSpawn\", \"DoubleGreyDoorMapSpawn\", \"SingleMetalDoorMapSpawn\", \"DoubleGlassSlidingDoorMainSpawn\", \"LabSingleDoorMapSpawn\", \"LabDoubleDoorMapSpawn\", \"DoubleRedDoorMapSpawn\", \"SingleRedDoorMapSpawn\", \"SingleGlassSlidingDoorResonanceSpawn\", \"DoubleGlassSlidingDoorResonanceSpawn\"],\n" +
                "      \"weight\": 1,\n" +
                "      \"hordes\": [\"Horde1_FacilitySize\", \"Horde2_FacilitySize\", \"Horde3_FacilitySize\", \"Horde4_FacilitySize\"]\n" +
                "    }\n" +
                "  ]\n" +
                "}";

        public static void LoadJSON()
        {
            string fullFilePath = Path.Combine(Paths.ConfigPath, FilePath);
            if (File.Exists(fullFilePath))
            {
                try
                {
                    string json = File.ReadAllText(fullFilePath);
                    JObject parsedJson = JObject.Parse(json);
                    if (!ValidateJsonStructure(parsedJson))
                        RenameOldFile(fullFilePath);
                }
                catch (Exception)
                {
                    RenameOldFile(fullFilePath);
                }
            }
            else
            {
                File.WriteAllText(fullFilePath, Get());
            }

            using (var reader = new StreamReader(Path.Combine(Paths.ConfigPath, FilePath)))
            {
                List<SurvivalHordeMapping> hordesMapping = LoadHordes();
                foreach (SurvivalHordeMapping horde in hordesMapping)
                {
                    Dictionary<int, EnemyType> enemiesSpawn = new Dictionary<int, EnemyType>();
                    foreach (SurvivalEnemySpawnMapping enemySpawnMapping in horde.EnemiesSpawn)
                    {
                        EnemyType enemyType = SawTapes.allEnemies.FirstOrDefault(e => !e.ToString().Contains("Outside") && e.enemyName.Equals(enemySpawnMapping.Enemy))
                            ?? SawTapes.allEnemies.FirstOrDefault(e => e.enemyName.Equals(enemySpawnMapping.Enemy));
                        if (enemyType != null)
                            enemiesSpawn.Add(enemySpawnMapping.Time, enemyType);
                        else
                            SawTapes.mls.LogWarning($"The enemy {enemySpawnMapping.Enemy} could not be found from the ST.survival_game.json file.");
                    }
                    SawTapes.hordes.Add(new SurvivalHorde(horde.HordeName, horde.GameDuration, horde.BillyValue, horde.MinHour, horde.MaxHour, enemiesSpawn));
                }

                List<SurvivalRoomMapping> roomsMapping = LoadRooms();
                foreach (SurvivalRoomMapping room in roomsMapping)
                {
                    List<SurvivalHorde> hordes = new List<SurvivalHorde>();
                    foreach (string hordeName in room.Hordes)
                    {
                        SurvivalHorde horde = SawTapes.hordes.FirstOrDefault(h => h.HordeName.Equals(hordeName));
                        if (horde != null)
                            hordes.Add(horde);
                        else
                            SawTapes.mls.LogWarning($"The horde {hordeName} could not be found from the file configuration.");
                    }
                    SawTapes.rooms.Add(new SurvivalRoom($"{room.RoomName}(Clone)", room.DoorsNames, room.Weight, hordes));
                }
            }
        }

        public static bool ValidateJsonStructure(JObject parsedJson)
        {
            bool isValid = true;
            if (parsedJson["hordes"] == null)
            {
                SawTapes.mls.LogWarning("Missing 'hordes' array in the JSON.");
                isValid = false;
            }

            if (parsedJson["rooms"] == null)
            {
                SawTapes.mls.LogWarning("Missing 'rooms' array in the JSON.");
                isValid = false;
            }

            // Validation des éléments dans 'hordes'
            foreach (var horde in parsedJson["hordes"])
            {
                if (horde["horde_name"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'horde_name' in one of the horde objects.");
                    isValid = false;
                }
                if (horde["game_duration"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'game_duration' in one of the horde objects.");
                    isValid = false;
                }
                if (horde["billy_value"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'billy_value' in one of the horde objects.");
                    isValid = false;
                }
                if (horde["min_hour"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'min_hour' in one of the horde objects.");
                    isValid = false;
                }
                if (horde["max_hour"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'max_hour' in one of the horde objects.");
                    isValid = false;
                }
                if (horde["enemies_spawn"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'enemies_spawn' array in one of the horde objects.");
                    isValid = false;
                }
            }

            // Validation des éléments dans 'rooms'
            foreach (var room in parsedJson["rooms"])
            {
                if (room["room_name"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'room_name' in one of the room objects.");
                    isValid = false;
                }
                if (room["doors_names"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'doors_names' array in one of the room objects.");
                    isValid = false;
                }
                if (room["weight"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'weight' in one of the room objects.");
                    isValid = false;
                }
                if (room["hordes"] == null)
                {
                    SawTapes.mls.LogWarning("Missing 'hordes' array in one of the room objects.");
                    isValid = false;
                }
            }
            return isValid;
        }

        public static void RenameOldFile(string filePath)
        {
            string backupFilePath = filePath + ".old";
            if (File.Exists(backupFilePath))
                File.Delete(backupFilePath);
            File.Move(filePath, backupFilePath);
            File.WriteAllText(filePath, Get());
        }

        public static List<SurvivalHordeMapping> LoadHordes()
        {
            string json = File.ReadAllText(FilePath);
            return JObject.Parse(json)["hordes"].ToObject<List<SurvivalHordeMapping>>();
        }

        public static List<SurvivalRoomMapping> LoadRooms()
        {
            string json = File.ReadAllText(FilePath);
            return JObject.Parse(json)["rooms"].ToObject<List<SurvivalRoomMapping>>();
        }
    }
}
