﻿using System.Text.Json.Serialization;

namespace Plag.Backend.Entities
{
    public class SubmissionEntity : Models.Submission
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("tokens")]
        public byte[] Tokens { get; set; }
    }
}