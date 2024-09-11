﻿using Newtonsoft.Json;

namespace SawTapes.Files.Values
{
    internal class SubtitleMapping
    {
        public string GameName { get; internal set; }

        [JsonProperty("timestamp")]
        public float Timestamp { get; internal set; }

        [JsonProperty("text")]
        public string Text { get; internal set; }
    }
}