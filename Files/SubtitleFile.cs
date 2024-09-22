using BepInEx;
using Newtonsoft.Json.Linq;
using SawTapes.Files.Values;
using System.Collections.Generic;
using System.IO;

namespace SawTapes.Files
{
    internal class SubtitleFile
    {
        public static string FilePath = Path.Combine(Paths.ConfigPath, "ST.subtitles.json");
        public static List<SubtitleMapping> survivalGameSubtitles;
        public static List<SubtitleMapping> billySubtitles;

        public static string Get()
        {
            return "{\n" +
                "  \"survival_game\": [\n" +
                "    {\n" +
                "      \"timestamp\": 0,\n" +
                "      // Hello worker\n" +
                "      \"text\": \"Bonjour travailleur\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 1.2,\n" +
                "      // Until now\n" +
                "      \"text\": \"Jusqu'à maintenant\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 2.15,\n" +
                "      // you have shown cowardice in crucial moments\n" +
                "      \"text\": \"vous avez fait preuve de lâcheté dans les moments cruciaux\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 4.56,\n" +
                "      // abandoning those who relied on you and avoiding your responsibilities\n" +
                "      \"text\": \"abandonnant ceux qui comptaient sur vous et évitant vos responsabilités\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 9.05,\n" +
                "      // You now find yourself in a sealed room\n" +
                "      \"text\": \"Vous vous trouvez maintenant dans une pièce scellée\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 11.65,\n" +
                "      // faced with a test of bravery\n" +
                "      \"text\": \"et vous êtes confronté à une épreuve de bravoure\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 13.84,\n" +
                "      // Terrifying and unpredictable creatures\n" +
                "      \"text\": \"Des créatures terrifiantes et imprévisibles\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 16.69,\n" +
                "      // have been released here\n" +
                "      \"text\": \"ont été relâchées ici\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 18.11,\n" +
                "      // If you manage to survive until the end of the allotted time\n" +
                "      \"text\": \"Si vous parvenez à survivre jusqu'à la fin du temps imparti\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 21.46,\n" +
                "      // you will have overcome your own cowardice and proven\n" +
                "      \"text\": \"vous aurez surmonté votre propre lâcheté et prouvé\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 24.47,\n" +
                "      // that you are capable of standing firm in critical moments\n" +
                "      \"text\": \"que vous êtes capable de vous affirmer dans les moments critiques\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 28.6,\n" +
                "      // Let the game begin\n" +
                "      \"text\": \"Que le jeu commence\"\n" +
                "    }\n" +
                "  ],\n" +
                "  \"billy_announcement\": [\n" +
                "    {\n" +
                "      \"timestamp\": 0,\n" +
                "      // Congratulations\n" +
                "      \"text\": \"Félicitations\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 1.37,\n" +
                "      // you've survived\n" +
                "      \"text\": \"vous avez survécu\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 2.71,\n" +
                "      // Today, you have emerged from this trial changed\n" +
                "      \"text\": \"Aujourd'hui, vous êtes sorti de cette épreuve changé\"\n" +
                "    },\n" +
                "    {\n" +
                "      \"timestamp\": 6.13,\n" +
                "      // But remember... life is made of choices\n" +
                "      \"text\": \"Mais rappelez-vous… la vie est faite de choix\"\n" +
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
                survivalGameSubtitles = LoadSurvivalGameSubtitles();
                billySubtitles = LoadBillyAnnouncementSubtitles();
            }
        }

        public static List<SubtitleMapping> LoadSurvivalGameSubtitles()
        {
            string json = File.ReadAllText(FilePath);
            return JObject.Parse(json)["survival_game"].ToObject<List<SubtitleMapping>>();
        }

        public static List<SubtitleMapping> LoadBillyAnnouncementSubtitles()
        {
            string json = File.ReadAllText(FilePath);
            return JObject.Parse(json)["billy_announcement"].ToObject<List<SubtitleMapping>>();
        }
    }
}
