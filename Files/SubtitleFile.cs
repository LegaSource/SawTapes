using BepInEx;
using Newtonsoft.Json.Linq;
using SawTapes.Files.Values;
using System;
using System.Collections.Generic;
using System.IO;

namespace SawTapes.Files;

public class SubtitleFile
{
    public static string FilePath = Path.Combine(Paths.ConfigPath, "ST.subtitles.json");
    public static HashSet<SubtitleMapping> survivalGameSubtitles;
    public static HashSet<SubtitleMapping> huntingGameSubtitles;
    public static HashSet<SubtitleMapping> escapeGameSubtitles;
    public static HashSet<SubtitleMapping> explosiveGameSubtitles;
    public static HashSet<SubtitleMapping> billyAnnouncementSubtitles;
    public static HashSet<SubtitleMapping> billyBathroomSubtitles;

    public static string Get()
        => "{\n" +
            "  \"survival_game\": [\n" +
            "    {\n" +
            "      \"timestamp\": 0,\n" +
            "      // Hello workers\n" +
            "      \"text\": \"Bonjour travailleurs\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 1.2,\n" +
            "      // All your life, you have fled from responsibility\n" +
            "      \"text\": \"Toute votre vie, vous avez fui toute responsabilité\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 4.01,\n" +
            "      // sacrificing others to ensure your own survival\n" +
            "      \"text\": \"sacrifiant les autres pour assurer votre propre survie\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 7.59,\n" +
            "      // But today, there is no escape\n" +
            "      \"text\": \"Mais aujourd’hui, il n’y a plus d’échappatoire\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 9.94,\n" +
            "      // You are being hunted and you will inevitably draw the attention of the creatures lurking here\n" +
            "      \"text\": \"Vous êtes traqués et vous attirerez inévitablement l’attention des créatures qui rôdent ici\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 15.11,\n" +
            "      // A single eye may divert their gaze… but at the cost of another sacrifice\n" +
            "      \"text\": \"Un œil peut détourner leur regard… mais au prix d’un autre sacrifice\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 19.56,\n" +
            "      // Let the game begin\n" +
            "      \"text\": \"Que le jeu commence\"\n" +
            "    }\n" +
            "  ],\n" +
            "  \"hunting_game\": [\n" +
            "    {\n" +
            "      \"timestamp\": 0,\n" +
            "      // Hello workers\n" +
            "      \"text\": \"Bonjour travailleurs\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 1.25,\n" +
            "      // All your life, you have spilled blood without consequences\n" +
            "      \"text\": \"Toute votre vie, vous avez fait couler le sang sans conséquences\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 4.91,\n" +
            "      // finding pleasure in the pain of others\n" +
            "      \"text\": \"trouvant du plaisir dans la douleur des autres\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 7.44,\n" +
            "      // Today, that brutality has become your only way out\n" +
            "      \"text\": \"Aujourd’hui, cette brutalité est devenue votre seule issue\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 11.41,\n" +
            "      // A key is your salvation, but it lies within the guts of beasts you must hunt\n" +
            "      \"text\": \"Une clé est votre salut, mais elle repose dans les entrailles de bêtes que vous devrez traquer\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 16.63,\n" +
            "      // Live or die, the choice is yours\n" +
            "      \"text\": \"Vivre ou mourir, à vous de choisir\"\n" +
            "    }\n" +
            "  ],\n" +
            "  \"escape_game\": [\n" +
            "    {\n" +
            "      \"timestamp\": 0,\n" +
            "      // Hello workers\n" +
            "      \"text\": \"Bonjour travailleurs\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 1.27,\n" +
            "      // Until now\n" +
            "      \"text\": \"Jusqu'à maintenant\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 2.21,\n" +
            "      // you have made decisions that impacted the lives of others without facing the consequences yourselves\n" +
            "      \"text\": \"vous avez pris des décisions qui ont affecté la vie des autres sans en subir vous-même les conséquences\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 8.43,\n" +
            "      // But today, your lives are bound together\n" +
            "      \"text\": \"Mais aujourd'hui, vos vies sont liées\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 11.16,\n" +
            "      // The chains that tether you represent your ability—or inability—to work as one\n" +
            "      \"text\": \"Les chaînes qui vous attachent représentent votre capacité, ou votre incapacité, à travailler ensemble\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 16.85,\n" +
            "      // Path to freedom is littered with deadly traps\n" +
            "      \"text\": \"Votre chemin vers la liberté est jonché de pièges mortels\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 20.41,\n" +
            "      // Reach the saw before it's too late, or prepare to perish together\n" +
            "      \"text\": \"Atteignez la scie avant qu'il ne soit trop tard, ou préparez-vous à périr ensemble\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 24.93,\n" +
            "      // The choice is yours\n" +
            "      \"text\": \"Le choix est à vous\"\n" +
            "    }\n" +
            "  ],\n" +
            "  \"explosive_game\": [\n" +
            "    {\n" +
            "      \"timestamp\": 0,\n" +
            "      // Hello workers\n" +
            "      \"text\": \"Bonjour travailleurs\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 1.24,\n" +
            "      // You’ve played with fire your entire lives\n" +
            "      \"text\": \"Vous avez joué avec le feu toute votre vie\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 3.85,\n" +
            "      // taking reckless risks without ever facing the consequences\n" +
            "      \"text\": \"prenant des risques inconsidérés sans jamais en assumer les conséquences\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 7.96,\n" +
            "      // Today, danger rests in your hands\n" +
            "      \"text\": \"Aujourd'hui, le danger repose entre vos mains\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 10.74,\n" +
            "      // One of you holds an unstable bomb\n" +
            "      \"text\": \"L'un d'entre vous détient une bombe instable\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 13.04,\n" +
            "      // Pass it on… or perish with it\n" +
            "      \"text\": \"Transmettez-la... ou périssez avec elle\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 15.33,\n" +
            "      // But remember, every action comes at a price\n" +
            "      \"text\": \"Mais n'oubliez pas que chaque action a un prix\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 18.66,\n" +
            "      // Find the container… if you hope to survive\n" +
            "      \"text\": \"Trouvez le conteneur, si vous espérez survivre\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 21.37,\n" +
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
            "  ],\n" +
            "  \"billy_bathroom\": [\n" +
            "    {\n" +
            "      \"timestamp\": 0,\n" +
            "      // Hello worker\n" +
            "      \"text\": \"Bonjour travailleur\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 1.1,\n" +
            "      // You have survived many trials\n" +
            "      \"text\": \"Tu as survécu à de nombreuses épreuves\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 3.13,\n" +
            "      // You may believe this grants you respite… but today, a new opportunity lies before you\n" +
            "      \"text\": \"Tu crois peut-être que cela t’offre un répit… mais aujourd’hui, c’est une nouvelle opportunité qui s’offre à toi\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 8.78,\n" +
            "      // In this room hides the object that will set you free\n" +
            "      \"text\": \"Dans cette pièce se cache ce dont tu as besoin pour te libérer\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 12.08,\n" +
            "      // Yet another path stands open to you: the blade before you offers immediate freedom… at the cost of your flesh\n" +
            "      \"text\": \"Mais une autre possibilité s'offre à toi, la lame devant toi peut t’offrir la liberté immédiate… au prix de ta chair\"\n" +
            "    },\n" +
            "    {\n" +
            "      \"timestamp\": 19.08,\n" +
            "      // The choice is yours\n" +
            "      \"text\": \"Fais ton choix\"\n" +
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

        using StreamReader reader = new StreamReader(Path.Combine(Paths.ConfigPath, FilePath));
        survivalGameSubtitles = LoadSurvivalGameSubtitles();
        huntingGameSubtitles = LoadHuntingGameSubtitles();
        escapeGameSubtitles = LoadEscapeGameSubtitles();
        explosiveGameSubtitles = LoadExplosiveGameSubtitles();
        billyAnnouncementSubtitles = LoadBillyAnnouncementSubtitles();
        billyBathroomSubtitles = LoadBillyBathroomSubtitles();
    }

    public static bool ValidateJsonStructure(JObject parsedJson)
    {
        List<string> expectedKeys = ["survival_game", "hunting_game", "escape_game", "explosive_game", "billy_announcement", "billy_bathroom"];
        foreach (string key in expectedKeys)
        {
            if (parsedJson[key] == null)
                return false; // Si une clé attendue est manquante

            if (parsedJson[key] is JArray array)
            {
                foreach (JToken item in array)
                {
                    if (item["timestamp"] == null || item["text"] == null)
                        return false; // Vérifier que chaque élément a bien les clés "timestamp" et "text"
                }
            }
        }
        return true;
    }

    public static void RenameOldFile(string filePath)
    {
        string backupFilePath = filePath + ".old";
        if (File.Exists(backupFilePath))
            File.Delete(backupFilePath);
        File.Move(filePath, backupFilePath);
        File.WriteAllText(filePath, Get());
    }

    public static HashSet<SubtitleMapping> LoadSurvivalGameSubtitles()
    {
        string json = File.ReadAllText(FilePath);
        return JObject.Parse(json)["survival_game"].ToObject<HashSet<SubtitleMapping>>();
    }

    public static HashSet<SubtitleMapping> LoadHuntingGameSubtitles()
    {
        string json = File.ReadAllText(FilePath);
        return JObject.Parse(json)["hunting_game"].ToObject<HashSet<SubtitleMapping>>();
    }

    public static HashSet<SubtitleMapping> LoadEscapeGameSubtitles()
    {
        string json = File.ReadAllText(FilePath);
        return JObject.Parse(json)["escape_game"].ToObject<HashSet<SubtitleMapping>>();
    }

    public static HashSet<SubtitleMapping> LoadExplosiveGameSubtitles()
    {
        string json = File.ReadAllText(FilePath);
        return JObject.Parse(json)["explosive_game"].ToObject<HashSet<SubtitleMapping>>();
    }

    public static HashSet<SubtitleMapping> LoadBillyAnnouncementSubtitles()
    {
        string json = File.ReadAllText(FilePath);
        return JObject.Parse(json)["billy_announcement"].ToObject<HashSet<SubtitleMapping>>();
    }

    public static HashSet<SubtitleMapping> LoadBillyBathroomSubtitles()
    {
        string json = File.ReadAllText(FilePath);
        return JObject.Parse(json)["billy_bathroom"].ToObject<HashSet<SubtitleMapping>>();
    }
}
